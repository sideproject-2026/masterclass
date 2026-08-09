using System.Net.Http.Json;

namespace Lms.IntegrationTests;

/// <summary><c>/api/admin</c> — curated instructor onboarding, card <c>A-6</c>.</summary>
public sealed class AdminEndpointTests(LmsApiFixture fixture)
{
    private readonly HttpClient _client = fixture.CreateClient();

    private sealed record Grant(Guid UserId, IReadOnlyList<string> Roles, string InstructorSlug);

    private sealed record AdminUserRow(
        Guid Id,
        string Email,
        string DisplayName,
        string? InstructorSlug,
        DateTimeOffset CreatedAt);

    /// <summary>Mirrors <c>PagedResult&lt;T&gt;</c> — restated so a contract rename breaks a test.</summary>
    private sealed record PagedBody<T>(
        IReadOnlyList<T> Items,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages);

    /// <summary>
    /// A unique, well-formed slug. Hyphen-separated lowercase hex, so it satisfies the domain
    /// pattern and cannot collide with another test's grant.
    /// </summary>
    private static string Slug(string prefix) => $"{prefix}-{Guid.CreateVersion7():N}";

    private async Task<string> AdminTokenAsync()
    {
        var tokens = await _client
            .LoginAsync(LmsApiFixture.AdminEmail, LmsApiFixture.AdminPassword)
            .ReadTokensAsync();

        return tokens.AccessToken;
    }

    private async Task<Guid> RegisterStudentAsync()
    {
        using var response = await _client.RegisterAsync(AuthRequests.UniqueEmail("grantee"));
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var account = await response.Content.ReadFromJsonAsync<RegisteredAccount>();
        return account.ShouldNotBeNull().UserId;
    }

    private async Task<HttpResponseMessage> GrantAsync(string token, Guid userId, string slug)
    {
        using var request = AuthRequests.Bearer(
            HttpMethod.Post,
            $"/api/admin/users/{userId}/grant-instructor",
            token);

        request.Content = JsonContent.Create(new { slug, headline = "Principal engineer" });

        return await _client.SendAsync(request);
    }

    // ---- authorisation ----------------------------------------------------------------

