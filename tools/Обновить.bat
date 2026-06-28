@echo off
chcp 65001 >nul
rem Двойной клик по этому файлу обновляет eCash Tablo, сохраняя настройки точки.
rem Путь к новой версии берётся из update-source.txt (или спросит вручную).
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0update.ps1"
