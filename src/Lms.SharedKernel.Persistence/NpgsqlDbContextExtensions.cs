using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Lms.SharedKernel.Persistence;

/// <summary>
/// The options every module's DbContext shares. Registered in one place so a module cannot
/// accidentally opt out of snake_case naming or the per-module migrations history table.
/// </summary>
public static class NpgsqlDbContextExtensions
{
    /// <summary>
    /// Applies the house conventions: no-tracking reads by default and snake_case identifiers.
    /// </summary>
    /// <remarks>
    /// Reads dominate and none of them need change tracking, so it is off by default and
    /// command handlers opt back in explicitly — artifacts/design/02-domain-model.md §8.2.
    /// </remarks>
    public static DbContextOptionsBuilder UseLmsConventions(this DbContextOptionsBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution)
            .UseSnakeCaseNamingConvention();
    }

    /// <summary>
    /// Gives the module its own migrations history table inside its own schema, so modules
    /// version independently (artifacts/design/02-domain-model.md §8.1).
    /// </summary>
    public static NpgsqlDbContextOptionsBuilder UseLmsMigrationHistory(
        this NpgsqlDbContextOptionsBuilder builder,
        string schema,
        string historyTable)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.MigrationsHistoryTable(historyTable, schema);
    }
}
