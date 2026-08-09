using Lms.Modules.Identity;
using Lms.Modules.Identity.Infrastructure;
using Lms.Modules.Notifications;
using Lms.Modules.Notifications.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(Lms.IntegrationTests.LmsApiFixture))]

namespace Lms.IntegrationTests;

/// <summary>
/// One real PostgreSQL and one in-process API for the whole assembly.
/// </summary>
/// <remarks>
/// A container per test class would multiply a five-second startup by every class for no
/// isolation benefit: tests use unique email addresses rather than a shared fixture user, so
/// they do not collide and the suite can stay parallel.
/// <para>
/// This is the harness <c>01-architecture.md §5</c> refers to when it says repository
/// interfaces are not needed — the argument for wrapping <c>DbContext</c> is testability, and
/// it only holds if the alternative is an in-memory fake. Against real PostgreSQL it does not:
/// an in-memory provider would not run the SQL the app actually emits.
/// </para>
/// </remarks>
public sealed class LmsApiFixture : IAsyncLifetime
{
    /// <summary>Matches the AppHost, so tests and local development run the same server.</summary>
    private const string PostgresImage = "postgres:18.3";

    /// <summary>
    /// Not the committed development key. The test host runs as Production, so
    /// <see cref="JwtOptionsValidator"/> actually executes here — supplying a distinct key means
    /// the tests exercise the same validation path a deployment does.
    /// </summary>
    private const string TestSigningKey = "INTEGRATION-TEST-SIGNING-KEY-not-the-development-one-91b4d7";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder(PostgresImage)
        .WithDatabase("lmsdb")
        .Build();

    private WebApplicationFactory<Program>? _api;

    private WebApplicationFactory<Program> Api =>
        _api ?? throw new InvalidOperationException("Fixture has not been initialised.");

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();

        // Migrate before the API starts, mirroring how the AppHost gates the API on the
        // migration job with WaitForCompletion. The app itself never migrates.
        await MigrateAsync();

        // The general-purpose host. Its auth rate limit is deliberately high: every request
        // here arrives with no RemoteIpAddress, so the whole suite shares one partition and
        // the production limit of ten would be spent within a couple of tests.
        _api = CreateApi(new Dictionary<string, string?>
        {
            ["RateLimiting:Auth:PermitLimit"] = "10000",
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (_api is not null)
        {
            await _api.DisposeAsync();
        }

        await _postgres.DisposeAsync();
    }

    /// <summary>A client against the shared host. Use this unless the test is about startup.</summary>
    public HttpClient CreateClient() => Api.CreateClient();

    /// <summary>
    /// A separate host over the same database, with configuration overrides applied.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used by the rate-limit test, which needs a low permit limit without imposing it on
    /// every other test. A second <see cref="WebApplicationFactory{T}"/> is cheap; a second
    /// container would not be.
    /// </para>
    /// <para>
    /// <b><c>UseSetting</c>, not <c>ConfigureAppConfiguration</c>.</b> The latter is appended
    /// after <c>Program.cs</c> has already run, so anything read eagerly there — the signing
    /// key, the rate limits — sees the app's own <c>appsettings.json</c> instead. Only the
    /// connection string survived that, and only because <c>AddDbContext</c>'s options lambda
    /// runs lazily. <c>UseSetting</c> goes into host configuration before the builder exists,
    /// so every read sees it.
    /// </para>
    /// </remarks>
    public WebApplicationFactory<Program> CreateApi(Dictionary<string, string?> settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        settings["ConnectionStrings:lmsdb"] = _postgres.GetConnectionString();
        settings["Jwt:SigningKey"] = TestSigningKey;

        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            foreach (var (key, value) in settings)
            {
                builder.UseSetting(key, value);
            }
        });
    }

    /// <summary>
    /// Applies every module's migrations.
    /// </summary>
    /// <remarks>
    /// The context list mirrors <c>Lms.MigrationService/Program.cs</c>. Add a module there and
    /// here together — a missing entry shows up as a table-not-found failure in whichever test
    /// touches it first, which is a survivable way to find out.
    /// </remarks>
    private async Task MigrateAsync()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:lmsdb"] = _postgres.GetConnectionString(),
            })
            .Build();

        services.AddIdentityPersistence(configuration);
        services.AddNotificationsPersistence(configuration);

        await using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        await scope.ServiceProvider.GetRequiredService<IdentityModuleDbContext>()
            .Database.MigrateAsync();
        await scope.ServiceProvider.GetRequiredService<NotificationsDbContext>()
            .Database.MigrateAsync();
    }
}
