using Lms.SharedKernel.Pagination;
using Microsoft.AspNetCore.Http;

namespace Lms.SharedKernel.Http;

/// <summary>
/// Query-string paging parameters, bound with <c>[AsParameters]</c>.
/// </summary>
/// <remarks>
/// Nullable on purpose so <see cref="PageRequest.Of"/> can apply the defaults and the clamp
/// in one place. A missing or nonsensical value is corrected, never rejected — see
/// artifacts/design/09-code-conventions.md §8.1.
/// </remarks>
public readonly record struct PagingParams(int? Page, int? PageSize)
{
    public PageRequest ToPageRequest() => PageRequest.Of(Page, PageSize);

    public static ValueTask<PagingParams> BindAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var query = context.Request.Query;

        return ValueTask.FromResult(new PagingParams(
            TryParse(query["page"]),
            TryParse(query["pageSize"])));

        static int? TryParse(string? raw) =>
            int.TryParse(raw, out var value) ? value : null;
    }
}
