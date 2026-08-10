# syntax=docker/dockerfile:1

# The migration job image.
#   docker build -f src/Lms.MigrationService/Dockerfile -t lms-migrations .
#
# This is a JOB, not a service: it applies every module's migrations, exits non-zero on
# failure, and stops. `D-3` runs it as the pre-deploy step that gates the API rollout —
# migrations must never run at API startup, which races across replicas
# (01-architecture.md §7). A host that cannot gate a rollout on a job is disqualified
# (10-adr-hosting.md §2).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props Directory.Packages.props ./
COPY .config/ .config/
COPY src/ src/

RUN dotnet restore src/Lms.MigrationService/Lms.MigrationService.csproj

RUN dotnet publish src/Lms.MigrationService/Lms.MigrationService.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# runtime, not aspnet: this never serves HTTP.
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

USER app

ENTRYPOINT ["dotnet", "Lms.MigrationService.dll"]
