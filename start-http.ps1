#Requires -Version 7
param(
    [int]$Port = 5883,
    [string]$ExeName = "yandex-search-mcp-dotnet",
    [string]$HostIP = "0.0.0.0"
)

$ErrorActionPreference = "Stop"

$apiKey = $env:YANDEX_CLOUD_API_KEY
$folderId = $env:YANDEX_CLOUD_FOLDER_ID

if (-not $apiKey) { Write-Error "YANDEX_CLOUD_API_KEY env var is not set"; exit 1 }
if (-not $folderId) { Write-Error "YANDEX_CLOUD_FOLDER_ID env var is not set"; exit 1 }

Set-Location $PSScriptRoot

# Kill previous instance if it owns the port
$conn = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue | Select-Object -First 1
if ($conn) {
    $proc = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
    if ($proc -and $proc.ProcessName -eq $ExeName) {
        Write-Host "Stopping previous instance (PID $($proc.Id))..."
        $proc.Kill($true)
        $proc.WaitForExit(3000)
    }
}

Write-Host "Building and starting on http://${HostIP}:$Port"
dotnet run -c Release -- --api-key $apiKey --folder-id $folderId --transport http --host $HostIP --port $Port
