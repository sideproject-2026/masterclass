using Lms.Modules.Identity.Domain;
using Lms.Modules.Identity.Features.Login;
using Lms.Modules.Identity.Infrastructure;
using Lms.SharedKernel.Messaging;
using Lms.SharedKernel.Results;
using Lms.SharedKernel.Time;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lms.Modules.Identity.Features.Refresh;

public sealed record RefreshSessionCommand(string RefreshToken) : ICommand<AuthTokens>;

/// <summary>
/// Exchanges a refresh token for a new pair, rotating the old one.
/// </summary>
/// <remarks>
/// <b>Rotation with reuse detection.</b> Each refresh token is single-use: presenting one
/// revokes it and issues a replacement. If an <i>already-revoked</i> token turns up, either
/// it was stolen and replayed or the legitimate client replayed it — in both cases the chain
/// is no longer trustworthy, so every active token for that user is revoked and the session
/// ends. Without this, a stolen token is silently useful for fourteen days.
/// </remarks>
internal sealed partial class RefreshSessionHandler(
    IdentityModuleDbContext db,
    UserManager<AppUser> users,
    JwtTokenService tokens,
    IClock clock,
    ILogger<RefreshSessionHandler> logger) : ICommandHandler<RefreshSessionCommand, AuthTokens>
{
    public async Task<Result<AuthTokens>> HandleAsync(RefreshSessionCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return IdentityErrors.InvalidRefreshToken;
        }

        var hash = RefreshToken.Hash(command.RefreshToken);

        var existing = await db.RefreshTokens
            .AsTracking()
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (existing is null)
        {
            return IdentityErrors.InvalidRefreshToken;
        }

        if (existing.IsRevoked)
        {
            await RevokeChainAsync(existing.UserId, ct);
            LogReuseDetected(existing.UserId);
            return IdentityErrors.InvalidRefreshToken;
        }

        if (!existing.IsActive(clock.UtcNow))
        {
            return IdentityErrors.InvalidRefreshToken;
        }

        var user = await users.FindByIdAsync(existing.UserId.ToString());
        if (user is null)
        {
            return IdentityErrors.InvalidRefreshToken;
        }

        var (replacement, rawReplacement) = RefreshToken.Issue(user.Id, clock.UtcNow);
        existing.RevokeAndReplace(clock.UtcNow, replacement.TokenHash);
        db.RefreshTokens.Add(replacement);

        var roles = await users.GetRolesAsync(user);
        var access = tokens.CreateAccessToken(user, roles);

        await db.SaveChangesAsync(ct);

        return new AuthTokens(access.Value, access.ExpiresInSeconds, rawReplacement);
    }

    /// <summary>Ends every session for the user. The chain cannot be trusted any more.</summary>
    private async Task RevokeChainAsync(Guid userId, CancellationToken ct)
    {
        var now = clock.UtcNow;

        await db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ExecuteUpdateAsync(set => set.SetProperty(t => t.RevokedAt, now), ct);
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Refresh token reuse detected for user {UserId}. All sessions revoked.")]
    private partial void LogReuseDetected(Guid userId);
}
