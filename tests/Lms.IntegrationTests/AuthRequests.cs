using System.Net.Http.Json;

namespace Lms.IntegrationTests;

/// <summary>The wire shapes, declared once. Deliberately separate from the server's records.</summary>
/// <remarks>
/// Restating them here means a rename on the server breaks a test rather than passing silently
/// because the test reused the same type. These are the contracts in
/// <c>03-api-design.md §3</c>; the tests are what hold the API to them.
/// </remarks>
public sealed record TokenPair(string AccessToken, int ExpiresIn, string RefreshToken, string TokenType);

public sealed record RegisteredAccount(Guid UserId, string Email, string DisplayName);

public sealed record MeResponse(
    Guid Id,
    string Email,
    string DisplayName,
    IReadOnlyList<string> Roles,
    string? InstructorSlug);

/// <summary>Request helpers, so a test reads as the property it is asserting.</summary>
internal static class AuthRequests
{
    /// <summary>Comfortably over the 10-character minimum from <c>04 §3.1</c>.</summary>
    public const string ValidPassword = "correct-horse-battery-staple";

    /// <summary>
    /// A unique address per call. Tests share one database and run in parallel, so a fixed
    /// address would make them order-dependent — and an order-dependent security test is worse
    /// than no test, because it fails for reasons unrelated to the property.
    /// </summary>
    public static string UniqueEmail(string prefix) => $"{prefix}-{Guid.CreateVersion7():N}@example.test";

    public static Task<HttpResponseMessage> RegisterAsync(
        this HttpClient client,
        string email,
        string password = ValidPassword,
        string displayName = "Test Person") =>
        client.PostAsJsonAsync(
            "/api/auth/register",
            new { email, password, displayName });

    public static Task<HttpResponseMessage> LoginAsync(
        this HttpClient client,
        string email,
        string password = ValidPassword) =>
        client.PostAsJsonAsync("/api/auth/login", new { email, password });

    public static Task<HttpResponseMessage> RefreshAsync(this HttpClient client, string refreshToken) =>
        client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken });

    public static Task<HttpResponseMessage> LogoutAsync(this HttpClient client, string refreshToken) =>
        client.PostAsJsonAsync("/api/auth/logout", new { refreshToken });

    /// <summary>Registers an account and returns its first token pair.</summary>
    public static async Task<TokenPair> RegisterAndLoginAsync(this HttpClient client, string prefix)
    {
        var email = UniqueEmail(prefix);

        var registered = await client.RegisterAsync(email);
        registered.StatusCode.ShouldBe(HttpStatusCode.Created);

        return await client.LoginAsync(email).ReadTokensAsync();
    }

    public static async Task<TokenPair> ReadTokensAsync(this Task<HttpResponseMessage> call)
    {
        using var response = await call;
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var tokens = await response.Content.ReadFromJsonAsync<TokenPair>();
        return tokens.ShouldNotBeNull();
    }

    public static HttpRequestMessage Bearer(HttpMethod method, string path, string accessToken)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }
}
