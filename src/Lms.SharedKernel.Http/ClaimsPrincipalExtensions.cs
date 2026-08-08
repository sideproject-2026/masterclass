using System.Security.Claims;
using Lms.SharedKernel.Identifiers;

namespace Lms.SharedKernel.Http;

/// <summary>
/// Reads the caller's identity out of the validated token.
/// </summary>
/// <remarks>
/// The <c>sub</c> claim is the user id, and every module stores it as a bare
/// <see cref="UserId"/> — see artifacts/design/04-adr-authentication.md §5. Keeping the claim
/// name in one place is what makes a future identity-provider swap a change here rather than
/// everywhere.
/// </remarks>
public static class ClaimsPrincipalExtensions
{
    /// <summary>The short JWT role claim. Must match what the token service issues.</summary>
    public const string RoleClaimType = "role";

    /// <summary>
    /// The caller's id. Throws if absent — an endpoint behind <c>RequireAuthorization</c>
    /// always has one, so a missing <c>sub</c> is a wiring bug, not a request the caller can fix.
    /// </summary>
    public static UserId GetUserId(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return principal.TryGetUserId(out var userId)
            ? userId
            : throw new InvalidOperationException(
                "No 'sub' claim on the current principal. Is this endpoint behind RequireAuthorization?");
    }

    public static bool TryGetUserId(this ClaimsPrincipal principal, out UserId userId)
    {
        ArgumentNullException.ThrowIfNull(principal);

        // JwtBearer maps 'sub' to NameIdentifier unless inbound mapping is disabled, so accept
        // both rather than depending on that setting staying put.
        var raw = principal.FindFirstValue("sub")
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(raw, out var parsed))
        {
            userId = new UserId(parsed);
            return true;
        }

        userId = default;
        return false;
    }

    public static IReadOnlyList<string> GetRoles(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return
        [
            .. principal.FindAll(RoleClaimType).Select(c => c.Value)
                .Concat(principal.FindAll(ClaimTypes.Role).Select(c => c.Value))
                .Distinct(StringComparer.Ordinal)
        ];
    }
}
