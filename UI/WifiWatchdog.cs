using LedImageUpdaterService.Services;

namespace LedImageUpdaterService.UI;

/// <summary>
/// Tray-level owner of the standalone <see cref="WifiAlertForm"/>. It no longer probes
/// Wi-Fi on a blind timer — instead it reacts to the board-send pipeline: after a failed
/// send the service (see <see cref="LedBoardService"/>) tries to reconnect and, if it
/// still cannot reach the board's Wi-Fi, raises <see cref="WifiAlertBridge.ShowAlertRequested"/>.
/// Once a send succeeds the service raises <see cref="WifiAlertBridge.HideAlertRequested"/>.
///
/// Bridge events may arrive on a background thread, so they are marshalled to the UI
/// thread via the captured <see cref="SynchronizationContext"/>. The alert window itself
/// re-checks the connection and auto-closes once the PC rejoins the correct network.
/// </summary>
internal sealed class WifiWatchdog : IDisposable
{
    private WifiAlertForm? _alert;
    private SynchronizationContext? _ui;
    private string? _lastLogLine;
    private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "logs", "wifi-watchdog.log");

    public void Start()
    {
        // Captured on the UI thread (WifiWatchdog is constructed/started from the tray ctor).
        _ui = SynchronizationContext.Current ?? new SynchronizationContext();
        WifiAlertBridge.ShowAlertRequested += OnShowRequested;
        WifiAlertBridge.HideAlertRequested += OnHideRequested;
        Log("WifiWatchdog запущен (проверка Wi-Fi выполняется после неудачной отправки на табло).");
    }

    public void Stop()
    {
        WifiAlertBridge.ShowAlertRequested -= OnShowRequested;
        WifiAlertBridge.HideAlertRequested -= OnHideRequested;
    }

    private void OnShowRequested(string expectedSsid, string? currentSsid)
    {
        Log($"Запрошено предупреждение: ожидается SSID='{expectedSsid}', сейчас='{currentSsid ?? "(нет сети)"}'.");
        Post(() => ShowAlert(expectedSsid, currentSsid));
    }

    private void OnHideRequested()
    {
        Post(() =>
        {
            if (_alert is { IsDisposed: false })
            {
                Log("Связь с табло восстановлена — закрываю предупреждение.");
                _alert.ForceClose();
            }
        });
    }

    private void Post(Action action)
    {
        var ui = _ui;
        if (ui != null) ui.Post(_ => action(), null);
        else action();
    }

    // Logs to logs/wifi-watchdog.log, skipping identical consecutive lines so a stable
    // state does not grow the file. Never throws.
    private void Log(string message)
    {
        try
        {
            if (message == _lastLogLine) return;
            _lastLogLine = message;
            var dir = Path.GetDirectoryName(LogPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
        }
        catch { /* logging must never break the watchdog */ }
    }

    private void ShowAlert(string expectedSsid, string? currentSsid)
    {
        if (_alert is { IsDisposed: false }) return;

        _alert = new WifiAlertForm(expectedSsid, currentSsid);
        _alert.FormClosed += (_, _) => _alert = null;
        _alert.Show();
        _alert.Activate();
    }

    public void Dispose()
    {
        Stop();
        if (_alert is { IsDisposed: false }) { _alert.ForceClose(); _alert.Dispose(); _alert = null; }
    }
}
