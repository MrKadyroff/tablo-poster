namespace LedImageUpdaterService.Models;

/// <summary>
/// Result of one "board link" evaluation produced by <c>BoardLinkMonitor</c>: whether the
/// PC is currently on the LED controller's Wi-Fi (an "island" AP with no internet, the
/// on-site/AnyDesk scenario), which controller IP was found, the connectivity probe
/// results, and the concrete recommendation/action taken.
///
/// This is the structured payload behind <c>GET /api/led/board-link</c> and the settings
/// UI's "Найти контроллер" button. It is intentionally a plain snapshot (no behaviour) so
/// it can be serialized as-is and stored as the "latest" report in <see cref="BoardLinkState"/>.
/// </summary>
public sealed record BoardLinkReport(
    DateTimeOffset CheckedAt,
    bool OnBoardWifi,
    string? InterfaceName,
    string? LocalIp,
    string? Gateway,
    bool HasInternet,
    string? CandidateControllerIp,
    bool PingOk,
    bool TcpOk,
    string? ConfiguredControllerIp,
    bool ControllerIpMatches,
    string? AppliedControllerIp,
    string Verdict,
    string Recommendation,
    IReadOnlyList<string> Steps)
{
    public static BoardLinkReport NotOnBoardWifi(IReadOnlyList<string> steps) => new(
        CheckedAt: DateTimeOffset.UtcNow,
        OnBoardWifi: false,
        InterfaceName: null, LocalIp: null, Gateway: null, HasInternet: true,
        CandidateControllerIp: null,
        PingOk: false, TcpOk: false,
        ConfiguredControllerIp: null, ControllerIpMatches: false, AppliedControllerIp: null,
        Verdict: "idle",
        Recommendation: "ПК не подключён к Wi-Fi табло — мониторинг ждёт переключения сети.",
        Steps: steps);
}

/// <summary>
/// Process-wide, thread-safe holder for the latest <see cref="BoardLinkReport"/> and the
/// runtime IP override the monitor applies on the fly.
///
/// The running <c>OnbonLedController</c> reads <see cref="OverrideControllerIp"/> so an
/// auto-applied controller IP takes effect immediately, without waiting for an
/// <c>IOptions</c> reload or a service restart (the point config file is patched
/// separately so the change also survives a restart).
/// </summary>
public sealed class BoardLinkState
{
    private readonly object _lock = new();
    private BoardLinkReport? _report;

    /// <summary>Controller IP applied at runtime by the monitor (null = use configured value).</summary>
    public string? OverrideControllerIp { get; private set; }

    public BoardLinkReport? Latest
    {
        get { lock (_lock) return _report; }
    }

    public void SetReport(BoardLinkReport report)
    {
        lock (_lock) _report = report;
    }

    public void SetControllerIpOverride(string? ip)
    {
        lock (_lock) OverrideControllerIp = string.IsNullOrWhiteSpace(ip) ? null : ip;
    }
}
