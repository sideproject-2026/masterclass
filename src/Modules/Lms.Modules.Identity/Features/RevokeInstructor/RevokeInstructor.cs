using Lms.Modules.Identity.Domain;
using Lms.SharedKernel.Authorization;
using Lms.SharedKernel.Messaging;
using Lms.SharedKernel.Results;
using Microsoft.AspNetCore.Identity;

namespace Lms.Modules.Identity.Features.RevokeInstructor;

public sealed record RevokeInstructorCommand(Guid UserId) : ICommand<Unit>;

/// <summary>
/// Removes the <c>Instructor</c> role. Nothing else.
/// </summary>
/// <remarks>
/// <b>Published courses stay published and the profile stays put</b>
/// (artifacts/design/03-api-design.md §6). Unpublishing on revoke would cut enrolled students
/// off from material they are part-way through, turning an administrative decision about one
/// person into an outage for everyone who trusted them. Removing content is a deliberate,
/// separate act.
/// <para>
/// The profile and its slug are kept for the same reason: course pages still name the author,
/// and freeing the slug would let a later instructor inherit another person's URL.
/// </para>
/// <para>
/// Revocation is not instant. The role lives in an access token that cannot be recalled, so it
/// takes effect within the 15-minute token lifetime — that window <i>is</i> the revocation
/// window (04-adr-authentication.md §3.1).
/// </para>
/// </remarks>
internal sealed class RevokeInstructorHandler(UserManager<AppUser> users)
    : ICommandHandler<RevokeInstructorCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(RevokeInstructorCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var user = await users.FindByIdAsync(command.UserId.ToString());
        if (user is null)
        {
            return IdentityErrors.UserNotFound;
        }

        if (!await users.IsInRoleAsync(user, Roles.Instructor))
        {
            // Already revoked. Idempotent, so a repeated call is not an error.
            return Unit.Value;
        }

        var removed = await users.RemoveFromRoleAsync(user, Roles.Instructor);

        return removed.Succeeded
            ? Unit.Value
            : IdentityErrors.RegistrationFailed(
                string.Join(" ", removed.Errors.Select(e => e.Description)));
    }
}
