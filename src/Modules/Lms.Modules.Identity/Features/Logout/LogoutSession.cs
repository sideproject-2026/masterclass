using Lms.Modules.Identity.Domain;
using Lms.Modules.Identity.Infrastructure;
using Lms.SharedKernel.Messaging;
using Lms.SharedKernel.Results;
using Lms.SharedKernel.Time;
using Microsoft.EntityFrameworkCore;

namespace Lms.Modules.Identity.Features.Logout;

public sealed record LogoutCommand(string RefreshToken) : ICommand<Unit>;

/// <summary>
/// Revokes the refresh token, ending the session.
/// </summary>
/// <remarks>
/// The already-issued access token stays valid until it expires — at most 15 minutes. That
/// window is the accepted cost of stateless validation
/// (artifacts/design/04-adr-authentication.md §3.1); this is what stops the session being
/// extended beyond it.
/// <para>
/// Deliberately succeeds even when the token is unknown or already revoked. Logout is not an
/// oracle for whether a token exists, and a client trying to end a session it cannot prove it
/// has should still see success rather than an error it cannot act on.
/// </para>
/// </remarks>
internal sealed class LogoutHandler(
    IdentityModuleDbContext db,
    IClock clock) : ICommandHandler<LogoutCommand, Unit>
{
    public async Task<Result<Unit>> HandleAsync(LogoutCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return Unit.Value;
        }

        var hash = RefreshToken.Hash(command.RefreshToken);
        var now = clock.UtcNow;

        await db.RefreshTokens
            .Where(t => t.TokenHash == hash && t.RevokedAt == null)
            .ExecuteUpdateAsync(set => set.SetProperty(t => t.RevokedAt, now), ct);

        return Unit.Value;
    }
}
