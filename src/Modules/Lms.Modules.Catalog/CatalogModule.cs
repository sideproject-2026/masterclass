using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.Modules.Catalog;

/// <summary>
/// The Catalog module's only public surface to the host. Adding a module is one line
/// in each of two places in Program.cs — see artifacts/design/01-architecture.md §3.1.
/// </summary>
public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Registrations arrive with the module's first feature.
        return services;
    }

    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // Route groups arrive with the module's first endpoint.
        return app;
    }
}
