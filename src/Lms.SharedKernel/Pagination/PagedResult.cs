namespace Lms.SharedKernel.Pagination;

/// <summary>
/// The client-facing paging envelope (artifacts/design/03-api-design.md §1.1).
/// </summary>
/// <remarks>
/// <c>QueryResult.Data</c> becomes <c>Items</c> here. The rename is deliberate:
/// <c>Data</c> is the internal payload, <c>items</c> is the published contract, and the
/// contract must not change just because an internal field was renamed.
/// </remarks>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages)
{
    public static PagedResult<T> From(QueryResult<T> result, PageRequest page) =>
        new(result.Data,
            page.Page,
            page.PageSize,
            result.TotalCount,
            (int)Math.Ceiling(result.TotalCount / (double)page.PageSize));

    public static PagedResult<T> Empty(PageRequest page) => From(QueryResult<T>.Empty, page);
}
