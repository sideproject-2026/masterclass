using Lms.Modules.Identity.Domain;
using Lms.SharedKernel.Authorization;
using Lms.SharedKernel.Time;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lms.Modules.Identity.Infrastructure;

/// <summary>
/// Creates the first <c>Admin</c> from configuration, and only from configuration.
/// </summary>
/// <remarks>
/// <b>There is no default password and no fallback.</b> If either setting is missing this does
/// nothing and says so. An <c>Admin</c> can mint instructors, who can publish to every student
/// on the platform, so a well-known seeded credential would be a total compromise of the content
/// pipeline — see artifacts/design/04-adr-authentication.md §7.
/// <para>
/// Idempotent: an existing account is left exactly as it is. In particular this never resets a
/// password, so someone who can edit configuration cannot use a redeploy to take over an
/// existing admin account.
/// </para>
/// </remarks>
internal sealed partial class AdminSeeder(
    IServiceProvider services,
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<AdminSeeder> logger) : IHostedService
{
    public const string SectionName = "Admin";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var email = configuration[$"{SectionName}:Email"];
        var password = configuration[$"{SectionName}:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            LogNotConfigured(environment.EnvironmentName);
            return;
        }

        using var scope = services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();

        if (await users.FindByEmailAsync(email) is not null)
        {
            LogAlreadyExists();
            return;
        }

        var admin = AppUser.Create(email, "Administrator", clock.UtcNow);
        admin.EmailConfirmed = true;

        try
        {
            var created = await users.CreateAsync(admin, password);
            if (!created.Succeeded)
            {
                // Most likely a password below the configured minimum. The process must still
                // start: an API that refuses to boot because seeding failed is a worse outage
                // than a missing admin account.
                LogNotCreated(string.Join(" ", created.Errors.Select(e => e.Description)));
                return;
            }

            // Student as well as Admin. "Every registered user holds Student"
            // (02-domain-model.md §2) is an invariant the authorisation model leans on:
            // AuthPolicies.Student is the "we know who you are" policy and is implemented as
            // RequireRole(Student), so an admin without it is refused /api/me — which
            // contradicts the matrix in 03-api-design.md §7. Registration grants it
            // automatically; this seeder creates a user directly and so must do it here.
            var assigned = await users.AddToRolesAsync(admin, [Roles.Student, Roles.Admin]);
            if (assigned.Succeeded)
            {
                LogSeeded(email);
            }
            else
            {
                LogNotCreated(string.Join(" ", assigned.Errors.Select(e => e.Description)));
            }
        }
        catch (DbUpdateException)
        {
            // Another replica created the account between the lookup above and this insert.
            // Same check-then-act race as RoleSeeder, and it surfaces the same way: a unique
            // index violation thrown from SaveChangesAsync, not a failed IdentityResult.
            LogAlreadyExists();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "No Admin:Email/Admin:Password configured for '{Environment}'; no admin seeded.")]
    private partial void LogNotConfigured(string environment);

    [LoggerMessage(Level = LogLevel.Debug, Message = "The configured admin account already exists.")]
    private partial void LogAlreadyExists();

    [LoggerMessage(Level = LogLevel.Information, Message = "Seeded the initial admin {Email}.")]
    private partial void LogSeeded(string email);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Admin account not seeded: {Reason}")]
    private partial void LogNotCreated(string reason);
}
