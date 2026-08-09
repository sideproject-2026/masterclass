using System.Net.Http.Json;

namespace Lms.IntegrationTests;

/// <summary><c>GET</c> and <c>PUT /api/me</c> — card <c>A-2</c>.</summary>
public sealed class MeEndpointTests(LmsApiFixture fixture)
{
    private readonly HttpClient _client = fixture.CreateClient();

    [Fact]
    public async Task Me_rejects_an_anonymous_caller()
    {
        using var response = await _client.GetAsync("/api/me");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_rejects_a_garbage_token()
    {
        using var request = AuthRequests.Bearer(HttpMethod.Get, "/api/me", "not.a.jwt");
        using var response = await _client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_returns_the_caller_for_a_valid_token()
    {
        var tokens = await _client.RegisterAndLoginAsync("me");

        using var request = AuthRequests.Bearer(HttpMethod.Get, "/api/me", tokens.AccessToken);
        using var response = await _client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var me = await response.Content.ReadFromJsonAsync<MeResponse>();
        me.ShouldNotBeNull();
        me.Id.ShouldNotBe(Guid.Empty);
        me.Roles.ShouldContain("Student");
    }

    /// <summary>
    /// <c>instructorSlug</c> is null until an admin grants the role.
    /// </summary>
    /// <remarks>
    /// The web app reads exactly this field to decide whether to show the Studio link, so a
    /// student seeing a non-null value here is a student seeing an authoring entry point.
    /// <c>A-6</c> makes it non-null for granted instructors; this pins the other half.
    /// </remarks>
    [Fact]
    public async Task Me_reports_no_instructor_slug_for_a_student()
    {
        var tokens = await _client.RegisterAndLoginAsync("no-slug");

        using var request = AuthRequests.Bearer(HttpMethod.Get, "/api/me", tokens.AccessToken);
        using var response = await _client.SendAsync(request);

        var me = await response.Content.ReadFromJsonAsync<MeResponse>();
        me.ShouldNotBeNull();
        me.InstructorSlug.ShouldBeNull();
    }

    /// <summary>
    /// A rename takes the user id from the token, never from the body.
    /// </summary>
    /// <remarks>
    /// If the id came from the request, this endpoint would let any authenticated caller rename
    /// any account. The request shape has no id field at all, which is the enforcement.
    /// </remarks>
    [Fact]
    public async Task Me_renames_the_caller_only()
    {
        var tokens = await _client.RegisterAndLoginAsync("rename");

        using var request = AuthRequests.Bearer(HttpMethod.Put, "/api/me", tokens.AccessToken);
        request.Content = JsonContent.Create(new { displayName = "Renamed Person" });

        using var response = await _client.SendAsync(request);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var me = await response.Content.ReadFromJsonAsync<MeResponse>();
        me.ShouldNotBeNull();
        me.DisplayName.ShouldBe("Renamed Person");
    }
}
