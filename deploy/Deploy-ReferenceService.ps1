param(
    [Parameter(Mandatory = $true)]
    [string]$PackagePath,

    [Parameter(Mandatory = $true)]
    [string]$InstallPath,

    [Parameter(Mandatory = $true)]
    [string]$ServiceName
)

$ErrorActionPreference = 'Stop'

Write-Host "Deploy başlıyor: $ServiceName"

$service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if ($service -and $service.Status -ne 'Stopped') {
    Stop-Service -Name $ServiceName -Force
    $service.WaitForStatus('Stopped', [TimeSpan]::FromSeconds(30))
}

$backupRoot = Join-Path $InstallPath '_backup'
$backupPath = Join-Path $backupRoot (Get-Date -Format 'yyyyMMdd_HHmmss')

if (Test-Path $InstallPath) {
    New-Item -ItemType Directory -Force -Path $backupPath | Out-Null

    Get-ChildItem -Path $InstallPath -Force |
        Where-Object { $_.Name -ne '_backup' } |
        Copy-Item -Destination $backupPath -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $InstallPath | Out-Null
Copy-Item -Path (Join-Path $PackagePath '*') -Destination $InstallPath -Recurse -Force

if ($service) {
    Start-Service -Name $ServiceName
    (Get-Service -Name $ServiceName).WaitForStatus('Running', [TimeSpan]::FromSeconds(30))
}

Write-Host "Deploy tamamlandı: $ServiceName"
Write-Host "Backup: $backupPath"
