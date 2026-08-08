using Lms.Modules.Identity.Domain;
using Lms.SharedKernel.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lms.Modules.Identity.Infrastructure;

/// <summary>
/// Ensures the three roles exist. Idempotent, so it is safe on every start.
/// </summary>
/// <remarks>
/// Roles are reference data, not schema, so this is seeding rather than a migration — and
/// unlike a migration it must tolerate running concurrently on several replicas.
/// The names come from <see cref="Roles"/> in SharedKernel; they are not redeclared here.
/// </remarks>
internal sealed partial class RoleSeeder(
    IServiceProvider services,
    ILogger<RoleSeeder> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

        foreach (var name in Roles.All)
        {
            if (await roles.RoleExistsAsync(name))
            {
                continue;
            }

            var result = await roles.CreateAsync(AppRole.Create(name));

            if (result.Succeeded)
            {
                LogRoleCreated(name);
            }
            else
            {
                // A concurrent replica winning the race is fine and expected.
                var reason = string.Join(" ", result.Errors.Select(e => e.Description));
                LogRoleNotCreated(name, reason);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [LoggerMessage(Level = LogLevel.Information, Message = "Seeded role {Role}.")]
    private partial void LogRoleCreated(string role);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Role {Role} not created: {Reason}")]
    private partial void LogRoleNotCreated(string role, string reason);
}
