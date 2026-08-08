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

// --- Application ---------------------------------------------------------------
// Lms.MigrationService (F-4) and the TanStack Start app (F-7) join here as they land.
builder.AddProject<Projects.Lms_Api>("api")
    .WithReference(lmsDb)
    .WithReference(courseAssets)
    .WithReference(lessonAttachments)
    .WaitFor(postgres);

builder.Build().Run();
