using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lms.SharedKernel.Persistence;

/// <summary>
/// Optimistic concurrency using PostgreSQL's <c>xmin</c> system column.
/// </summary>
/// <remarks>
/// Every PostgreSQL table already has <c>xmin</c>, so this costs nothing in schema: the Npgsql
/// provider maps a <see cref="uint"/> property marked <c>IsRowVersion()</c> onto it by
/// convention and suppresses all DDL for it. No extra column, no trigger, no migration noise.
/// See artifacts/design/01-architecture.md §7.2 and 02-domain-model.md §8.2.
/// </remarks>
public static class ConcurrencyExtensions
{
    /// <summary>
    /// Maps a <see cref="uint"/> property to <c>xmin</c> as the entity's concurrency token.
    /// Two Studio tabs editing the same course then surface a 409 rather than last-write-wins.
    /// </summary>
    /// <remarks>
    /// <c>IsRowVersion()</c> is sufficient on its own: it sets <c>ValueGenerated.OnAddOrUpdate</c>
    /// and <c>IsConcurrencyToken</c>, and the Npgsql provider's model-finalising convention then
    /// points the column at <c>xmin</c> and suppresses all DDL for it. Naming the column
    /// explicitly here would be redundant.
    /// </remarks>
    public static PropertyBuilder<uint> IsXminConcurrencyToken(this PropertyBuilder<uint> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.IsRowVersion();
    }
}
