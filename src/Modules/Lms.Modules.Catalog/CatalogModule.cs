using Lms.Modules.Catalog.Infrastructure;
using Lms.SharedKernel.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.Modules.Catalog;

/// <summary>
/// The Catalog module's only public surface to the host. Adding a module is one line
/// in each of two places in Program.cs — see artifacts/design/01-architecture.md §3.1.
/// </summary>
public static class CatalogModule
{
    private const string ConnectionName = "lmsdb";

    /// <summary>
    /// Registers the DbContext and nothing else.
    /// </summary>
    /// <remarks>
    /// Split from <see cref="AddCatalogModule"/> so <c>Lms.MigrationService</c> can take the
    /// schema without also wiring handlers it will never run — the pattern established in
    /// <c>A-1</c>.
    /// </remarks>
    public static IServiceCollection AddCatalogPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<CatalogDbContext>((_, options) =>
        {
            options
                .UseNpgsql(
                    configuration.GetConnectionString(ConnectionName),
                    npgsql => npgsql.UseLmsMigrationHistory(
                        CatalogDbContext.Schema,
                        CatalogDbContext.MigrationsHistoryTable))
                .UseLmsConventions();
        });

        return services;
    }

    public static IServiceCollection AddCatalogModule(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services.AddCatalogPersistence(configuration);

    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // Route groups arrive with S-2. The domain and its schema land first (S-1) so the
        // endpoints have something to be endpoints for.
        return app;
    }
}
