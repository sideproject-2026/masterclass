# syntax=docker/dockerfile:1

# The API image. Built from the repository root, not from this directory:
#   docker build -f src/Lms.Api/Dockerfile -t lms-api .
# because the build needs Directory.Build.props and every referenced project.

# ---- build ----------------------------------------------------------------
# The tag must satisfy global.json. That file pins a 10.0.4xx SDK with
# rollForward=latestPatch, so a lower feature band in the image fails the restore with a
# message about the SDK, not about the code — worth knowing before you go looking in C#.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copied first and separately so a source-only change does not invalidate the restore layer.
# The whole source tree follows rather than a list of fifteen .csproj paths: the list is a
# maintenance burden that breaks silently the first time somebody adds a project, and the
# cache win over a repo this size does not pay for that.
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY .config/ .config/

COPY src/ src/

RUN dotnet restore src/Lms.Api/Lms.Api.csproj

# No --no-restore: restore ran above, and publish is quiet about a stale one.
RUN dotnet publish src/Lms.Api/Lms.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ---- runtime --------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# 8080, not 80: the aspnet images have defaulted to a non-privileged port since .NET 8
# precisely so the container does not need root to bind it.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

# The 'app' user ships in the image. Running as root inside a container is a habit that costs
# nothing to break here and a lot to break later.
USER app

ENTRYPOINT ["dotnet", "Lms.Api.dll"]
