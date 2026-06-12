param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path,
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

Set-Location $ProjectRoot

Write-Host "Publishing Windows package..." -ForegroundColor Cyan

$outDir = Join-Path $ProjectRoot "publish-win"
if (Test-Path $outDir) {
    Remove-Item $outDir -Recurse -Force
}

$projectFile = Join-Path $ProjectRoot "LedImageUpdaterService.csproj"

& dotnet publish $projectFile `
    --configuration Release `
    --runtime $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:DebugType=none `
    --output $outDir

if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish failed." -ForegroundColor Red
    exit $LASTEXITCODE
}

$requiredPublishFiles = @(
    "LedImageUpdaterService.exe",
    "appsettings.json",
    "YQNetCom.dll",
    "libcurl.dll",
    "libssl-1_1-x64.dll",
    "libcrypto-1_1-x64.dll",
    "BouncyCastle.Crypto.dll",
    "msvcr100.dll",
    "msvcr110.dll",
    "msvcr120.dll"
)

$missing = @()
foreach ($name in $requiredPublishFiles) {
    $path = Join-Path $outDir $name
    if (-not (Test-Path $path)) {
        $missing += $name
    }
}

if ($missing.Count -gt 0) {
    Write-Host "Publish completed, but required files are missing:" -ForegroundColor Yellow
    $missing | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }
    exit 2
}

Write-Host "Publish completed successfully: $outDir" -ForegroundColor Green
Write-Host "Next: copy publish-win to target Windows machine and run install-service.ps1 as Administrator." -ForegroundColor Cyan
