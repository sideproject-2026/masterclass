using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Lms.MigrationService;

/// <summary>
/// Applies pending migrations for every registered module DbContext, then stops the host.
/// </summary>
/// <remarks>
/// Migrating at API startup races across replicas — two instances can attempt the same
/// migration at once. This runs as a discrete job that must finish before the API rolls out
/// (artifacts/design/01-architecture.md §7).
/// <para>
/// Registering a new module's DbContext in <c>Program.cs</c> is all it takes; this runner
/// discovers whatever is in the container.
/// </para>
/// </remarks>
internal sealed partial class MigrationRunner(
    IServiceProvider services,
    IHostApplicationLifetime lifetime,
    ILogger<MigrationRunner> logger) : BackgroundService
{
    internal const string ActivitySourceName = "Lms.MigrationService";
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = ActivitySource.StartActivity("Apply migrations", ActivityKind.Client);

        try
        {
            using var scope = services.CreateScope();
            var contexts = scope.ServiceProvider.GetServices<DbContext>().ToList();

            if (contexts.Count == 0)
            {
                LogNoContexts();
            }

            foreach (var context in contexts)
            {
                await MigrateAsync(context, stoppingToken);
            }

            LogAllApplied(contexts.Count);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            LogFailed(ex);

            // Non-zero exit so the deploy pipeline stops here rather than rolling out an API
            // against a schema that was never migrated.
            Environment.ExitCode = 1;
        }

        lifetime.StopApplication();
    }

    private async Task MigrateAsync(DbContext context, CancellationToken ct)
    {
        var name = context.GetType().Name;

        var pending = (await context.Database.GetPendingMigrationsAsync(ct)).ToList();
        if (pending.Count == 0)
        {
            LogUpToDate(name);
            return;
        }

        // Formatted into a local rather than inline, so the log argument is a cheap read
        // (CA1873). This job runs once per deploy — the allocation is not worth guarding.
        var pendingList = string.Join(", ", pending);
        LogApplying(name, pending.Count, pendingList);

        // The strategy handles transient connection faults; a fresh database may still be
        // accepting its first connections when this runs.
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(() => context.Database.MigrateAsync(ct));

        LogMigrated(name);
    }

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "No DbContext was registered. Nothing to migrate — is a module missing from Program.cs?")]
    private partial void LogNoContexts();

    [LoggerMessage(Level = LogLevel.Information, Message = "All migrations applied. {Count} context(s).")]
    private partial void LogAllApplied(int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Context}: already up to date.")]
    private partial void LogUpToDate(string context);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "{Context}: applying {Count} migration(s) — {Migrations}")]
    private partial void LogApplying(string context, int count, string migrations);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Context}: done.")]
    private partial void LogMigrated(string context);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Migration failed. The API must not start against this database.")]
    private partial void LogFailed(Exception exception);
}
