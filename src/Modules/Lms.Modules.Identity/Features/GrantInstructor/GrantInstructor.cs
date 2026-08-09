using Lms.Modules.Identity.Domain;
using Lms.Modules.Identity.Infrastructure;
using Lms.SharedKernel.Authorization;
using Lms.SharedKernel.Messaging;
using Lms.SharedKernel.Results;
using Lms.SharedKernel.Time;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lms.Modules.Identity.Features.GrantInstructor;

/// <summary>Response shape from artifacts/design/03-api-design.md §6.</summary>
public sealed record InstructorGrant(
    Guid UserId,
    IReadOnlyList<string> Roles,
    string InstructorSlug);

public sealed record GrantInstructorCommand(Guid UserId, string Slug, string Headline)
    : ICommand<InstructorGrant>;

/// <summary>
/// Curated instructor onboarding — the whole of it (00-overview.md §5).
/// </summary>
/// <remarks>
/// There is no self-service path to this role and there must never be one: an instructor can
/// publish content to every student on the platform. The endpoint is behind the <c>Admin</c>
/// policy and is called with an HTTP client, not a console.
/// </remarks>
internal sealed class GrantInstructorHandler(
    UserManager<AppUser> users,
    IdentityModuleDbContext db,
    IClock clock) : ICommandHandler<GrantInstructorCommand, InstructorGrant>
{
    public async Task<Result<InstructorGrant>> HandleAsync(
        GrantInstructorCommand command,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var user = await users.FindByIdAsync(command.UserId.ToString());
        if (user is null)
        {
            return IdentityErrors.UserNotFound;
        }

        var existing = await db.InstructorProfiles
            .FirstOrDefaultAsync(p => p.UserId == user.Id, ct);

        // Idempotent: granting twice is a retry, not a conflict. Only a *different* person
        // already holding the slug is a real 409.
        if (existing is null)
        {
            var created = await CreateProfileAsync(user.Id, command, ct);
            if (created.IsFailure)
            {
                return created.Error;
            }

            existing = created.Value;
        }

        if (!await users.IsInRoleAsync(user, Roles.Instructor))
        {
            var added = await users.AddToRoleAsync(user, Roles.Instructor);
            if (!added.Succeeded)
            {
                return IdentityErrors.RegistrationFailed(
                    string.Join(" ", added.Errors.Select(e => e.Description)));
            }
        }

        var roles = await users.GetRolesAsync(user);

        return new InstructorGrant(user.Id, [.. roles], existing.Slug);
    }

    private async Task<Result<InstructorProfile>> CreateProfileAsync(
        Guid userId,
        GrantInstructorCommand command,
        CancellationToken ct)
    {
        var profile = InstructorProfile.Create(userId, command.Slug, command.Headline, clock.UtcNow);
        if (profile.IsFailure)
        {
            return profile.Error;
        }

        db.InstructorProfiles.Add(profile.Value);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // The unique index on slug is the real arbiter. Checking first and inserting after
            // would still lose to a second admin granting the same slug concurrently, so the
            // conflict is caught here rather than pre-empted with a query that can go stale.
            db.Entry(profile.Value).State = EntityState.Detached;
            return IdentityErrors.SlugAlreadyTaken;
        }

        return profile.Value;
    }
}
