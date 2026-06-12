param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
)

$ErrorActionPreference = "Stop"

function Fail($message) {
    Write-Host "[FAIL] $message" -ForegroundColor Red
    exit 1
}

function Ok($message) {
    Write-Host "[OK]   $message" -ForegroundColor Green
}

Write-Host "Checking Windows migration prerequisites..." -ForegroundColor Cyan

if (-not $IsWindows) {
    Fail "This script must be run on Windows."
}
Ok "Windows OS detected."

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    Fail "dotnet SDK is not installed or not in PATH."
}

$versionText = (& dotnet --version).Trim()
$major = [int]($versionText.Split('.')[0])
if ($major -lt 8) {
    Fail "dotnet SDK 8+ required. Current: $versionText"
}
Ok "dotnet SDK version: $versionText"

$requiredFiles = @(
    "LedImageUpdaterService.csproj",
    "appsettings.json",
    "Program.cs",
    "Services/OnbonLedController.cs",
    "install-service.ps1",
    "publish-win.bat"
)

foreach ($rel in $requiredFiles) {
    $path = Join-Path $ProjectRoot $rel
    if (-not (Test-Path $path)) {
        Fail "Required file missing: $rel"
    }
}
Ok "Required project files exist."

$appsettingsPath = Join-Path $ProjectRoot "appsettings.json"
$appsettingsRaw = Get-Content $appsettingsPath -Raw

if ($appsettingsRaw -notmatch '"ScreenWidth"\s*:\s*560') {
    Fail "OnbonLed.ScreenWidth is not 560 in appsettings.json"
}
if ($appsettingsRaw -notmatch '"ScreenHeight"\s*:\s*80') {
    Fail "OnbonLed.ScreenHeight is not 80 in appsettings.json"
}
Ok "Onbon dimensions are set to 560x80."

if ($appsettingsRaw -match '"AutoSend"\s*:\s*true') {
    Ok "AutoSend is enabled."
}
else {
    Write-Host "[WARN] AutoSend is disabled. Background upload will not run." -ForegroundColor Yellow
}

$csprojPath = Join-Path $ProjectRoot "LedImageUpdaterService.csproj"
$csprojRaw = Get-Content $csprojPath -Raw
if ($csprojRaw -notmatch 'Compile Remove="BX_Y_CSharp_SDK\\\*\*"') {
    Write-Host "[WARN] BX_Y_CSharp_SDK source exclusion was not found in csproj text." -ForegroundColor Yellow
    Write-Host "       If duplicate namespace errors appear, re-check exclusion block." -ForegroundColor Yellow
}
else {
    Ok "BX_Y_CSharp_SDK source exclusion found in csproj."
}

Write-Host "Environment check completed." -ForegroundColor Cyan
