<#
.SYNOPSIS
    Wipes local development data (PostgreSQL and Azurite volumes).

.DESCRIPTION
    Local containers are persistent by design (see artifacts/design/06-tech-stack.md §3.1),
    so restarting the AppHost does NOT clear database state. The reset is the volume, not
    the process. This is the one known fix for a broken local database.

    Stop the AppHost before running this.

.PARAMETER Database
    Reset only the PostgreSQL volume.

.PARAMETER Blobs
    Reset only the Azurite volume.

.EXAMPLE
    ./scripts/reset-local-data.ps1
    Resets everything, with a confirmation prompt.

.EXAMPLE
    ./scripts/reset-local-data.ps1 -Database -Force
    Resets only the database, no prompt.
#>
[CmdletBinding()]
param(
    [switch]$Database,
    [switch]$Blobs,
    [switch]$Force
)

$ErrorActionPreference = 'Stop'

# No target flags means "everything".
if (-not $Database -and -not $Blobs) {
    $Database = $true
    $Blobs = $true
}

$volumes = @()
if ($Database) { $volumes += 'lms-postgres-data' }
if ($Blobs)    { $volumes += 'lms-azurite-data' }

Write-Host "About to delete these Docker volumes:" -ForegroundColor Yellow
$volumes | ForEach-Object { Write-Host "  - $_" }

if (-not $Force) {
    $answer = Read-Host "This permanently destroys local data. Type 'yes' to continue"
    if ($answer -ne 'yes') {
        Write-Host "Aborted. Nothing was deleted." -ForegroundColor Green
        exit 0
    }
}

# Persistent containers hold a lock on their volume, so they must go first.
Write-Host "`nStopping containers using these volumes..." -ForegroundColor Cyan
foreach ($volume in $volumes) {
    $containers = docker ps -aq --filter "volume=$volume"
    foreach ($container in $containers) {
        if ($container) {
            docker rm -f $container | Out-Null
            Write-Host "  removed container $container"
        }
    }
}

Write-Host "`nRemoving volumes..." -ForegroundColor Cyan
foreach ($volume in $volumes) {
    $exists = docker volume ls -q --filter "name=^$volume$"
    if ($exists) {
        docker volume rm $volume | Out-Null
        Write-Host "  removed $volume" -ForegroundColor Green
    }
    else {
        Write-Host "  $volume does not exist — nothing to do"
    }
}

Write-Host "`nDone. The next AppHost run will start from empty." -ForegroundColor Green
