using Lms.SharedKernel.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Lms.SharedKernel.Persistence;

/// <summary>
/// The only paging implementation in the codebase.
/// </summary>
/// <remarks>
/// If <c>Skip(</c> or <c>Take(</c> appears anywhere outside this file, that is a bug —
/// see artifacts/design/09-code-conventions.md §8.3.
/// <para>
/// This lives in a separate project from <c>Lms.SharedKernel</c> so that EF Core does not
/// become transitively visible from every module's Domain folder, which would weaken the
/// architecture rule in artifacts/design/01-architecture.md §4.1.
/// </para>
/// </remarks>
public static class QueryableExtensions
{
    /// <summary>
    /// Materialises one page plus the total count.
    /// </summary>
    /// <remarks>
    /// The query <b>must already be ordered, with a unique tiebreaker</b>. PostgreSQL makes no
    /// guarantee about row order between <c>OFFSET</c> queries otherwise, so rows can repeat
    /// across pages or vanish entirely. Ordering by a non-unique column such as
    /// <c>PublishedAt</c> is not sufficient — append <c>.ThenBy(x =&gt; x.Id)</c>.
    /// </remarks>
    public static async Task<QueryResult<T>> ToQueryResultAsync<T>(
        this IQueryable<T> query,
        PageRequest page,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var total = await query.CountAsync(ct);
        if (total == 0)
        {
            return QueryResult<T>.Empty;
        }

        var data = await query
            .Skip(page.Skip)
            .Take(page.PageSize)
            .ToListAsync(ct);

        return new QueryResult<T>(data, total);
    }
}
