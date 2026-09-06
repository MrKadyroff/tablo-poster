using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using LedImageUpdaterService.Models;
using Microsoft.Extensions.Options;

namespace LedImageUpdaterService.Services;

/// <summary>
/// Background "watchdog" that handles the AnyDesk / on-site scenario where an operator
/// switches the PC's Wi-Fi onto the LED controller's own access point (an "island"
/// network with no internet). When that happens this monitor:
///
///   1. Detects the network switch (NetworkChange events + a safety poll).
///   2. Recognises the board AP: a Wi-Fi interface on a private subnet with no internet.
///   3. Resolves the candidate controller IP as the interface's gateway (in AP mode the
///      controller IS the gateway, e.g. 192.168.43.1) — Onbon has no UDP discovery beacon,
///      unlike the LAN-scan <see cref="ControllerDiscovery"/> uses for the FTP path.
///   4. Runs connectivity diagnostics: ICMP ping + a TCP connect on the configured SDK port.
///   5. Compares the found IP with the configured <c>OnbonLed:ControllerIp</c> and, when
///      they differ, applies the discovered IP at runtime (<see cref="BoardLinkState"/>)
///      and patches the active point's config file — gated by
///      <see cref="OnbonOptions.AutoApplyControllerIp"/> and only on an island AP so a
///      deliberate static LAN IP is never overwritten.
///
/// The structured verdict is published to <see cref="BoardLinkState"/> (for the API/UI)
/// and the key lines are written to <see cref="InMemoryLogStore"/> with the
/// <c>[BoardLink]</c> tag — but only when the state changes, to avoid log spam.
/// </summary>
public sealed class BoardLinkMonitor : BackgroundService
{
    private const string Tag = "BoardLink";

    private readonly ILogger<BoardLinkMonitor> _logger;
    private readonly OnbonOptions _options;
    private readonly BoardLinkState _state;
    private readonly InMemoryLogStore _logStore;
    private readonly IConfiguration _configuration;

    private readonly SemaphoreSlim _wake = new(0, 1);
    private string? _lastSignature;

    public BoardLinkMonitor(
        ILogger<BoardLinkMonitor> logger,
        IOptions<OnbonOptions> options,
        BoardLinkState state,
        InMemoryLogStore logStore,
        IConfiguration configuration)
    {
        _logger = logger;
        _options = options.Value;
        _state = state;
        _logStore = logStore;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled || !_options.AutoDetectOnApLink)
        {
            Log(LogLevel.Information,
                "[BoardLink] Отключён (OnbonLed:Enabled=false или AutoDetectOnApLink=false). Мониторинг не запущен.");
            return;
        }

        NetworkChange.NetworkAddressChanged += OnNetworkChanged;
        NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;

        Log(LogLevel.Information,
            $"[BoardLink] Запущен. Слежу за переключением Wi-Fi на сеть табло (safety-poll каждые {_options.BoardLinkPollSeconds}с).");

        try
        {
            // Small initial settle so the network stack is up before the first probe.
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await EvaluateAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Log(LogLevel.Error, $"[BoardLink] Ошибка цикла: {ex.Message}");
                }

