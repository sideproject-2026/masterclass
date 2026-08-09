using Lms.Modules.Identity.Domain;
using Lms.SharedKernel.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

            await CreateRoleAsync(roles, name);
        }
    }

    /// <summary>
    /// Creates one role, tolerating another replica having created it in between.
    /// </summary>
    /// <remarks>
    /// <c>RoleExistsAsync</c> followed by <c>CreateAsync</c> is check-then-act, so with more
    /// than one replica starting at once both can pass the check and one loses on the unique
    /// index. That loss arrives as a <see cref="DbUpdateException"/> from
    /// <c>SaveChangesAsync</c> — it is <b>not</b> a failed <c>IdentityResult</c>, so returning
    /// the result unexamined would let it escape and take the process down on first deploy.
    /// Found by the integration suite, where several test hosts start simultaneously.
    /// </remarks>
    private async Task CreateRoleAsync(RoleManager<AppRole> roles, string name)
    {
        try
        {
            var result = await roles.CreateAsync(AppRole.Create(name));

            if (result.Succeeded)
            {
                LogRoleCreated(name);
            }
            else
            {
                var reason = string.Join(" ", result.Errors.Select(e => e.Description));
                LogRoleNotCreated(name, reason);
            }
        }
        catch (DbUpdateException)
        {
            // Someone else created it between the check and the insert. That is the desired
            // end state, so it is not an error — but say so, because a seeder that is silently
            // never the winner is worth noticing.
            LogRoleRaceLost(name);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [LoggerMessage(Level = LogLevel.Information, Message = "Seeded role {Role}.")]
    private partial void LogRoleCreated(string role);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Role {Role} not created: {Reason}")]
    private partial void LogRoleNotCreated(string role, string reason);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Role {Role} was created concurrently by another instance.")]
    private partial void LogRoleRaceLost(string role);
}
