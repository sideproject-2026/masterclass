using Lms.Modules.Identity.Domain;

namespace Lms.UnitTests.Identity;

public class RefreshTokenTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 8, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid UserId = Guid.CreateVersion7();

    [Fact]
    public void Issue_returns_the_raw_value_and_stores_only_its_hash()
    {
        var (token, raw) = RefreshToken.Issue(UserId, Now);

        raw.ShouldNotBeNullOrWhiteSpace();
        token.TokenHash.ShouldNotBe(raw, "the raw token must never be persisted");
        token.TokenHash.ShouldBe(RefreshToken.Hash(raw));
    }

    [Fact]
    public void The_hash_is_a_64_character_hex_digest()
    {
        var (token, _) = RefreshToken.Issue(UserId, Now);

        token.TokenHash.Length.ShouldBe(64, "SHA-256 hex, matching the column's max length");
        token.TokenHash.ShouldAllBe(c => Uri.IsHexDigit(c));
    }

    [Fact]
    public void Raw_tokens_are_unpredictable()
    {
        var values = Enumerable.Range(0, 200)
            .Select(_ => RefreshToken.GenerateRawToken())
            .ToHashSet(StringComparer.Ordinal);

        values.Count.ShouldBe(200, "a collision here would mean the CSPRNG is not being used");
    }

    [Fact]
    public void A_new_token_is_active_and_expires_in_fourteen_days()
    {
        var (token, _) = RefreshToken.Issue(UserId, Now);

        token.IsActive(Now).ShouldBeTrue();
        token.ExpiresAt.ShouldBe(Now.AddDays(RefreshToken.LifetimeDays));
        token.IsRevoked.ShouldBeFalse();
    }

    [Fact]
    public void An_expired_token_is_not_active()
    {
        var (token, _) = RefreshToken.Issue(UserId, Now);

        token.IsActive(Now.AddDays(RefreshToken.LifetimeDays).AddSeconds(1)).ShouldBeFalse();
    }

    [Fact]
    public void Revoke_makes_it_inactive_immediately()
    {
        var (token, _) = RefreshToken.Issue(UserId, Now);

        token.Revoke(Now.AddHours(1));

        token.IsRevoked.ShouldBeTrue();
        token.IsActive(Now.AddHours(2)).ShouldBeFalse("logout must end the session");
    }

    [Fact]
    public void Revoke_is_idempotent_and_keeps_the_first_timestamp()
    {
        var (token, _) = RefreshToken.Issue(UserId, Now);
        var firstRevoke = Now.AddHours(1);

        token.Revoke(firstRevoke);
        token.Revoke(Now.AddHours(5));

        token.RevokedAt.ShouldBe(firstRevoke);
    }

    [Fact]
    public void RevokeAndReplace_links_the_rotation_chain()
    {
        var (oldToken, _) = RefreshToken.Issue(UserId, Now);
        var (newToken, _) = RefreshToken.Issue(UserId, Now.AddHours(1));

        oldToken.RevokeAndReplace(Now.AddHours(1), newToken.TokenHash);

        oldToken.IsRevoked.ShouldBeTrue();
        oldToken.ReplacedByTokenHash.ShouldBe(newToken.TokenHash,
            "the chain is what makes a replayed token detectable rather than merely useless");
    }

    [Fact]
    public void Hashing_is_deterministic()
    {
        const string raw = "a-fixed-token-value";

        RefreshToken.Hash(raw).ShouldBe(RefreshToken.Hash(raw));
        RefreshToken.Hash(raw).ShouldNotBe(RefreshToken.Hash(raw + "x"));
    }
}