                // Wake on a network change (debounced) or after the safety-poll interval.
                var pollDelay = Task.Delay(TimeSpan.FromSeconds(_options.BoardLinkPollSeconds), stoppingToken);
                var woken = _wake.WaitAsync(stoppingToken);
                var done = await Task.WhenAny(pollDelay, woken);
                if (done == woken)
                {
                    // Debounce: a switch often fires several events in a burst.
                    try { await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken); } catch { }
                }
            }
        }
        finally
        {
            NetworkChange.NetworkAddressChanged -= OnNetworkChanged;
            NetworkChange.NetworkAvailabilityChanged -= OnNetworkAvailabilityChanged;
            Log(LogLevel.Information, "[BoardLink] Остановлен.");
        }
    }

    private void OnNetworkChanged(object? sender, EventArgs e) => Nudge();
    private void OnNetworkAvailabilityChanged(object? sender, NetworkAvailabilityEventArgs e) => Nudge();

    private void Nudge()
    {
        // Release the wait if it isn't already signalled; ignore if it is.
        try { if (_wake.CurrentCount == 0) _wake.Release(); } catch { }
    }

    /// <summary>Force a re-evaluation now (used by the API recheck endpoint).</summary>
    public async Task<BoardLinkReport> RecheckAsync(CancellationToken ct)
        => await EvaluateAsync(ct);

    // ─── Core evaluation ─────────────────────────────────────────────────────

    private async Task<BoardLinkReport> EvaluateAsync(CancellationToken ct)
    {
        var steps = new List<string>();

        var ap = FindBoardApInterface(steps);
        if (ap is null)
        {
            var idle = BoardLinkReport.NotOnBoardWifi(steps);
            Publish(idle);
            return idle;
        }

        steps.Add($"Сеть-«остров» табло: интерфейс '{ap.Name}', локальный IP {ap.LocalIp}, " +
                  $"шлюз {ap.Gateway?.ToString() ?? "—"}, интернет={(ap.HasInternet ? "есть" : "нет")}.");

        // Onbon has no UDP discovery beacon, so the only candidate in AP mode is the
        // interface's own gateway — the controller acts as the access point's router.
        string? candidate = ap.Gateway?.ToString();
        if (candidate is not null)
            steps.Add($"Кандидат — шлюз интерфейса {candidate} (в режиме AP контроллер = шлюз).");
        else
            steps.Add("✗ Шлюз интерфейса неизвестен — определить IP контроллера невозможно.");

        // Connectivity diagnostics on the candidate.
        bool pingOk = false, tcpOk = false;
        if (candidate is not null)
        {
            pingOk = TryPing(candidate, 1000);
            steps.Add($"ICMP ping {candidate}: {(pingOk ? "ответ есть" : "нет ответа")}.");

            int probeTimeout = Math.Min(_options.ConnectionTimeoutMs, 4000);
            tcpOk = await TryTcpAsync(candidate, _options.ControllerPort, probeTimeout, ct);
            steps.Add($"TCP {candidate}:{_options.ControllerPort}: {(tcpOk ? "подключение OK" : "не подключается")}.");
        }

        // Compare with config and decide.
        var configuredIp = string.IsNullOrWhiteSpace(_options.ControllerIp) ? null : _options.ControllerIp.Trim();
        bool ipMatches = candidate is not null && string.Equals(candidate, configuredIp, StringComparison.OrdinalIgnoreCase);
        string? appliedIp = null;

        bool singleControllerOnIsland = !ap.HasInternet;
        bool reachable = tcpOk;

        if (candidate is not null && reachable && !ipMatches && singleControllerOnIsland && IsPrivate(candidate))
        {
            if (_options.AutoApplyControllerIp)
            {
                _state.SetControllerIpOverride(candidate);
                bool persisted = TryPatchPointConfig(candidate, steps);
                appliedIp = candidate;
                steps.Add($"✓ Авто-применён ControllerIp={candidate} (рантайм{(persisted ? " + конфиг точки" : "")}).");
            }
            else
            {
                steps.Add($"→ Рекомендуется поставить OnbonLed:ControllerIp={candidate} (AutoApplyControllerIp=false).");
            }
        }

        var (verdict, recommendation) = BuildVerdict(candidate, reachable, ipMatches, appliedIp, _options.ControllerPort);

        var report = new BoardLinkReport(
            CheckedAt: DateTimeOffset.UtcNow,
            OnBoardWifi: true,
            InterfaceName: ap.Name,
            LocalIp: ap.LocalIp.ToString(),
            Gateway: ap.Gateway?.ToString(),
            HasInternet: ap.HasInternet,
            CandidateControllerIp: candidate,
            PingOk: pingOk,
            TcpOk: tcpOk,
            ConfiguredControllerIp: configuredIp,
            ControllerIpMatches: ipMatches,
            AppliedControllerIp: appliedIp,
            Verdict: verdict,
            Recommendation: recommendation,
            Steps: steps);

        Publish(report);
        return report;
    }

    private static (string verdict, string recommendation) BuildVerdict(
        string? candidate, bool reachable, bool ipMatches, string? appliedIp, int port)
    {
        if (candidate is null)
            return ("controller-not-found",
                "Контроллер не найден на Wi-Fi табло. Проверьте, что ПК подключён именно к точке доступа табло.");

        if (!reachable)
            return ("unreachable",
                $"Контроллер {candidate} найден (это шлюз сети), но не отвечает по TCP {candidate}:{port}. " +
                "Проверьте firewall и что это действительно адрес контроллера.");

        if (appliedIp is not null)
            return ("fixed",
                $"Готово: связь с контроллером есть, ControllerIp автоматически установлен в {appliedIp}. Можно отправлять на табло.");

        if (ipMatches)
            return ("ok", $"Всё в порядке: контроллер {candidate} доступен и совпадает с настройками. Можно отправлять.");

        return ("needs-config",
            $"Контроллер доступен на {candidate}, но в настройках другой ControllerIp. " +
            $"Установите OnbonLed:ControllerIp={candidate} (или включите AutoApplyControllerIp).");
    }

    // ─── Publish (dedup + log) ───────────────────────────────────────────────

    private void Publish(BoardLinkReport report)
    {
        _state.SetReport(report);

        // Signature ignores the timestamp so we only log on a meaningful change.
        var sig = $"{report.OnBoardWifi}|{report.Verdict}|{report.CandidateControllerIp}|{report.AppliedControllerIp}|{report.ControllerIpMatches}";
        if (sig == _lastSignature) return;
        _lastSignature = sig;

        var level = report.Verdict switch
        {
            "ok" or "fixed" or "idle" => LogLevel.Information,
            "needs-config" => LogLevel.Warning,
            _ => LogLevel.Error,
        };
        Log(level, $"[BoardLink] {report.Recommendation}");
    }

    // ─── Network detection ───────────────────────────────────────────────────

    private sealed record ApInterface(string Name, IPAddress LocalIp, IPAddress? Gateway, bool HasInternet);

    /// <summary>
    /// Returns the active Wi-Fi interface that looks like the board's AP (private IPv4,
    /// preferring one with no internet), or null if the PC is on a normal network.
    /// </summary>
    private ApInterface? FindBoardApInterface(List<string> steps)
    {
        ApInterface? best = null;
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up) continue;
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
            if (IsVirtualAdapter(ni)) continue;            // skip AnyDesk/VPN/etc.
            if (!IsWifiInterface(ni)) continue;             // board AP is Wi-Fi

            var props = ni.GetIPProperties();
            var ipv4 = props.UnicastAddresses
                .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork);
            if (ipv4?.Address is null || !IsPrivate(ipv4.Address)) continue;

            var gw = props.GatewayAddresses
                .Select(g => g.Address)
                .FirstOrDefault(a => a is not null && a.AddressFamily == AddressFamily.InterNetwork
                                     && !a.Equals(IPAddress.Any));

            bool internet = HasInternet();

            // A board AP is a Wi-Fi island with NO internet. A Wi-Fi that has internet is
            // almost certainly the normal office/home network — don't treat it as the board.
            if (!internet)
                return new ApInterface(ni.Name, ipv4.Address, gw, false);

            best ??= new ApInterface(ni.Name, ipv4.Address, gw, true);
        }

        if (best is not null)
            steps.Add($"Wi-Fi '{best.Name}' с интернетом — это обычная сеть, не AP табло. Жду переключения.");
        return null;
    }

    private static bool IsVirtualAdapter(NetworkInterface ni)
    {
        var f = ($"{ni.Name} {ni.Description}").ToLowerInvariant();
        return f.Contains("anydesk") || f.Contains("teamviewer") || f.Contains("virtual")
               || f.Contains("vpn") || f.Contains("vethernet") || f.Contains("hyper-v")
               || f.Contains("loopback") || f.Contains("tap-") || f.Contains("wintun")
               || f.Contains("wireguard") || f.Contains("vmware") || f.Contains("vbox");
    }

    private static bool IsWifiInterface(NetworkInterface ni)
    {
        if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) return true;
        var f = ($"{ni.Name} {ni.Description}").ToLowerInvariant();
        return f.Contains("wifi") || f.Contains("wi-fi") || f.Contains("wlan") || f.Contains("802.11");
    }

    /// <summary>Quick internet probe: a 700 ms TCP connect to a public DNS resolver.</summary>
    private static bool HasInternet()
    {
        foreach (var host in new[] { "8.8.8.8", "1.1.1.1" })
        {
            try
            {
                using var s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var ar = s.BeginConnect(IPAddress.Parse(host), 53, null, null);
                if (ar.AsyncWaitHandle.WaitOne(700) && s.Connected) { s.EndConnect(ar); return true; }
            }
            catch { /* try next */ }
        }
        return false;
    }

    private static bool IsPrivate(string ip)
        => IPAddress.TryParse(ip, out var a) && IsPrivate(a);

    private static bool IsPrivate(IPAddress ip)
    {
        if (ip.AddressFamily != AddressFamily.InterNetwork) return false;
        var b = ip.GetAddressBytes();
        return b[0] == 10
               || (b[0] == 172 && b[1] >= 16 && b[1] <= 31)
               || (b[0] == 192 && b[1] == 168);
    }

    // ─── Probes ──────────────────────────────────────────────────────────────

    private static bool TryPing(string ip, int timeoutMs)
    {
        try
        {
            using var ping = new Ping();
            return ping.Send(ip, timeoutMs).Status == IPStatus.Success;
        }
        catch { return false; }
    }

    private static async Task<bool> TryTcpAsync(string ip, int port, int timeoutMs, CancellationToken ct)
    {
        try
        {
            using var tcp = new TcpClient();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeoutMs);
            await tcp.ConnectAsync(ip, port, cts.Token);
            return true;
        }
        catch { return false; }
    }

    // ─── Persistence ─────────────────────────────────────────────────────────

    private static readonly JsonSerializerOptions _writeOpts = new()
    {
        WriteIndented = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    };
    private static readonly JsonDocumentOptions _readOpts = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    /// <summary>
    /// Patches OnbonLed:ControllerIp in the ACTIVE POINT's config file
    /// (config/points/{ActivePointId}.json) — that is where this app's settings UI
    /// (<c>AppSettingsManager.SavePointConfig</c>) persists the controller IP per point,
    /// not the root appsettings.json.
    /// </summary>
    private bool TryPatchPointConfig(string controllerIp, List<string> steps)
    {
        try
        {
            var pointId = _configuration["ActivePointId"];
            if (string.IsNullOrWhiteSpace(pointId))
            {
                steps.Add("⚠ ActivePointId не задан — записал только в рантайм.");
                return false;
            }

            var path = Path.Combine(AppContext.BaseDirectory, "config", "points", $"{pointId}.json");
            if (!File.Exists(path)) { steps.Add($"⚠ Конфиг точки {path} не найден — записал только в рантайм."); return false; }

            var root = JsonNode.Parse(File.ReadAllText(path), null, _readOpts)!.AsObject();
            var ob = root["OnbonLed"]?.AsObject() ?? new JsonObject();
            ob["ControllerIp"] = controllerIp;
            root["OnbonLed"] = ob;
            File.WriteAllText(path, root.ToJsonString(_writeOpts));
            return true;
        }
        catch (Exception ex)
        {
            steps.Add($"⚠ Не удалось записать конфиг точки: {ex.Message} (рантайм-значение применено).");
            return false;
        }
    }

    // ─── Logging ─────────────────────────────────────────────────────────────

    private void Log(LogLevel level, string message)
    {
        _logger.Log(level, "{Message}", message);
        _logStore.Add(level, Tag, message);
    }

    public override void Dispose()
    {
        _wake.Dispose();
        base.Dispose();
    }
}
