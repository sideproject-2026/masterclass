using System.Security.Cryptography;

namespace Lms.Modules.Identity.Domain;

/// <summary>
/// A long-lived credential that buys a new access token.
/// </summary>
/// <remarks>
/// Access tokens are JWTs and cannot be revoked before they expire, so their 15-minute
/// lifetime <i>is</i> the revocation window. This is the part that makes logout mean
/// something: revoke the refresh token and the session cannot be extended past that window.
/// See artifacts/design/04-adr-authentication.md §3.1.
/// <para>
/// <b>Only the hash is stored.</b> A database leak must not hand out usable sessions.
/// </para>
/// </remarks>
public sealed class RefreshToken
{
    public const int LifetimeDays = 14;

    private RefreshToken() => TokenHash = null!;

    private RefreshToken(Guid id, Guid userId, string tokenHash, DateTimeOffset createdAt, DateTimeOffset expiresAt)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }

    /// <summary>SHA-256 of the raw token. The raw value exists only in the response body.</summary>
    public string TokenHash { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    /// <summary>Set when this token was rotated, linking the chain for reuse detection.</summary>
    public string? ReplacedByTokenHash { get; private set; }

    public bool IsRevoked => RevokedAt is not null;

    public bool IsActive(DateTimeOffset now) => !IsRevoked && now < ExpiresAt;

    /// <summary>
    /// Mints a token. Returns the raw value <b>once</b> — it is never recoverable afterwards.
    /// </summary>
    public static (RefreshToken Token, string RawValue) Issue(Guid userId, DateTimeOffset now)
    {
        var raw = GenerateRawToken();

        var token = new RefreshToken(
            Guid.CreateVersion7(),
            userId,
            Hash(raw),
            now,
            now.AddDays(LifetimeDays));

        return (token, raw);
    }

    /// <summary>Revokes on logout. Idempotent — re-revoking does not move the timestamp.</summary>
    public void Revoke(DateTimeOffset now)
    {
        RevokedAt ??= now;
    }

    /// <summary>Revokes because this token was exchanged for a new one.</summary>
    public void RevokeAndReplace(DateTimeOffset now, string replacementHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(replacementHash);

        RevokedAt ??= now;
        ReplacedByTokenHash = replacementHash;
    }

    /// <summary>256 bits from a CSPRNG. Not a Guid — Guids are identifiers, not secrets.</summary>
    public static string GenerateRawToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    /// <summary>
    /// Plain SHA-256, deliberately not a password hash. The token is already 256 bits of
    /// entropy, so there is nothing to brute-force and a slow KDF would only tax every refresh.
    /// </summary>
    public static string Hash(string rawToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawToken);

        return Convert.ToHexString(
            SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken)));
    }
}
