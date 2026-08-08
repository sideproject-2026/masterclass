using Lms.Modules.Notifications.Domain;
using Lms.SharedKernel.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lms.Modules.Notifications.Infrastructure;

/// <summary>
/// One DbContext per module, one schema per module — this is what makes the
/// "no cross-module foreign keys" rule physically true rather than aspirational
/// (artifacts/design/01-architecture.md §4).
/// </summary>
public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
    : DbContext(options)
{
    public const string Schema = "notifications";

    /// <summary>Separate history table per module, so modules version independently.</summary>
    public const string MigrationsHistoryTable = "__ef_migrations_history_notifications";

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);

        configurationBuilder.ApplyStronglyTypedIdConventions();
    }
}
