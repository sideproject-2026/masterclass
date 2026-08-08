using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Lms.Modules.Identity.Infrastructure;

/// <summary>
/// JWT signing and validation settings, bound from the <c>Jwt</c> configuration section.
/// </summary>
/// <remarks>
/// Validated at startup with <c>ValidateOnStart()</c>. A misconfigured signing key must stop
/// the process, not surface as a puzzling 401 the first time somebody logs in.
/// </remarks>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// 15 minutes, not the hour in the original ADR.
    /// </summary>
    /// <remarks>
    /// A JWT cannot be revoked before it expires, so this value <b>is</b> the revocation
    /// window: how long a logged-out session, or a just-revoked instructor, keeps working.
    /// The BFF refreshes transparently server-side, so shortening it costs users nothing.
    /// </remarks>
    public const int AccessTokenMinutes = 15;

    [Required]
    public string Issuer { get; init; } = "lms-api";

    [Required]
    public string Audience { get; init; } = "lms-web";

    /// <summary>
    /// HMAC signing key. At least 32 bytes — HS256 truncates or rejects anything shorter,
    /// and a short key is a forgeable token.
    /// </summary>
    [Required]
    [MinLength(32, ErrorMessage = "Jwt:SigningKey must be at least 32 characters.")]
    public string SigningKey { get; init; } = string.Empty;

    public SymmetricSecurityKey SecurityKey() =>
        new(Encoding.UTF8.GetBytes(SigningKey));
}
