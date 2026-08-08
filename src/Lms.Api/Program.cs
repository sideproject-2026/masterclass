using Lms.Modules.Catalog;
using Lms.Modules.Enrollment;
using Lms.Modules.Identity;
using Lms.Modules.Media;
using Lms.Modules.Notifications;
using Lms.SharedKernel.Events;
using Lms.SharedKernel.Time;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<IEventBus, InProcessEventBus>();

builder.Services
    .AddIdentityModule(builder.Configuration)
    .AddCatalogModule(builder.Configuration)
    .AddEnrollmentModule(builder.Configuration)
    .AddMediaModule(builder.Configuration)
    .AddNotificationsModule(builder.Configuration);

var app = builder.Build();

// Placeholder until F-3 adds ServiceDefaults, real health checks and OpenAPI.
app.MapGet("/health/live", () => Results.Ok(new { status = "healthy" }));

app.MapIdentityEndpoints()
   .MapCatalogEndpoints()
   .MapEnrollmentEndpoints()
   .MapMediaEndpoints()
   .MapNotificationsEndpoints();

await app.RunAsync();

/// <summary>Exposed so integration tests can drive the host via WebApplicationFactory.</summary>
public partial class Program;
