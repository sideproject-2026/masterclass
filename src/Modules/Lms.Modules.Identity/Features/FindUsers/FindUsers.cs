using Lms.Modules.Identity.Infrastructure;
using Lms.SharedKernel.Messaging;
using Lms.SharedKernel.Pagination;
using Lms.SharedKernel.Persistence;
using Lms.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace Lms.Modules.Identity.Features.FindUsers;

/// <summary>Response shape from artifacts/design/03-api-design.md §6.</summary>
public sealed record AdminUser(
    Guid Id,
    string Email,
    string DisplayName,
    string? InstructorSlug,
    DateTimeOffset CreatedAt);

public sealed record FindUsersQuery(string? Search, PageRequest Page)
    : IQuery<QueryResult<AdminUser>>;

/// <summary>
/// Finds a user id by email, so an admin can grant the instructor role.
/// </summary>
/// <remarks>
/// This is the only endpoint in the system that lists accounts, which is exactly why it sits
/// behind the <c>Admin</c> policy: the same query exposed one level lower would be the user
/// enumeration that <c>login</c> goes out of its way to prevent.
/// </remarks>
internal sealed class FindUsersHandler(IdentityModuleDbContext db)
    : IQueryHandler<FindUsersQuery, QueryResult<AdminUser>>
{
    public async Task<Result<QueryResult<AdminUser>>> HandleAsync(
        FindUsersQuery query,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var users = db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";

            // ILIKE rather than ToUpper().Contains(): the comparison happens in PostgreSQL
            // either way, and this states the case-insensitivity in SQL instead of relying on
            // EF to translate a .NET string method into it.
            users = users.Where(u =>
                EF.Functions.ILike(u.Email ?? string.Empty, pattern)
                || EF.Functions.ILike(u.DisplayName, pattern));
        }

        // Project inside the query — a list view never materialises an entity, and an AppUser
        // carries a password hash and a security stamp that have no business leaving the module.
        //
        // ThenBy(Id) is the unique tiebreaker paging needs: without it two accounts created in
        // the same tick can swap places between pages, so a row is seen twice or not at all.
        var projected = users
            .OrderBy(u => u.CreatedAt)
            .ThenBy(u => u.Id)
            .Select(u => new AdminUser(
                u.Id,
                u.Email ?? string.Empty,
                u.DisplayName,
                db.InstructorProfiles.Where(p => p.UserId == u.Id).Select(p => p.Slug).FirstOrDefault(),
                u.CreatedAt));

        return await projected.ToQueryResultAsync(query.Page, ct);
    }
}
