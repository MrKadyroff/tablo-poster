using System.ComponentModel.DataAnnotations;

namespace LedImageUpdaterService.Models;

/// <summary>
/// Configuration for the Onbon BX-Y series LED controller SDK.
/// Mapped from the "OnbonLed" section of appsettings.json.
/// </summary>
public sealed class OnbonOptions
{
    public const string SectionName = "OnbonLed";

    /// <summary>Controller IP address (e.g. "192.168.22.2").</summary>
    [Required]
    public required string ControllerIp { get; init; }

    /// <summary>HTTP/SDK port. Default for BX-Y series is 80.</summary>
    [Range(1, 65535)]
    public int ControllerPort { get; init; } = 80;

    /// <summary>SDK login name. Default is "guest".</summary>
    public string UserName { get; init; } = "guest";

    /// <summary>SDK login password. Default is "guest".</summary>
    public string Password { get; init; } = "guest";

    /// <summary>Screen pixel width.</summary>
    [Range(8, 4096)]
    public int ScreenWidth { get; init; } = 128;

    /// <summary>Screen pixel height.</summary>
    [Range(8, 4096)]
    public int ScreenHeight { get; init; } = 32;

    /// <summary>
    /// Controller model type code used by the SDK.
    /// BX-Y04=8280  BX-Y08=8536  BX-Y2=8792  BX-Y2L=9304
    /// BX-Y3=9048   BX-Y5E=10584 BX-Y1=9560  BX-Y1L=10072
    /// BX-C08=33026
    /// </summary>
    [Range(1000, 65535)]
    public int DeviceType { get; init; } = 8792; // BX-Y2

    /// <summary>
    /// Local temp directory the SDK uses to build program packages before sending.
    /// Must be writable. Relative paths are resolved from the application root.
    /// </summary>
    public string TempPath { get; init; } = "onbon-temp";

    /// <summary>Number of additional retries after the first failure (0 = no retries).</summary>
    [Range(0, 10)]
    public int RetryCount { get; init; } = 2;

    /// <summary>Delay in milliseconds between retries.</summary>
    [Range(500, 30000)]
    public int RetryDelayMs { get; init; } = 2000;

    /// <summary>
    /// Set to false to disable all SDK calls (useful for macOS development).
    /// When false, operations log a warning and return false without touching the DLL.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>TCP connection check timeout in milliseconds.</summary>
    [Range(100, 30000)]
    public int ConnectionTimeoutMs { get; init; } = 3000;

    /// <summary>
    /// Polling interval for LedBoardService background send loop.
    /// </summary>
    [Range(5, 3600)]
    public int PollSeconds { get; init; } = 10;

    /// <summary>
    /// When false, LedBoardService will NOT automatically send images from the
    /// watch folder. All manual API endpoints (Swagger) remain fully operational.
    /// Useful when you want rates images to be generated automatically but delivery
    /// to the board should be triggered manually via the API.
    /// </summary>
    public bool AutoSend { get; init; } = true;

    /// <summary>
    /// When true, image send operations are executed in an isolated helper process.
    /// This protects the main Web API process from native SDK crashes.
    /// </summary>
    public bool UseIsolatedSender { get; init; } = true;

    /// <summary>
    /// When true, identical images (same normalized BMP hash) are skipped.
    /// This reduces repeated uploads and controller load.
    /// </summary>
    public bool SkipDuplicateUploads { get; init; } = true;

    /// <summary>
    /// When true, image size must exactly match ScreenWidth/ScreenHeight before publish.
    /// If false, mismatched images are logged and still processed.
    /// </summary>
    public bool RejectSizeMismatchBeforePublish { get; init; } = true;

    // ─── Board-link monitor (auto-detect the controller on the board's Wi-Fi) ─

    /// <summary>
    /// When true (default), <c>BoardLinkMonitor</c> watches for the PC joining the LED
    /// controller's own Wi-Fi (an "island" AP with no internet — the on-site/AnyDesk
    /// scenario) and runs connectivity diagnostics against the AP's gateway (in AP mode
    /// the controller IS the gateway).
    /// </summary>
    public bool AutoDetectOnApLink { get; init; } = true;

    /// <summary>
    /// When true (default), if the monitor finds the controller on the board Wi-Fi at an
    /// IP that differs from <see cref="ControllerIp"/>, it applies the discovered IP at
    /// runtime and patches the active point's config file. Only fires on an island AP
    /// (private IP, no internet) so a deliberate static LAN IP is never clobbered.
    /// </summary>
    public bool AutoApplyControllerIp { get; init; } = true;

    /// <summary>Re-evaluation interval (seconds) for the board-link monitor's safety poll.</summary>
    [Range(5, 600)]
    public int BoardLinkPollSeconds { get; init; } = 30;
}
