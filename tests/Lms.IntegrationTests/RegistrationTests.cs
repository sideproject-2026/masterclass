using System.Net.Http.Json;

namespace Lms.IntegrationTests;

/// <summary><c>POST /api/auth/register</c> — acceptance criteria from card <c>A-1</c>.</summary>
public sealed class RegistrationTests(LmsApiFixture fixture)
{
    private readonly HttpClient _client = fixture.CreateClient();

    [Fact]
    public async Task Register_creates_a_student_account()
    {
        var email = AuthRequests.UniqueEmail("register");

        using var response = await _client.RegisterAsync(email, displayName: "Ada Lovelace");

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var account = await response.Content.ReadFromJsonAsync<RegisteredAccount>();
        account.ShouldNotBeNull();
        account.Email.ShouldBe(email);
        account.DisplayName.ShouldBe("Ada Lovelace");
        account.UserId.ShouldNotBe(Guid.Empty);
    }

    /// <summary>
    /// Registration always grants <c>Student</c> and never anything else.
    /// </summary>
    /// <remarks>
    /// The alternative — a role in the request body — would let anyone mint themselves an
    /// instructor or admin account. Instructors are granted by an admin (<c>A-6</c>).
    /// </remarks>
    [Fact]
    public async Task Register_grants_Student_and_nothing_else()
    {
        var tokens = await _client.RegisterAndLoginAsync("roles");

        using var request = AuthRequests.Bearer(HttpMethod.Get, "/api/me", tokens.AccessToken);
        using var response = await _client.SendAsync(request);

        var me = await response.Content.ReadFromJsonAsync<MeResponse>();
        me.ShouldNotBeNull();
        me.Roles.ShouldBe(["Student"]);
    }

    [Fact]
    public async Task Register_rejects_a_duplicate_email_with_409()
    {
        var email = AuthRequests.UniqueEmail("duplicate");

        using var first = await _client.RegisterAsync(email);
        first.StatusCode.ShouldBe(HttpStatusCode.Created);

        using var second = await _client.RegisterAsync(email);
        second.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    /// <summary>Length is the rule, not composition — see <c>04 §3.1</c>.</summary>
    [Fact]
    public async Task Register_rejects_a_short_password_with_400()
    {
        using var response = await _client.RegisterAsync(
            AuthRequests.UniqueEmail("weak"),
            password: "short");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
