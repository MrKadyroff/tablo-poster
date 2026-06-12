# Windows Migration Pack

This package helps you move development and deployment of TabloPosterService to a Windows machine.

## 1. What you get

- Predictable setup steps for a fresh Windows PC.
- Environment validation before first run.
- One-command publish for production deployment.
- A clear operational checklist for service mode.

## 2. Preconditions

- Windows 10/11 x64.
- Git installed.
- .NET SDK 8.x installed.
- Access to LED controller network.
- Administrative rights (for scheduled task installation).

## 3. Clone and first build (Windows)

1. Open PowerShell.
2. Clone repository.
3. Enter project directory.
4. Restore and build.

Commands:

- git clone https://github.com/MrKadyroff/tablo-poster.git
- cd tablo-poster/TabloPosterService
- dotnet restore
- dotnet build

Expected result: Build succeeded with no errors.

## 4. Validate local environment

Run:

- powershell -ExecutionPolicy Bypass -File migration-pack/windows/verify-windows-env.ps1

This script checks:

- OS is Windows.
- dotnet is installed and major version is 8 or newer.
- Required project files exist.
- Onbon dimensions in appsettings are set to 560x80.

## 5. Local run for API checks

Run:

- dotnet run

Open:

- http://localhost:5050

Quick checks:

- Swagger opens.
- /api/led/connection returns response.
- /api/led/upload works with test image.

## 6. Publish package for production machine

Run:

- powershell -ExecutionPolicy Bypass -File migration-pack/windows/publish-win-package.ps1

Output folder:

- publish-win/

## 7. Deploy on production Windows machine

1. Copy publish-win folder to target, for example C:/TabloPoster.
2. Edit C:/TabloPoster/appsettings.json:
   - OnbonLed.ControllerIp
   - OnbonLed.ScreenWidth = 560
   - OnbonLed.ScreenHeight = 80
   - OnbonLed.AutoSend = true
3. Run as Administrator:
   - C:/TabloPoster/install-service.ps1
4. Optional manual test:
   - C:/TabloPoster/start.bat

## 8. Operational notes

- Auto send uses the same send pipeline as the upload endpoint.
- If image size mismatches screen size and strict check is enabled, send is rejected.
- Dynamic fallback is enabled for Onbon SDK player command error 30 (0x1e).

## 9. Known pitfalls

- If you see duplicate class/namespace errors from BX_Y_CSharp_SDK, ensure build is executed from this project and that the current LedImageUpdaterService.csproj still contains SDK source exclusion for BX_Y_CSharp_SDK/**.
- If native DLL load fails on Windows, verify publish-win contains YQNetCom.dll and related native dependencies.
