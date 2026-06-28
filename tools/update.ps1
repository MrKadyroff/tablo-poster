<#
.SYNOPSIS
  Обновляет eCash Tablo, СОХРАНЯЯ локальные настройки точки.

.DESCRIPTION
  Решает проблему "после обновления всё настраивать заново". Берёт новую сборку из
  центрального источника (сетевая папка, облачная папка, USB — любой путь к папке, где
  лежит новый eCashTablo.exe), и копирует её поверх установленной версии, НО сохраняет
  файлы с настройками этой точки:
      appsettings.json, config\, layout\points\, content\points\

  Версия определяется по eCashTablo.exe. Если версия в источнике не новее установленной,
  обновление пропускается (если не указан -Force).

.PARAMETER Source
  Путь к папке с новой сборкой (где лежит eCashTablo.exe). Если не указан — берётся из
  файла tools\update-source.txt, иначе спрашивается у пользователя.

.PARAMETER Force
  Обновить даже если версия не новее.

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File update.ps1 -Source "\\server\ecash\latest"
#>

param(
    [string]$Source,
    [switch]$Force
)

$ErrorActionPreference = 'Stop'

# Install dir = родитель папки tools\, где лежит этот скрипт.
$Install   = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$LogFile   = Join-Path $PSScriptRoot 'update.log'
$SourceTxt = Join-Path $PSScriptRoot 'update-source.txt'
$ExeName   = 'eCashTablo.exe'

# Файлы/папки с настройками точки — НЕ перезаписываются при обновлении.
$Preserve = @(
    'appsettings.json',
    'config',
    'layout\points',
    'content\points',
    'tools\update-source.txt'
)

function Log($msg) {
    $line = "[{0}] {1}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $msg
    Write-Host $line
    try { Add-Content -Path $LogFile -Value $line -Encoding UTF8 } catch {}
}

function Fail($msg) {
    Log "ОШИБКА: $msg"
    Write-Host ''
    Read-Host 'Нажмите Enter, чтобы закрыть'
    exit 1
}

Log "=== Обновление eCash Tablo ==="
Log "Установка: $Install"

# ── 1. Определить источник ────────────────────────────────────────────────────
if (-not $Source) {
    if (Test-Path $SourceTxt) {
        $Source = (Get-Content $SourceTxt -Raw).Trim()
        Log "Источник из update-source.txt: $Source"
    }
}
if (-not $Source) {
    $Source = (Read-Host 'Укажите путь к папке с новой версией (где лежит eCashTablo.exe)').Trim('"').Trim()
}
if (-not $Source) { Fail 'Источник не указан.' }

$srcExe = Join-Path $Source $ExeName
$dstExe = Join-Path $Install $ExeName
if (-not (Test-Path $srcExe)) { Fail "В источнике нет $ExeName : $srcExe" }
if ((Resolve-Path $Source).Path -ieq $Install) { Fail 'Источник совпадает с установкой.' }

# ── 2. Сравнить версии ─────────────────────────────────────────────────────────
function ExeVersion($path) {
    try { return [version](Get-Item $path).VersionInfo.FileVersion } catch { return $null }
}
$srcVer = ExeVersion $srcExe
$dstVer = if (Test-Path $dstExe) { ExeVersion $dstExe } else { $null }
Log "Версия источника: $srcVer; установленная: $dstVer"

if (-not $Force -and $dstVer -and $srcVer -and $srcVer -le $dstVer) {
    Log "Установлена та же или более новая версия — обновление не требуется."
    Write-Host ''
    Read-Host 'Нажмите Enter, чтобы закрыть'
    exit 0
}

# ── 3. Остановить приложение ────────────────────────────────────────────────────
Log "Останавливаю приложение…"
Get-Process -Name 'eCashTablo' -ErrorAction SilentlyContinue | ForEach-Object {
    try { $_.Kill() } catch {}
}
Start-Sleep -Seconds 2

# ── 4. Резервная копия настроек ─────────────────────────────────────────────────
$Backup = Join-Path $env:TEMP ("ecash-cfg-backup-{0}" -f (Get-Date -Format 'yyyyMMddHHmmss'))
New-Item -ItemType Directory -Path $Backup -Force | Out-Null
Log "Сохраняю настройки в $Backup"
foreach ($rel in $Preserve) {
    $src = Join-Path $Install $rel
    if (-not (Test-Path $src)) { continue }
    $dst = Join-Path $Backup $rel
    $dstDir = Split-Path $dst -Parent
    New-Item -ItemType Directory -Path $dstDir -Force | Out-Null
    if ((Get-Item $src).PSIsContainer) {
        robocopy $src $dst /E /NFL /NDL /NJH /NJS /NP | Out-Null
    } else {
        Copy-Item $src $dst -Force
    }
}

# ── 5. Скопировать новую сборку поверх ──────────────────────────────────────────
Log "Копирую новую версию из $Source …"
# /E — со вложенными папками; не /MIR, чтобы не удалить logs\, onbon-temp\ и т.п.
robocopy $Source $Install /E /R:2 /W:2 /NFL /NDL /NJH /NJS /NP | Out-Null
if ($LASTEXITCODE -ge 8) { Fail "robocopy вернул код $LASTEXITCODE при копировании сборки." }

# ── 6. Восстановить настройки ────────────────────────────────────────────────────
Log "Восстанавливаю настройки точки…"
foreach ($rel in $Preserve) {
    $src = Join-Path $Backup $rel
    if (-not (Test-Path $src)) { continue }
    $dst = Join-Path $Install $rel
    if ((Get-Item $src).PSIsContainer) {
        robocopy $src $dst /E /NFL /NDL /NJH /NJS /NP | Out-Null
    } else {
        Copy-Item $src $dst -Force
    }
}
try { Remove-Item $Backup -Recurse -Force } catch {}

# ── 7. Запустить приложение ──────────────────────────────────────────────────────
Log "Запускаю eCash Tablo…"
Start-Process -FilePath $dstExe -WorkingDirectory $Install

Log "Готово. Установлена версия $srcVer."
Write-Host ''
Read-Host 'Обновление завершено. Нажмите Enter, чтобы закрыть'
