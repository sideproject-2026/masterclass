using Lms.Modules.Identity;
using Lms.Modules.Identity.Infrastructure;
using Lms.Modules.Notifications;
using Lms.Modules.Notifications.Infrastructure;
using Lms.MigrationService;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHostedService<MigrationRunner>();
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(MigrationRunner.ActivitySourceName));

// Persistence only — this job needs each module's schema, not its endpoints, handlers or
// authentication. Add a line here when a module gains a DbContext.
builder.Services.AddIdentityPersistence(builder.Configuration);
builder.Services.AddNotificationsPersistence(builder.Configuration);

// The runner resolves DbContext, not the concrete types, so it stays agnostic of how
// many modules exist.
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<IdentityModuleDbContext>());
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<NotificationsDbContext>());

var host = builder.Build();
await host.RunAsync();
