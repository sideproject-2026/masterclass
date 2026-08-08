using Lms.Modules.Identity.Domain;
using Lms.Modules.Identity.Endpoints;
using Lms.Modules.Identity.Features.Login;
using Lms.Modules.Identity.Features.Register;
using Lms.Modules.Identity.Infrastructure;
using Lms.SharedKernel.Messaging;
using Lms.SharedKernel.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Lms.Modules.Identity;

/// <summary>
/// The Identity module's only public surface to the host
/// (artifacts/design/01-architecture.md §3.1).
/// </summary>
public static class IdentityModule
{
    public const string ConnectionName = "lmsdb";

    /// <summary>
    /// The DbContext alone. Used by <c>Lms.MigrationService</c>, which needs the schema but
    /// none of the authentication machinery — and must not fail startup over a signing key
    /// it will never use.
    /// </summary>
    public static IServiceCollection AddIdentityPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<IdentityModuleDbContext>(options =>
        {
            options
                .UseNpgsql(
                    configuration.GetConnectionString(ConnectionName),
                    npgsql => npgsql.UseLmsMigrationHistory(
                        IdentityModuleDbContext.Schema,
                        IdentityModuleDbContext.MigrationsHistoryTable))
                .UseLmsConventions();
        });

        return services;
    }

    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddIdentityPersistence(configuration);

        // ValidateOnStart: a missing or short signing key stops the process here rather than
        // becoming a puzzling 401 the first time somebody logs in.
        services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // No AddDefaultTokenProviders(): those generate email-confirmation and password-reset
        // tokens, which are H-1 (Sprint 29). They also require AddDataProtection(), so adding
        // them now would mean wiring key management for a flow nothing calls.
        services
            .AddIdentityCore<AppUser>(ConfigurePasswordAndLockout)
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<IdentityModuleDbContext>();

        services.AddScoped<JwtTokenService>();
        services.AddHostedService<RoleSeeder>();

        services.AddScoped<
            ICommandHandler<RegisterUserCommand, RegisteredUser>, RegisterUserHandler>();
        services.AddScoped<
            ICommandHandler<LoginUserCommand, AuthTokens>, LoginUserHandler>();

        return services;
    }

    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.MapAuthEndpoints();
    }

    /// <summary>
    /// Length over composition rules. Forcing a symbol and a digit produces <c>Password1!</c>,
    /// which is weaker than a long passphrase and harder to remember
    /// (artifacts/design/04-adr-authentication.md §3.1).
    /// </summary>
    private static void ConfigurePasswordAndLockout(IdentityOptions options)
    {
        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.AllowedForNewUsers = true;

        options.User.RequireUniqueEmail = true;

        // MVP allows login before confirming. Enforcement is H-1, before public registration
        // opens — see 04-adr-authentication.md §7.
        options.SignIn.RequireConfirmedAccount = false;
    }
}
