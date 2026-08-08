namespace Lms.SharedKernel.Pagination;

/// <summary>
/// What a paged query handler returns: one page of data plus the total row count.
/// </summary>
/// <remarks>
/// Deliberately knows nothing about page numbers or page sizes — the caller already knows
/// what it asked for. Keeping those out means a handler can be reused by something that
/// pages differently (an export job, a cross-module Contracts query) without carrying
/// meaningless fields. See artifacts/design/09-code-conventions.md §8.2.
/// </remarks>
public sealed record QueryResult<T>(IReadOnlyList<T> Data, int TotalCount)
{
    public static QueryResult<T> Empty { get; } = new([], 0);

    public QueryResult<TOut> Map<TOut>(Func<T, TOut> map) =>
        new([.. Data.Select(map)], TotalCount);
}
