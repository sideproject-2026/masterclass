using System.Text.RegularExpressions;

namespace Lms.IntegrationTests;

/// <summary><c>POST /api/auth/login</c> — including the enumeration defence from <c>A-1</c>.</summary>
public sealed partial class LoginTests(LmsApiFixture fixture)
{
    private readonly HttpClient _client = fixture.CreateClient();

    [Fact]
    public async Task Login_returns_a_token_pair()
    {
        var tokens = await _client.RegisterAndLoginAsync("login");

        tokens.AccessToken.ShouldNotBeNullOrWhiteSpace();
        tokens.RefreshToken.ShouldNotBeNullOrWhiteSpace();
        tokens.TokenType.ShouldBe("Bearer");

        // 15 minutes. The access token cannot be revoked early, so this value *is* the
        // revocation window (04 §3.1) — a silent change to it is a security change.
        tokens.ExpiresIn.ShouldBe(900);
    }

    /// <summary>
    /// The single most important assertion in this file.
    /// </summary>
    /// <remarks>
    /// A wrong password and an unregistered address must be indistinguishable, or the endpoint
    /// answers "does this person have an account here?" for anyone who asks. It is broken by
    /// something as ordinary as a well-meant "no account found with that email" message, and
    /// nothing else in the build would notice.
    /// </remarks>
    [Fact]
    public async Task Login_failures_are_indistinguishable_between_unknown_email_and_wrong_password()
    {
        var registered = AuthRequests.UniqueEmail("known");
        using var created = await _client.RegisterAsync(registered);
        created.StatusCode.ShouldBe(HttpStatusCode.Created);

        using var wrongPassword = await _client.LoginAsync(registered, "definitely-not-the-password");
        using var unknownEmail = await _client.LoginAsync(
            AuthRequests.UniqueEmail("never-registered"),
            "definitely-not-the-password");

        wrongPassword.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        unknownEmail.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var wrongPasswordBody = Normalise(await wrongPassword.Content.ReadAsStringAsync());
        var unknownEmailBody = Normalise(await unknownEmail.Content.ReadAsStringAsync());

        unknownEmailBody.ShouldBe(wrongPasswordBody);
    }

    [Fact]
    public async Task Login_rejects_a_wrong_password()
    {
        var email = AuthRequests.UniqueEmail("wrong-password");
        using var created = await _client.RegisterAsync(email);
        created.StatusCode.ShouldBe(HttpStatusCode.Created);

        using var response = await _client.LoginAsync(email, "not-the-right-password");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Strips the correlation id, which is the one field that legitimately differs per request.
    /// Everything else in the two bodies has to match character for character.
    /// </summary>
    private static string Normalise(string problemDetails) =>
        TraceId().Replace(problemDetails, "\"traceId\":\"<normalised>\"");

    [GeneratedRegex("\"traceId\"\\s*:\\s*\"[^\"]*\"")]
    private static partial Regex TraceId();
}
