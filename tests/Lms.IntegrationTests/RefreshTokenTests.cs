namespace Lms.IntegrationTests;

/// <summary>Rotation, replay detection and logout — the behaviours introduced by <c>A-2</c>.</summary>
public sealed class RefreshTokenTests(LmsApiFixture fixture)
{
    private readonly HttpClient _client = fixture.CreateClient();

    [Fact]
    public async Task Refresh_issues_a_new_pair()
    {
        var original = await _client.RegisterAndLoginAsync("refresh");

        var rotated = await _client.RefreshAsync(original.RefreshToken).ReadTokensAsync();

        rotated.RefreshToken.ShouldNotBe(original.RefreshToken);
        rotated.AccessToken.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Refresh_tokens_are_single_use()
    {
        var original = await _client.RegisterAndLoginAsync("single-use");
        await _client.RefreshAsync(original.RefreshToken).ReadTokensAsync();

        using var replay = await _client.RefreshAsync(original.RefreshToken);

        replay.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Replaying a rotated token revokes the entire chain, including the live replacement.
    /// </summary>
    /// <remarks>
    /// A rotated-away token reappearing means someone is holding a copy they should not have.
    /// Rejecting only the replayed token would leave the thief's *other* token — or the victim's
    /// — working for fourteen days. Killing the chain turns a silent theft into a visible
    /// logout, which is the only signal either party gets.
    /// </remarks>
    [Fact]
    public async Task Replaying_a_rotated_token_revokes_the_whole_chain()
    {
        var original = await _client.RegisterAndLoginAsync("chain");
        var rotated = await _client.RefreshAsync(original.RefreshToken).ReadTokensAsync();

        // The replay. This is the moment the theft becomes detectable.
        using var replay = await _client.RefreshAsync(original.RefreshToken);
        replay.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        // The replacement was valid a line ago and must now be dead too.
        using var replacement = await _client.RefreshAsync(rotated.RefreshToken);
        replacement.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_revokes_the_refresh_token()
    {
        var tokens = await _client.RegisterAndLoginAsync("logout");

        using var loggedOut = await _client.LogoutAsync(tokens.RefreshToken);
        loggedOut.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        using var afterwards = await _client.RefreshAsync(tokens.RefreshToken);
        afterwards.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Logout succeeds for a token that never existed.
    /// </summary>
    /// <remarks>
    /// A 404 here would make the endpoint an oracle for whether a given token is real. It also
    /// has to stay 204 for the BFF's sake: the web app calls logout on every sign-out, including
    /// ones where the session had already expired.
    /// </remarks>
    [Fact]
    public async Task Logout_succeeds_for_an_unknown_token()
    {
        using var response = await _client.LogoutAsync("this-token-was-never-issued");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Logout answers 204 with no body.
    /// </summary>
    /// <remarks>
    /// Pinned because it is load-bearing for the BFF and was got wrong twice. <c>A-2</c> found
    /// the generic <c>ToHttpResult</c> answering <c>200 {}</c>; <c>A-3</c>'s sign-out defect was
    /// the web client calling <c>response.json()</c> on the resulting empty body. Both bugs live
    /// on this one line of contract.
    /// </remarks>
    [Fact]
    public async Task Logout_returns_204_with_an_empty_body()
    {
        var tokens = await _client.RegisterAndLoginAsync("204");

        using var response = await _client.LogoutAsync(tokens.RefreshToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await response.Content.ReadAsStringAsync()).ShouldBeEmpty();
    }
}