    /// <summary>
    /// A signed-in student is not an admin.
    /// </summary>
    /// <remarks>
    /// This is the assertion that matters most in the file. If it ever fails, any registered
    /// user can promote themselves to instructor and publish to the whole platform — the exact
    /// thing curated onboarding exists to prevent.
    /// </remarks>
    [Fact]
    public async Task Grant_is_forbidden_for_a_student()
    {
        var student = await _client.RegisterAndLoginAsync("not-admin");
        var target = await RegisterStudentAsync();

        using var response = await GrantAsync(student.AccessToken, target, Slug("nope"));

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Grant_is_unauthorized_for_an_anonymous_caller()
    {
        using var response = await _client.PostAsJsonAsync(
            $"/api/admin/users/{Guid.CreateVersion7()}/grant-instructor",
            new { slug = "anon", headline = "Nope" });

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Finding_users_is_forbidden_for_a_student()
    {
        var student = await _client.RegisterAndLoginAsync("no-list");

        using var request = AuthRequests.Bearer(HttpMethod.Get, "/api/admin/users", student.AccessToken);
        using var response = await _client.SendAsync(request);

        // The one endpoint that lists accounts. Below Admin it would be the enumeration oracle
        // that login is carefully built not to be.
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// The seeded admin holds <c>Student</c> too, so <c>/api/me</c> answers for them.
    /// </summary>
    /// <remarks>
    /// "Every registered user holds Student" (02-domain-model.md §2) is an invariant the
    /// authorisation model leans on — <c>AuthPolicies.Student</c> means "we know who you are"
    /// and is implemented as <c>RequireRole(Student)</c>. The seeder creates a user directly
    /// rather than going through registration, so it has to uphold the invariant itself.
    /// Caught against a live stack: the admin got 403 from <c>/api/me</c>, contradicting the
    /// authorisation matrix in 03-api-design.md §7.
    /// </remarks>
    [Fact]
    public async Task Seeded_admin_can_read_api_me()
    {
        using var request = AuthRequests.Bearer(HttpMethod.Get, "/api/me", await AdminTokenAsync());
        using var response = await _client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var me = await response.Content.ReadFromJsonAsync<MeResponse>();
        me.ShouldNotBeNull();
        me.Roles.ShouldContain("Admin");
        me.Roles.ShouldContain("Student");
    }

    // ---- granting ---------------------------------------------------------------------

    [Fact]
    public async Task Grant_adds_the_role_and_creates_a_profile()
    {
        var token = await AdminTokenAsync();
        var userId = await RegisterStudentAsync();
        var slug = Slug("granted");

        using var response = await GrantAsync(token, userId, slug);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var grant = await response.Content.ReadFromJsonAsync<Grant>();
        grant.ShouldNotBeNull();
        grant.UserId.ShouldBe(userId);
        grant.InstructorSlug.ShouldBe(slug);
        grant.Roles.ShouldContain("Instructor");
        grant.Roles.ShouldContain("Student");
    }

    /// <summary>
    /// The field the web app reads to decide whether to show the Studio link.
    /// </summary>
    /// <remarks>
    /// It was hardcoded to null from <c>A-1</c> until this card, with a comment saying so. This
    /// is the test that stops it silently reverting to a constant.
    /// </remarks>
    [Fact]
    public async Task Granted_instructor_sees_their_slug_on_api_me()
    {
        var email = AuthRequests.UniqueEmail("studio-link");
        using var registered = await _client.RegisterAsync(email);
        var account = await registered.Content.ReadFromJsonAsync<RegisteredAccount>();
        account.ShouldNotBeNull();

        var slug = Slug("studio");
        using var granted = await GrantAsync(await AdminTokenAsync(), account.UserId, slug);
        granted.StatusCode.ShouldBe(HttpStatusCode.OK);

        // A fresh login: the role and slug have to survive into a newly issued token.
        var tokens = await _client.LoginAsync(email).ReadTokensAsync();

        using var request = AuthRequests.Bearer(HttpMethod.Get, "/api/me", tokens.AccessToken);
        using var response = await _client.SendAsync(request);

        var me = await response.Content.ReadFromJsonAsync<MeResponse>();
        me.ShouldNotBeNull();
        me.InstructorSlug.ShouldBe(slug);
        me.Roles.ShouldContain("Instructor");
    }

    [Fact]
    public async Task Granting_the_same_slug_to_someone_else_conflicts()
    {
        var token = await AdminTokenAsync();
        var slug = Slug("taken");

        using var first = await GrantAsync(token, await RegisterStudentAsync(), slug);
        first.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var second = await GrantAsync(token, await RegisterStudentAsync(), slug);

        second.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    /// <summary>Granting twice is a retry, not a conflict.</summary>
    [Fact]
    public async Task Granting_the_same_user_twice_is_idempotent()
    {
        var token = await AdminTokenAsync();
        var userId = await RegisterStudentAsync();
        var slug = Slug("twice");

        using var first = await GrantAsync(token, userId, slug);
        first.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var second = await GrantAsync(token, userId, slug);

        second.StatusCode.ShouldBe(HttpStatusCode.OK);
        var grant = await second.Content.ReadFromJsonAsync<Grant>();
        grant.ShouldNotBeNull().InstructorSlug.ShouldBe(slug);
    }

    [Theory]
    [InlineData("Jane Doe")]
    [InlineData("../admin")]
    [InlineData("jane--doe")]
    [InlineData("")]
    public async Task Grant_rejects_a_malformed_slug(string slug)
    {
        var token = await AdminTokenAsync();

        using var response = await GrantAsync(token, await RegisterStudentAsync(), slug);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Grant_returns_404_for_an_unknown_user()
    {
        var token = await AdminTokenAsync();

        using var response = await GrantAsync(token, Guid.CreateVersion7(), Slug("ghost"));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // ---- revoking ---------------------------------------------------------------------

    /// <summary>
    /// Revoking removes the role and keeps the profile.
    /// </summary>
    /// <remarks>
    /// The slug stays reserved deliberately: course pages still name the author, and releasing
    /// it would let a later instructor inherit someone else's URL.
    /// </remarks>
    [Fact]
    public async Task Revoke_removes_the_role_but_keeps_the_profile()
    {
        var token = await AdminTokenAsync();
        var email = AuthRequests.UniqueEmail("revoked");

        using var registered = await _client.RegisterAsync(email);
        var account = await registered.Content.ReadFromJsonAsync<RegisteredAccount>();
        account.ShouldNotBeNull();

        var slug = Slug("revoked");
        using var granted = await GrantAsync(token, account.UserId, slug);
        granted.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var revokeRequest = AuthRequests.Bearer(
            HttpMethod.Post,
            $"/api/admin/users/{account.UserId}/revoke-instructor",
            token);
        using var revoked = await _client.SendAsync(revokeRequest);
        revoked.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // A fresh token, because the old one still carries the Instructor role until it expires
        // — the 15-minute lifetime *is* the revocation window.
        var tokens = await _client.LoginAsync(email).ReadTokensAsync();
        using var meRequest = AuthRequests.Bearer(HttpMethod.Get, "/api/me", tokens.AccessToken);
        using var me = await _client.SendAsync(meRequest);

        var body = await me.Content.ReadFromJsonAsync<MeResponse>();
        body.ShouldNotBeNull();
        body.Roles.ShouldNotContain("Instructor");
        body.InstructorSlug.ShouldBe(slug, "the profile and its slug outlive the role");
    }

    [Fact]
    public async Task Revoking_someone_who_is_not_an_instructor_succeeds()
    {
        var token = await AdminTokenAsync();

        using var request = AuthRequests.Bearer(
            HttpMethod.Post,
            $"/api/admin/users/{await RegisterStudentAsync()}/revoke-instructor",
            token);
        using var response = await _client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    // ---- finding ----------------------------------------------------------------------

    [Fact]
    public async Task Find_users_locates_an_account_by_email()
    {
        var token = await AdminTokenAsync();
        var email = AuthRequests.UniqueEmail("findable");

        using var registered = await _client.RegisterAsync(email);
        registered.StatusCode.ShouldBe(HttpStatusCode.Created);

        using var request = AuthRequests.Bearer(
            HttpMethod.Get,
            $"/api/admin/users?search={Uri.EscapeDataString(email)}",
            token);
        using var response = await _client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var page = await response.Content.ReadFromJsonAsync<PagedBody<AdminUserRow>>();
        page.ShouldNotBeNull();
        page.TotalCount.ShouldBe(1);
        page.Items.ShouldHaveSingleItem().Email.ShouldBe(email);
    }

    /// <summary>The search is case-insensitive — an admin pasting an address should not have to match casing.</summary>
    [Fact]
    public async Task Find_users_ignores_case()
    {
        var token = await AdminTokenAsync();
        var email = AuthRequests.UniqueEmail("MixedCase");

        using var registered = await _client.RegisterAsync(email);
        registered.StatusCode.ShouldBe(HttpStatusCode.Created);

        using var request = AuthRequests.Bearer(
            HttpMethod.Get,
            $"/api/admin/users?search={Uri.EscapeDataString(email.ToUpperInvariant())}",
            token);
        using var response = await _client.SendAsync(request);

        var page = await response.Content.ReadFromJsonAsync<PagedBody<AdminUserRow>>();
        page.ShouldNotBeNull();
        page.TotalCount.ShouldBe(1);
    }

    /// <summary>A list view must never carry a password hash or security stamp off the module.</summary>
    [Fact]
    public async Task Find_users_returns_no_credential_fields()
    {
        var token = await AdminTokenAsync();

        using var request = AuthRequests.Bearer(HttpMethod.Get, "/api/admin/users", token);
        using var response = await _client.SendAsync(request);

        var raw = await response.Content.ReadAsStringAsync();

        raw.ShouldNotContain("passwordHash", Case.Insensitive);
        raw.ShouldNotContain("securityStamp", Case.Insensitive);
        raw.ShouldNotContain("concurrencyStamp", Case.Insensitive);
    }

    [Fact]
    public async Task Find_users_pages()
    {
        var token = await AdminTokenAsync();

        using var request = AuthRequests.Bearer(
            HttpMethod.Get,
            "/api/admin/users?page=1&pageSize=2",
            token);
        using var response = await _client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var page = await response.Content.ReadFromJsonAsync<PagedBody<AdminUserRow>>();
        page.ShouldNotBeNull();
        page.PageSize.ShouldBe(2);
        page.Items.Count.ShouldBeLessThanOrEqualTo(2);
    }
}
