using System.Security.Claims;
using Lms.Modules.Identity.Domain;
using Lms.Modules.Identity.Infrastructure;
using Lms.SharedKernel.Authorization;
using Lms.SharedKernel.Time;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Lms.UnitTests.Identity;

public class JwtTokenServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 8, 12, 0, 0, TimeSpan.Zero);

    private sealed class FixedClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow { get; } = now;
    }

    private static JwtTokenService Service() => new(
        Options.Create(new JwtOptions
        {
            Issuer = "lms-api",
            Audience = "lms-web",
            SigningKey = "a-test-signing-key-that-is-long-enough-32"
        }),
        new FixedClock(Now));

    private static AppUser AUser() =>
        AppUser.Create("sam@example.com", "Sam Rivera", Now);

    private static JsonWebToken Decode(string token) =>
        new JsonWebTokenHandler().ReadJsonWebToken(token);

    [Fact]
    public void The_token_is_a_jwt()
    {
        var token = Service().CreateAccessToken(AUser(), [Roles.Student]);

        // 03-api-design.md §3 specifies an "eyJ..." access token — that prefix is a JWT header.
        token.Value.ShouldStartWith("eyJ");
        token.Value.Split('.').Length.ShouldBe(3);
    }

    [Fact]
    public void It_carries_sub_email_and_name()
    {
        var user = AUser();

        var jwt = Decode(Service().CreateAccessToken(user, [Roles.Student]).Value);

        jwt.GetClaim(JwtRegisteredClaimNames.Sub).Value.ShouldBe(user.Id.ToString());
        jwt.GetClaim(JwtRegisteredClaimNames.Email).Value.ShouldBe("sam@example.com");
        jwt.GetClaim(JwtRegisteredClaimNames.Name).Value.ShouldBe("Sam Rivera");
    }

    [Fact]
    public void Each_role_becomes_its_own_claim()
    {
        var jwt = Decode(Service()
            .CreateAccessToken(AUser(), [Roles.Student, Roles.Instructor]).Value);

        var roles = jwt.Claims
            .Where(c => c.Type == JwtTokenService.RoleClaimType)
            .Select(c => c.Value)
            .ToList();

        roles.ShouldContain(Roles.Student);
        roles.ShouldContain(Roles.Instructor);

        // Short "role", never the SOAP-era ClaimTypes.Role URI.
        jwt.Claims.ShouldNotContain(c => c.Type == ClaimTypes.Role);
    }

    [Fact]
    public void It_carries_no_profile_data_beyond_the_documented_claims()
    {
        // Anything in a token is frozen for its lifetime, so a mutable field becomes a stale
        // one. 04-adr-authentication.md §3.1: sub, email, name, role — "nothing else".
        var jwt = Decode(Service().CreateAccessToken(AUser(), [Roles.Student]).Value);

        var allowed = new HashSet<string>(StringComparer.Ordinal)
        {
            JwtRegisteredClaimNames.Sub, JwtRegisteredClaimNames.Email,
            JwtRegisteredClaimNames.Name, JwtRegisteredClaimNames.Jti,
            JwtRegisteredClaimNames.Iss, JwtRegisteredClaimNames.Aud,
            JwtRegisteredClaimNames.Exp, JwtRegisteredClaimNames.Iat,
            JwtRegisteredClaimNames.Nbf,
            JwtTokenService.RoleClaimType
        };

        var unexpected = jwt.Claims.Select(c => c.Type).Where(t => !allowed.Contains(t)).ToList();

        unexpected.ShouldBeEmpty();
    }

    [Fact]
    public void It_expires_in_fifteen_minutes()
    {
        // The lifetime IS the revocation window — a JWT cannot be withdrawn early.
        var token = Service().CreateAccessToken(AUser(), [Roles.Student]);

        token.ExpiresInSeconds.ShouldBe(15 * 60);
        Decode(token.Value).ValidTo.ShouldBe(Now.AddMinutes(15).UtcDateTime, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void It_sets_the_configured_issuer_and_audience()
    {
        var jwt = Decode(Service().CreateAccessToken(AUser(), [Roles.Student]).Value);

        jwt.Issuer.ShouldBe("lms-api");
        jwt.Audiences.ShouldContain("lms-web");
    }

    [Fact]
    public void Two_tokens_for_the_same_user_differ()
    {
        var user = AUser();
        var service = Service();

        // Distinct jti, so tokens are individually identifiable in logs even at a fixed clock.
        service.CreateAccessToken(user, [Roles.Student]).Value
            .ShouldNotBe(service.CreateAccessToken(user, [Roles.Student]).Value);
    }
}
