// Local development orchestration — see artifacts/design/06-tech-stack.md §3.1.
//
// Data is long-lived on purpose. Both settings are required and do different jobs:
//   WithDataVolume()                        -> a named Docker volume, so bytes outlive the container
//   WithLifetime(ContainerLifetime.Persistent) -> the container itself survives between AppHost runs
//
// Together: stop debugging, come back tomorrow, and your seeded instructor, draft courses and
// uploaded thumbnails are still there. To wipe it, run scripts/reset-local-data.ps1 — a broken
// local database has one known fix, not five improvised ones.

var builder = DistributedApplication.CreateBuilder(args);

// --- PostgreSQL ---------------------------------------------------------------
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("lms-postgres-data")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgWeb();

var lmsDb = postgres.AddDatabase("lmsdb");

// --- Blob storage (Azurite) ----------------------------------------------------
// Persisted for the same reason: re-uploading test assets every session is exactly the
// friction that leads to the SAS upload path being skipped and shipped untested.
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(azurite =>
    {
        azurite.WithDataVolume("lms-azurite-data")
               .WithLifetime(ContainerLifetime.Persistent);
    });

// AddBlobContainer, not AddBlobs: the latter only names the blob service endpoint, whereas
// this actually creates the container. The two containers differ in access — see
// artifacts/design/05-adr-video-and-storage.md §3.
var courseAssets = storage.AddBlobContainer("course-assets");
var lessonAttachments = storage.AddBlobContainer("lesson-attachments");

// --- Migrations ------------------------------------------------------------------
// Runs to completion before the API starts. Never migrate at API startup — with more than
// one replica that is a race (artifacts/design/01-architecture.md §7).
var migrations = builder.AddProject<Projects.Lms_MigrationService>("migrations")
    .WithReference(lmsDb)
    .WaitFor(postgres);

// --- Application ---------------------------------------------------------------
var api = builder.AddProject<Projects.Lms_Api>("api")
    .WithReference(lmsDb)
    .WithReference(courseAssets)
    .WithReference(lessonAttachments)
    .WaitForCompletion(migrations);

// TanStack Start. WithReference(api) injects services__api__http__0, which the Start
// server reads to call the API — see web/src/server/api.ts. The browser never gets it.
builder.AddViteApp("web", "../../web")
    .WithReference(api)
    .WaitFor(api)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints();

builder.Build().Run();
