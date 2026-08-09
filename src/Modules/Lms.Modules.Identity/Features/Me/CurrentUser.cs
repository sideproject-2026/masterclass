using Lms.Modules.Identity.Domain;
using Lms.Modules.Identity.Infrastructure;
using Lms.SharedKernel.Identifiers;
using Lms.SharedKernel.Messaging;
using Lms.SharedKernel.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lms.Modules.Identity.Features.Me;

/// <summary>Response shape from artifacts/design/03-api-design.md §3.</summary>
public sealed record CurrentUserDto(
    Guid Id,
    string Email,
    string DisplayName,
    IReadOnlyList<string> Roles,
    string? InstructorSlug);

public sealed record GetCurrentUserQuery(UserId UserId) : IQuery<CurrentUserDto>;

public sealed record UpdateCurrentUserCommand(UserId UserId, string DisplayName)
    : ICommand<CurrentUserDto>;

internal sealed class GetCurrentUserHandler(UserManager<AppUser> users, IdentityModuleDbContext db)
    : IQueryHandler<GetCurrentUserQuery, CurrentUserDto>
{
    public async Task<Result<CurrentUserDto>> HandleAsync(GetCurrentUserQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var user = await users.FindByIdAsync(query.UserId.Value.ToString());

        return user is null
            ? IdentityErrors.UserNotFound
            : await ToDtoAsync(users, db, user, ct);
    }

    internal static async Task<CurrentUserDto> ToDtoAsync(
        UserManager<AppUser> users,
        IdentityModuleDbContext db,
        AppUser user,
        CancellationToken ct)
    {
        var roles = await users.GetRolesAsync(user);

        // The web app reads this to decide whether to show the Studio link. It is a hint for
        // the UI and never the access control — /api/studio/* is gated by the Instructor policy
        // plus a per-course ownership check, so a stale or forged value buys nothing.
        var instructorSlug = await db.InstructorProfiles
            .AsNoTracking()
            .Where(p => p.UserId == user.Id)
            .Select(p => p.Slug)
            .FirstOrDefaultAsync(ct);

        return new CurrentUserDto(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName,
            [.. roles],
            instructorSlug);
    }
}

internal sealed class UpdateCurrentUserHandler(UserManager<AppUser> users, IdentityModuleDbContext db)
    : ICommandHandler<UpdateCurrentUserCommand, CurrentUserDto>
{
    public async Task<Result<CurrentUserDto>> HandleAsync(
        UpdateCurrentUserCommand command,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.DisplayName))
        {
            return IdentityErrors.RegistrationFailed("Display name is required.");
        }

        var user = await users.FindByIdAsync(command.UserId.Value.ToString());
        if (user is null)
        {
            return IdentityErrors.UserNotFound;
        }

        user.Rename(command.DisplayName);

        var updated = await users.UpdateAsync(user);

        return updated.Succeeded
            ? await GetCurrentUserHandler.ToDtoAsync(users, db, user, ct)
            : IdentityErrors.RegistrationFailed(
                string.Join(" ", updated.Errors.Select(e => e.Description)));
    }
}
