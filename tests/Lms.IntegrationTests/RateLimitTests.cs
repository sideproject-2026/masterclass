namespace Lms.IntegrationTests;

/// <summary>
/// The <c>/api/auth/*</c> rate limiter from <c>A-2</c>, driven on its own host.
/// </summary>
/// <remarks>
/// Its own host because the limit is per caller and every request under
/// <c>WebApplicationFactory</c> has no <c>RemoteIpAddress</c> — so the whole assembly shares one
/// partition. Running this at the production limit of ten would starve unrelated tests; running
/// the other tests at a low limit would make them flaky. A second factory over the same
/// container costs nothing.
/// </remarks>
public sealed class RateLimitTests(LmsApiFixture fixture)
{
    private const int PermitLimit = 3;

    [Fact]
    public async Task Auth_endpoints_reject_a_caller_over_the_limit()
    {
        await using var api = fixture.CreateApi(new Dictionary<string, string?>
        {
            ["RateLimiting:Auth:PermitLimit"] = PermitLimit.ToString(CultureInfo.InvariantCulture),
            ["RateLimiting:Auth:WindowMinutes"] = "5",
        });

        using var client = api.CreateClient();
        var statuses = new List<HttpStatusCode>();

        // One over the limit. The failures are deliberate — credential stuffing is what the
        // limiter exists for, so the test drives it the way an attacker would.
        for (var attempt = 0; attempt < PermitLimit + 1; attempt++)
        {
            using var response = await client.LoginAsync(
                AuthRequests.UniqueEmail("flood"),
                "wrong-password");

            statuses.Add(response.StatusCode);
        }

        statuses.Take(PermitLimit).ShouldAllBe(status => status == HttpStatusCode.Unauthorized);
        statuses[^1].ShouldBe(HttpStatusCode.TooManyRequests);
    }

    /// <summary>
    /// Rate limiting applies to the auth group and not to the rest of the API.
    /// </summary>
    /// <remarks>
    /// A limiter accidentally attached at the root would throttle ordinary browsing under load
    /// and be diagnosed as a performance problem rather than a configuration one.
    /// </remarks>
    [Fact]
    public async Task Health_is_not_rate_limited()
    {
        await using var api = fixture.CreateApi(new Dictionary<string, string?>
        {
            ["RateLimiting:Auth:PermitLimit"] = "1",
            ["RateLimiting:Auth:WindowMinutes"] = "5",
        });

        using var client = api.CreateClient();

        for (var attempt = 0; attempt < 5; attempt++)
        {
            using var response = await client.GetAsync("/health/live");
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }
}
