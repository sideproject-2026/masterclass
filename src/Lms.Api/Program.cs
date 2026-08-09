using Lms.Api;
using Lms.Modules.Catalog;
using Lms.Modules.Enrollment;
using Lms.Modules.Identity;
using Lms.Modules.Media;
using Lms.Modules.Notifications;
using Lms.SharedKernel.Events;
using Lms.SharedKernel.Http;
using Lms.SharedKernel.Time;
using Microsoft.AspNetCore.Http.Json;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// OpenTelemetry, health checks, service discovery and HTTP resilience.
// Same code path locally and in Azure — only the OTLP destination differs.
builder.AddServiceDefaults();

builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<IEventBus, InProcessEventBus>();

// Every unhandled exception becomes ProblemDetails; nothing leaks a stack trace.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Enums on the wire are strings, so reordering one is not a breaking change.
builder.Services.Configure<JsonOptions>(options =>
    options.SerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter()));

// .NET 10 generates the OpenAPI document in-box — no Swashbuckle.
builder.Services.AddOpenApi();

// Validates bearer tokens; never issues them. See AuthenticationSetup.
builder.Services.AddLmsAuthentication(builder.Configuration);
builder.Services.AddLmsRateLimiting(builder.Configuration);

builder.Services
    .AddIdentityModule(builder.Configuration)
    .AddCatalogModule(builder.Configuration)
    .AddEnrollmentModule(builder.Configuration)
    .AddMediaModule(builder.Configuration)
    .AddNotificationsModule(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseRateLimiter();

// Order matters: authentication resolves who the caller is, authorisation then decides.
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();              // /openapi/v1.json
    app.MapScalarApiReference();   // /scalar/v1 — Microsoft.AspNetCore.OpenApi ships no UI
}

// /health/live and /health/ready — excluded from the OpenAPI document.
app.MapDefaultEndpoints();

app.MapIdentityEndpoints()
   .MapCatalogEndpoints()
   .MapEnrollmentEndpoints()
   .MapMediaEndpoints()
   .MapNotificationsEndpoints();

await app.RunAsync();

/// <summary>Exposed so integration tests can drive the host via WebApplicationFactory.</summary>
public partial class Program;
