using Lms.Modules.Catalog.Domain;
using Lms.SharedKernel.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lms.Modules.Catalog.Infrastructure;

/// <summary>
/// One DbContext per module, one schema per module (01-architecture.md §4).
/// </summary>
public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public const string Schema = "catalog";
    public const string MigrationsHistoryTable = "__ef_migrations_history_catalog";

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Chapter> Chapters => Set<Chapter>();

    public DbSet<Lesson> Lessons => Set<Lesson>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);

        configurationBuilder.ApplyStronglyTypedIdConventions();
    }
}
