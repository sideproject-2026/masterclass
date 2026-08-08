namespace Lms.SharedKernel.Pagination;

/// <summary>
/// A validated, clamped page request. Constructed only through <see cref="Of"/>, so no
/// endpoint can forget the cap — see artifacts/design/09-code-conventions.md §8.1.
/// </summary>
public readonly record struct PageRequest
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 50;

    private PageRequest(int page, int pageSize)
    {
        Page = page;
        PageSize = pageSize;
    }

    public int Page { get; }
    public int PageSize { get; }
    public int Skip => (Page - 1) * PageSize;

    /// <summary>
    /// Clamps rather than rejects: <c>?pageSize=5000</c> becomes 50 and <c>?page=-3</c> becomes 1.
    /// A nonsense query parameter on a public catalogue should not be an error page.
    /// </summary>
    public static PageRequest Of(int? page, int? pageSize) =>
        new(Math.Max(1, page ?? 1),
            Math.Clamp(pageSize ?? DefaultPageSize, 1, MaxPageSize));

    public static PageRequest First { get; } = Of(null, null);

    public override string ToString() => $"page {Page} × {PageSize}";
}
