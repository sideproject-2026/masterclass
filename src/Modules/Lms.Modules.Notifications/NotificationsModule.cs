using Lms.Modules.Notifications.Infrastructure;
using Lms.SharedKernel.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.Modules.Notifications;

/// <summary>
/// The Notifications module's only public surface to the host. Adding a module is one line
/// in each of two places in Program.cs — see artifacts/design/01-architecture.md §3.1.
/// </summary>
public static class NotificationsModule
{
    /// <summary>Connection-string name injected by the Aspire AppHost.</summary>
    public const string ConnectionName = "lmsdb";

    public static IServiceCollection AddNotificationsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<NotificationsDbContext>((_, options) =>
        {
            options
                .UseNpgsql(
                    configuration.GetConnectionString(ConnectionName),
                    npgsql => npgsql.UseLmsMigrationHistory(
                        NotificationsDbContext.Schema,
                        NotificationsDbContext.MigrationsHistoryTable))
                .UseLmsConventions();
        });

        return services;
    }

    public static IEndpointRouteBuilder MapNotificationsEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // Notifications reacts to events; it exposes no HTTP surface.
        return app;
    }
}
