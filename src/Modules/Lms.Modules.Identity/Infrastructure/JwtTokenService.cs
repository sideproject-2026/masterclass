using System.Security.Claims;
using Lms.Modules.Identity.Domain;
using Lms.SharedKernel.Time;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Lms.Modules.Identity.Infrastructure;

public sealed record AccessToken(string Value, int ExpiresInSeconds);

/// <summary>
/// Issues the signed access tokens the API validates.
/// </summary>
/// <remarks>
/// Custom issuance rather than <c>MapIdentityApi</c>'s opaque tokens: the API contract
/// specifies an <c>eyJ...</c> access token with named claims, and stateless validation is
/// what keeps authorisation off the database on every request
/// (artifacts/design/04-adr-authentication.md §3.1).
/// </remarks>
internal sealed class JwtTokenService(IOptions<JwtOptions> options, IClock clock)
{
    /// <summary>
    /// The short, standard JWT role claim name. Both the issuer here and the validating
    /// side in <c>Lms.Api</c> must agree on this, or every policy silently denies.
    /// </summary>
    public const string RoleClaimType = "role";

    private readonly JwtOptions _options = options.Value;

    /// <summary>
    /// Builds a token carrying <c>sub</c>, <c>email</c>, <c>name</c> and one <c>role</c> per
    /// role — and nothing else.
    /// </summary>
    /// <remarks>
    /// No profile data. Anything in here is frozen for the token's lifetime, so a field that
    /// can change becomes a field that can be stale and wrong.
    /// </remarks>
    public AccessToken CreateAccessToken(AppUser user, IEnumerable<string> roles)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(roles);

        var now = clock.UtcNow;
        var expires = now.AddMinutes(JwtOptions.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Name, user.DisplayName),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString())
        };

        // "role", not ClaimTypes.Role. The latter is the SOAP-era URI
        // http://schemas.microsoft.com/ws/2008/06/identity/claims/role, which bloats every
        // token and is not what 04-adr-authentication.md §3.1 specifies. The validating side
        // sets RoleClaimType = "role" so policies still resolve.
        claims.AddRange(roles.Select(role => new Claim(RoleClaimType, role)));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expires.UtcDateTime,
            SigningCredentials = new SigningCredentials(
                _options.SecurityKey(),
                SecurityAlgorithms.HmacSha256)
        };

        var value = new JsonWebTokenHandler().CreateToken(descriptor);

        return new AccessToken(value, (int)(expires - now).TotalSeconds);
    }
}
