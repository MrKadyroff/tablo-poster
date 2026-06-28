namespace LedImageUpdaterService.Services;

/// <summary>
/// Process-wide bridge between the board-send pipeline (DI host, see
/// <see cref="LedBoardService"/>) and the tray UI Wi-Fi alert (see
/// <c>UI.WifiWatchdog</c> / <c>UI.WifiAlertForm</c>).
///
/// The send pipeline detects "board not reachable / wrong Wi-Fi" only when a send
/// actually fails, but the loud standalone alert window lives in the WinForms tray
/// layer. Since both run in the same process, this static bridge lets the service
/// ask the UI to show/hide the alert without taking a hard dependency on WinForms.
///
/// Handlers may be invoked from background threads — subscribers are responsible for
/// marshalling to the UI thread.
/// </summary>
public static class WifiAlertBridge
{
    /// <summary>Raised after a failed board send when the PC is on the wrong Wi-Fi.
    /// Arguments: (expectedSsid, currentSsid).</summary>
    public static event Action<string, string?>? ShowAlertRequested;

    /// <summary>Raised once the board is reachable again (delivery succeeded).</summary>
    public static event Action? HideAlertRequested;

    public static void RequestShow(string expectedSsid, string? currentSsid)
    {
        try { ShowAlertRequested?.Invoke(expectedSsid, currentSsid); }
        catch { /* UI signalling must never break the send pipeline */ }
    }

    public static void RequestHide()
    {
        try { HideAlertRequested?.Invoke(); }
        catch { /* UI signalling must never break the send pipeline */ }
    }
}
