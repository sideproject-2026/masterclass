using Lms.Modules.Notifications;
using Lms.Modules.Notifications.Infrastructure;
using Lms.MigrationService;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHostedService<MigrationRunner>();
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(MigrationRunner.ActivitySourceName));

// Each module registers its own DbContext, exactly as the API does.
// Add a line here when a module gains one.
builder.Services.AddNotificationsModule(builder.Configuration);

// The runner resolves DbContext, not the concrete types, so it stays agnostic of how
// many modules exist.
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<NotificationsDbContext>());

var host = builder.Build();
await host.RunAsync();
