using Lms.Modules.Identity.Domain;
using Lms.Modules.Identity.Infrastructure;
using Lms.SharedKernel.Messaging;
using Lms.SharedKernel.Results;
using Lms.SharedKernel.Time;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lms.Modules.Identity.Features.Login;

public sealed record LoginUserCommand(string Email, string Password) : ICommand<AuthTokens>;

/// <summary>The login/refresh response shape from artifacts/design/03-api-design.md §3.</summary>
public sealed record AuthTokens(
    string AccessToken,
    int ExpiresIn,
    string RefreshToken,
    string TokenType = "Bearer");

/// <summary>
/// Exchanges credentials for a token pair.
/// </summary>
/// <remarks>
/// <b>Every failure returns the same error.</b> Unknown email, wrong password and lockout are
/// indistinguishable to the caller — otherwise this endpoint tells an attacker which addresses
/// are registered. See <see cref="IdentityErrors.InvalidCredentials"/>.
/// </remarks>
internal sealed class LoginUserHandler(
    UserManager<AppUser> users,
    IdentityModuleDbContext db,
    JwtTokenService tokens,
    IClock clock) : ICommandHandler<LoginUserCommand, AuthTokens>
{
    public async Task<Result<AuthTokens>> HandleAsync(LoginUserCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var user = await users.FindByEmailAsync(command.Email.Trim());
        if (user is null)
        {
            // Hash a throwaway password so a missing account costs the same time as a wrong
            // one. Without this, response latency leaks which emails exist.
            users.PasswordHasher.HashPassword(AppUser.Create("probe@invalid", "probe", clock.UtcNow),
                command.Password);
            return IdentityErrors.InvalidCredentials;
        }

        if (await users.IsLockedOutAsync(user))
        {
            return IdentityErrors.InvalidCredentials;
        }

        if (!await users.CheckPasswordAsync(user, command.Password))
        {
            await users.AccessFailedAsync(user);
            return IdentityErrors.InvalidCredentials;
        }

        await users.ResetAccessFailedCountAsync(user);

        return await IssueTokensAsync(user, ct);
    }

    private async Task<AuthTokens> IssueTokensAsync(AppUser user, CancellationToken ct)
    {
        var roles = await users.GetRolesAsync(user);
        var access = tokens.CreateAccessToken(user, roles);

        var (refresh, rawRefresh) = RefreshToken.Issue(user.Id, clock.UtcNow);
        db.RefreshTokens.Add(refresh);
        await db.SaveChangesAsync(ct);

        return new AuthTokens(access.Value, access.ExpiresInSeconds, rawRefresh);
    }
}
