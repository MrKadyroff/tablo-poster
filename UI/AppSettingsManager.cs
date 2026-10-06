using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using System.Xml.Linq;

namespace LedImageUpdaterService.UI;

internal sealed class AppConfig
{
    /// <summary>The point being viewed/edited right now (board 1 or board 2).</summary>
    public string ActivePointId { get; set; } = "aport2";
    /// <summary>Point of board 1 (the main one). Equals <see cref="ActivePointId"/> while board 1 is edited.</summary>
    public string PrimaryPointId { get; set; } = "aport2";
    /// <summary>Point of board 2, or empty when only one board is used.</summary>
    public string SecondPointId { get; set; } = "";
    /// <summary>Which board <see cref="ActivePointId"/> refers to: 0 = board 1, 1 = board 2. Not saved.</summary>
    public int BoardIndex { get; set; }
    /// <summary>API address of board 1.</summary>
    public string Urls { get; set; } = "http://localhost:5050";
    /// <summary>Explicit API address of board 2; empty = board 1's port + 1.</summary>
    public string SecondUrls { get; set; } = "";
    public bool HasSecondBoard => !string.IsNullOrWhiteSpace(SecondPointId);
    /// <summary>API address of the board currently being edited.</summary>
    public string BoardUrls => BoardIndex == 1
        ? Services.BoardTopology.SecondUrls(Urls, SecondUrls)
        : Urls;

    // Password gate for the full settings window. Cashiers use the simplified
    // window (no password); administrators must enter this to open Настройки.
    // Stored in appsettings.json root; change it there to set a new password.
    public string AdminPassword { get; set; } = "admin";

    // LedUpdater
    public string RunMode { get; set; } = "RenderOnly";
    public string PublishMode { get; set; } = "WifiFtp";
    public int PollSeconds { get; set; } = 10;
    public int RatesFetchIntervalMinutes { get; set; } = 1;
    public string FtpUser { get; set; } = "guest";
    public string FtpPassword { get; set; } = "guest";
    public int FtpPort { get; set; } = 21;
    public bool UseTls { get; set; }
    public bool SkipIfUnchanged { get; set; } = true;
    public bool ForceComposeEveryPoll { get; set; }
    public bool LayoutTestMode { get; set; } = false;
    // true = permanent (wired) internet → auto-send to board on timer (current behavior).
    // false = no stable internet → render only, push to board manually from the Design tab.
    public bool PermanentInternet { get; set; } = true;
    public bool EnforceWifiOnly { get; set; }
    public bool RequirePrivateAddress { get; set; } = true;
    public string RatesApiUrl { get; set; } = "https://api.quiq.kz/Department/getDepsLandingInfo";
    public string ControllerReloadUrl { get; set; } = "";
    public string ForceRemoteRoot { get; set; } = "";

    // Expected Wi-Fi network name (SSID) of the LED board's access point. Used only by the
    // cashier window to detect when the PC is connected to the wrong/no Wi-Fi and show a
    // prominent alert. Empty = no SSID check. Can be set globally (appsettings LedUpdater)
    // or per point (config/points/{id}.json → LedUpdater.WifiSsid).
    public string WifiSsid { get; set; } = "";

    // This app drives the Onbon family only (Huidu lives in the separate eCash Tablo Huidu app).
    public string ControllerFamily { get; set; } = ControllerFamilyCatalog.Onbon;

    // Telegram notifications (board send failure / recovery)
    public bool TelegramEnabled { get; set; }
    public string TelegramBotToken { get; set; } = "";
    public string TelegramChatId { get; set; } = "";

    // OnbonLed
    public bool OnbonEnabled { get; set; } = true;
    public string ControllerIp { get; set; } = "192.168.22.2";
    public int ControllerPort { get; set; } = 80;
    public int ScreenWidth { get; set; } = 128;
    public int ScreenHeight { get; set; } = 256;
    public int DeviceType { get; set; } = 9304;
    // Connection (communication) method for the LedShow descriptor (screen.xml ip_mode).
    // "Ip" = direct fixed-IP connection (default), "Server" = cloud server mode.
    public string ConnectionMode { get; set; } = ConnectionModeCatalog.DefaultKey;
    public string OnbonUserName { get; set; } = "guest";
    public string OnbonPassword { get; set; } = "guest";
    public string TempPath { get; set; } = "onbon-temp";
    public int RetryCount { get; set; } = 2;
    public int RetryDelayMs { get; set; } = 2000;
    public int ConnectionTimeoutMs { get; set; } = 3000;
    public int OnbonPollSeconds { get; set; } = 10;
    public bool AutoSend { get; set; } = true;
    public bool UseIsolatedSender { get; set; } = true;
    public bool SkipDuplicateUploads { get; set; } = true;
    public bool RejectSizeMismatchBeforePublish { get; set; } = true;

    // Compose config — multi-column model (up to 3 columns)
    public int ColumnCount { get; set; } = 1;
    public List<List<string>> Columns { get; set; } = [["USD", "EUR", "RUB", "CNY", "KGS", "TRY"]];
    // Per-column buy/sell header label lines (typically 3 lines: KZ/RU/EN)
    public List<List<string>> ColumnBuyLabels { get; set; } = [];
    public List<List<string>> ColumnSellLabels { get; set; } = [];
    // Optional per-column absolute X offset (1× px). null = auto (evenly spaced).
    // Lets you place columns freely, e.g. a centred logo with rates on both sides.
    public List<int?> ColumnX { get; set; } = [];
    public int CanvasWidth { get; set; } = 128;
    public int CanvasHeight { get; set; } = 256;

    public static List<string> DefaultBuyLabels => ["Сатып аламыз", "Покупаем", "We buy"];
    public static List<string> DefaultSellLabels => ["Сатамыз", "Продаем", "We sell"];

    /// <summary>Ensures Columns and label lists have exactly ColumnCount entries.</summary>
    public void NormalizeColumns()
    {
        ColumnCount = Math.Clamp(ColumnCount, 1, 3);
        while (Columns.Count < ColumnCount) Columns.Add([]);
        if (Columns.Count > ColumnCount) Columns = Columns.Take(ColumnCount).ToList();

        while (ColumnBuyLabels.Count < ColumnCount) ColumnBuyLabels.Add([.. DefaultBuyLabels]);
        if (ColumnBuyLabels.Count > ColumnCount) ColumnBuyLabels = ColumnBuyLabels.Take(ColumnCount).ToList();

        while (ColumnSellLabels.Count < ColumnCount) ColumnSellLabels.Add([.. DefaultSellLabels]);
        if (ColumnSellLabels.Count > ColumnCount) ColumnSellLabels = ColumnSellLabels.Take(ColumnCount).ToList();

        while (ColumnX.Count < ColumnCount) ColumnX.Add(null);
        if (ColumnX.Count > ColumnCount) ColumnX = ColumnX.Take(ColumnCount).ToList();
    }

    /// <summary>All selected currency codes across every column (distinct, order-preserving).</summary>
    public List<string> AllCurrencies => Columns.SelectMany(c => c).Distinct().ToList();

    // ─── Layout geometry (gridLayout block) ──────────────────────────────────
    // Logo
    public int LogoX { get; set; } = 2;
    public int LogoY { get; set; } = 2;
    public int LogoW { get; set; } = 40;
    public int LogoH { get; set; } = 31;
    // Headers
    public int HeaderBuyX { get; set; } = 67;
    public int HeaderBuyY { get; set; } = 3;
    public int HeaderSellX { get; set; } = 104;
    public int HeaderSellY { get; set; } = 3;
    // Rows
    public int RowsStartY { get; set; } = 36;
    public int RowH { get; set; } = 30;
    public int HeaderH { get; set; } = 32;
    // Columns
    public int ColFlagX { get; set; } = 2;
    public int ColFlagW { get; set; } = 23;
    public int ColFlagH { get; set; } = 24;
    /// <summary>Scrolling band at the top of the board (renders final.gif instead of final.jpg).</summary>
    public bool TickerEnabled { get; set; }
    /// <summary>Ticker scroll speed in board pixels per frame.</summary>
    public double TickerSpeed { get; set; } = 3;
    /// <summary>Ticker band colour, "#RRGGBB" (default: eCash orange).</summary>
    public string TickerBgColor { get; set; } = "#F58220";
    /// <summary>Ticker text colour, "#RRGGBB".</summary>
    public string TickerTextColor { get; set; } = "#FFFFFF";
    /// <summary>Glint sweeping over random flags (renders final.gif instead of final.jpg).</summary>
    public bool ShineEnabled { get; set; }
    /// <summary>How many flags glint per render.</summary>
    public int ShineCount { get; set; } = 3;
    /// <summary>Glint brightness, 0.05..1.</summary>
    public double ShineStrength { get; set; } = 0.75;
    /// <summary>Glint width as a fraction of the flag width, 0.05..1.</summary>
    public double ShineWidth { get; set; } = 0.35;
    /// <summary>Ticker band height in board pixels. 0 = automatic (~14 % of the board).</summary>
    public int TickerH { get; set; }
    /// <summary>Ticker text size in board pixels. 0 = automatic (72 % of the band).</summary>
    public int TickerFontSize { get; set; }
    /// <summary>Custom ticker text, one message per line. Empty = "Обмен валют" in several languages.</summary>
    public string TickerText { get; set; } = "";
    public int ColCodeX { get; set; } = 27;
    public int ColBuyX { get; set; } = 57;
    public int ColBuyW { get; set; } = 38;
    public int ColSellX { get; set; } = 97;
    public int ColSellW { get; set; } = 29;
    // Fonts
    public int FszHdr { get; set; } = 7;
    public int FszCode { get; set; } = 16;
    public int FszValue { get; set; } = 17;
    public int FszArrow { get; set; } = 8;
    public double FontScaleX { get; set; } = 0.91;
    public int TextStroke { get; set; } = 1;
    public int ValueShiftX { get; set; } = -5;
    public int Oversample { get; set; } = 4;
}

internal static class AppSettingsManager
{
    // TypeInfoResolver is required so JsonNode.ToJsonString can serialize values
    // created from CLR types (double/decimal) without throwing on .NET 8.
    private static readonly JsonSerializerOptions _writeOpts = new()
    {
        WriteIndented = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    };
    private static readonly JsonDocumentOptions _readOpts = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static string AppDir => AppContext.BaseDirectory;
    private static string AppSettingsPath => Path.Combine(AppDir, "appsettings.json");

    public static string[] GetAvailablePoints()
    {
        var dir = Path.Combine(AppDir, "config", "points");
        if (!Directory.Exists(dir)) return [];
        return Directory.GetFiles(dir, "*.json")
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .OrderBy(n => n)
            .ToArray();
    }

    public static string[] GetAvailableCurrencies()
    {
        var flagsDir = Path.Combine(AppDir, "content", "common", "flags");
        if (!Directory.Exists(flagsDir)) return KnownCurrencies.Keys.ToArray();
        return Directory.GetFiles(flagsDir, "*.png")
            .Select(f => Path.GetFileNameWithoutExtension(f).ToUpperInvariant())
            .Where(c => !c.StartsWith("GOLD", StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c)
            .ToArray();
    }

    /// <param name="boardIndex">0 = board 1 (default), 1 = board 2 (falls back to board 1 when none is set).</param>
    /// <param name="primaryOverride">Board 1's point, when it differs from the saved one (unsaved edit).</param>
    /// <param name="secondOverride">Board 2's point ("" = none), when it differs from the saved one.</param>
    public static AppConfig Load(int boardIndex = 0, string? primaryOverride = null, string? secondOverride = null)
    {
        var cfg = new AppConfig();
        try
        {
            var text = File.ReadAllText(AppSettingsPath);
            var root = JsonNode.Parse(text, null, _readOpts)!.AsObject();

            cfg.ActivePointId = root["ActivePointId"]?.GetValue<string>() ?? cfg.ActivePointId;
            cfg.PrimaryPointId = cfg.ActivePointId;
            cfg.SecondPointId = root["SecondPointId"]?.GetValue<string>() ?? "";
            cfg.Urls = root["Urls"]?.GetValue<string>() ?? cfg.Urls;
            cfg.SecondUrls = root["SecondUrls"]?.GetValue<string>() ?? "";
            if (primaryOverride is not null) cfg.PrimaryPointId = primaryOverride;
            if (secondOverride is not null) cfg.SecondPointId = secondOverride;
            cfg.ActivePointId = cfg.PrimaryPointId;
            if (boardIndex == 1 && cfg.HasSecondBoard)
            {
                cfg.BoardIndex = 1;
                cfg.ActivePointId = cfg.SecondPointId;
            }
            cfg.AdminPassword = root["AdminPassword"]?.GetValue<string>() ?? cfg.AdminPassword;

            cfg.ControllerFamily = root["Led"]?["Family"]?.GetValue<string>() ?? cfg.ControllerFamily;

            var tg = root["Telegram"]?.AsObject();
            if (tg != null)
            {
                cfg.TelegramEnabled = tg["Enabled"]?.GetValue<bool>() ?? cfg.TelegramEnabled;
                cfg.TelegramBotToken = tg["BotToken"]?.GetValue<string>() ?? cfg.TelegramBotToken;
                cfg.TelegramChatId = tg["ChatId"]?.GetValue<string>() ?? cfg.TelegramChatId;
            }

            var lu = root["LedUpdater"]?.AsObject();
            if (lu != null)
            {
                cfg.RunMode = lu["RunMode"]?.GetValue<string>() ?? cfg.RunMode;
                cfg.PublishMode = lu["PublishMode"]?.GetValue<string>() ?? cfg.PublishMode;
                cfg.PollSeconds = lu["PollSeconds"]?.GetValue<int>() ?? cfg.PollSeconds;
                cfg.RatesFetchIntervalMinutes = lu["RatesFetchIntervalMinutes"]?.GetValue<int>() ?? cfg.RatesFetchIntervalMinutes;
                cfg.FtpUser = lu["FtpUser"]?.GetValue<string>() ?? cfg.FtpUser;
                cfg.FtpPassword = lu["FtpPassword"]?.GetValue<string>() ?? cfg.FtpPassword;
                cfg.FtpPort = lu["FtpPort"]?.GetValue<int>() ?? cfg.FtpPort;
                cfg.UseTls = lu["UseTls"]?.GetValue<bool>() ?? cfg.UseTls;
                cfg.SkipIfUnchanged = lu["SkipIfUnchanged"]?.GetValue<bool>() ?? cfg.SkipIfUnchanged;
                cfg.ForceComposeEveryPoll = lu["ForceComposeEveryPoll"]?.GetValue<bool>() ?? cfg.ForceComposeEveryPoll;
                cfg.LayoutTestMode = lu["LayoutTestMode"]?.GetValue<bool>() ?? cfg.LayoutTestMode;
                cfg.PermanentInternet = lu["PermanentInternet"]?.GetValue<bool>() ?? cfg.PermanentInternet;
                cfg.EnforceWifiOnly = lu["EnforceWifiOnly"]?.GetValue<bool>() ?? cfg.EnforceWifiOnly;
                cfg.RequirePrivateAddress = lu["RequirePrivateAddress"]?.GetValue<bool>() ?? cfg.RequirePrivateAddress;
                cfg.RatesApiUrl = lu["RatesApiUrl"]?.GetValue<string>() ?? cfg.RatesApiUrl;
                cfg.WifiSsid = lu["WifiSsid"]?.GetValue<string>() ?? cfg.WifiSsid;
            }

            var ob = root["OnbonLed"]?.AsObject();
            if (ob != null)
            {
                cfg.OnbonEnabled = ob["Enabled"]?.GetValue<bool>() ?? cfg.OnbonEnabled;
                cfg.OnbonUserName = ob["UserName"]?.GetValue<string>() ?? cfg.OnbonUserName;
                cfg.OnbonPassword = ob["Password"]?.GetValue<string>() ?? cfg.OnbonPassword;
                cfg.TempPath = ob["TempPath"]?.GetValue<string>() ?? cfg.TempPath;
                cfg.RetryCount = ob["RetryCount"]?.GetValue<int>() ?? cfg.RetryCount;
                cfg.RetryDelayMs = ob["RetryDelayMs"]?.GetValue<int>() ?? cfg.RetryDelayMs;
                cfg.ConnectionTimeoutMs = ob["ConnectionTimeoutMs"]?.GetValue<int>() ?? cfg.ConnectionTimeoutMs;
                cfg.OnbonPollSeconds = ob["PollSeconds"]?.GetValue<int>() ?? cfg.OnbonPollSeconds;
                cfg.AutoSend = ob["AutoSend"]?.GetValue<bool>() ?? cfg.AutoSend;
                cfg.UseIsolatedSender = ob["UseIsolatedSender"]?.GetValue<bool>() ?? cfg.UseIsolatedSender;
                cfg.SkipDuplicateUploads = ob["SkipDuplicateUploads"]?.GetValue<bool>() ?? cfg.SkipDuplicateUploads;
                cfg.RejectSizeMismatchBeforePublish = ob["RejectSizeMismatchBeforePublish"]?.GetValue<bool>() ?? cfg.RejectSizeMismatchBeforePublish;
                cfg.ControllerIp = ob["ControllerIp"]?.GetValue<string>() ?? cfg.ControllerIp;
                cfg.ControllerPort = ob["ControllerPort"]?.GetValue<int>() ?? cfg.ControllerPort;
                cfg.ScreenWidth = ob["ScreenWidth"]?.GetValue<int>() ?? cfg.ScreenWidth;
                cfg.ScreenHeight = ob["ScreenHeight"]?.GetValue<int>() ?? cfg.ScreenHeight;
                cfg.DeviceType = ob["DeviceType"]?.GetValue<int>() ?? cfg.DeviceType;
                cfg.ConnectionMode = ob["ConnectionMode"]?.GetValue<string>() ?? cfg.ConnectionMode;
            }

            // Load point-specific overrides
            LoadPointConfig(cfg, cfg.ActivePointId);

            // Load compose config
            LoadComposeConfig(cfg, cfg.ActivePointId);
        }
        catch { /* return defaults on any error */ }
        return cfg;
    }

    private static void LoadPointConfig(AppConfig cfg, string pointId)
    {
        var path = Path.Combine(AppDir, "config", "points", $"{pointId}.json");
        if (!File.Exists(path)) return;
        try
        {
            var text = File.ReadAllText(path);
            var root = JsonNode.Parse(text, null, _readOpts)!.AsObject();

            var lu = root["LedUpdater"]?.AsObject();
            if (lu != null)
            {
                cfg.ControllerReloadUrl = lu["ControllerReloadUrl"]?.GetValue<string>() ?? cfg.ControllerReloadUrl;
                cfg.ForceRemoteRoot = lu["ForceRemoteRoot"]?.GetValue<string>() ?? cfg.ForceRemoteRoot;
                cfg.WifiSsid = lu["WifiSsid"]?.GetValue<string>() ?? cfg.WifiSsid;
            }

            cfg.ControllerFamily = root["Led"]?["Family"]?.GetValue<string>() ?? cfg.ControllerFamily;

            var ob = root["OnbonLed"]?.AsObject();
            if (ob != null)
            {
                cfg.ControllerIp = ob["ControllerIp"]?.GetValue<string>() ?? cfg.ControllerIp;
                cfg.ControllerPort = ob["ControllerPort"]?.GetValue<int>() ?? cfg.ControllerPort;
                cfg.ScreenWidth = ob["ScreenWidth"]?.GetValue<int>() ?? cfg.ScreenWidth;
                cfg.ScreenHeight = ob["ScreenHeight"]?.GetValue<int>() ?? cfg.ScreenHeight;
                cfg.DeviceType = ob["DeviceType"]?.GetValue<int>() ?? cfg.DeviceType;
                cfg.ConnectionMode = ob["ConnectionMode"]?.GetValue<string>() ?? cfg.ConnectionMode;
            }
        }
        catch { }
    }

    private static void LoadComposeConfig(AppConfig cfg, string pointId)
    {
        var path = Path.Combine(AppDir, "layout", "points", $"{pointId}.compose.json");
        if (!File.Exists(path)) return;
        try
        {
            var text = File.ReadAllText(path);
            var root = JsonNode.Parse(text, null, _readOpts)!.AsObject();

            var canvas = root["canvas"]?.AsObject();
            if (canvas != null)
            {
                cfg.CanvasWidth = canvas["width"]?.GetValue<int>() ?? cfg.CanvasWidth;
                cfg.CanvasHeight = canvas["height"]?.GetValue<int>() ?? cfg.CanvasHeight;
            }

            var gl = root["gridLayout"]?.AsObject();
            if (gl != null)
            {
                // ── Columns model (preferred). Fall back to legacy left/right. ──
                var columnsArr = gl["columns"]?.AsArray();
                if (columnsArr is { Count: > 0 })
                {
                    cfg.Columns = [];
                    cfg.ColumnBuyLabels = [];
                    cfg.ColumnSellLabels = [];
                    cfg.ColumnX = [];
                    foreach (var colNode in columnsArr)
                    {
                        var col = colNode?.AsObject();
                        if (col == null) continue;
                        cfg.Columns.Add(ReadStringArray(col["codes"]));
                        var buy = ReadStringArray(col["buy"]);
                        var sell = ReadStringArray(col["sell"]);
                        cfg.ColumnBuyLabels.Add(buy.Count > 0 ? buy : [.. AppConfig.DefaultBuyLabels]);
                        cfg.ColumnSellLabels.Add(sell.Count > 0 ? sell : [.. AppConfig.DefaultSellLabels]);
                        cfg.ColumnX.Add(col["x"]?.GetValue<int>());
                    }
                    cfg.ColumnCount = gl["columnCount"]?.GetValue<int>() ?? cfg.Columns.Count;
                }
                else
                {
                    var left = ReadStringArray(gl["left"]);
                    var right = ReadStringArray(gl["right"]);
                    var mode = gl["mode"]?.GetValue<string>() ?? "singleColumn";
                    if (string.Equals(mode, "dualColumn", StringComparison.OrdinalIgnoreCase) || right.Count > 0)
                    {
                        cfg.Columns = [left, right];
                        cfg.ColumnCount = 2;
                    }
                    else
                    {
                        cfg.Columns = [left];
                        cfg.ColumnCount = 1;
                    }
                }
                cfg.NormalizeColumns();

                int GetI(string key, int def) => gl[key]?.GetValue<int>() ?? def;
                double GetD(string key, double def) => gl[key]?.GetValue<double>() ?? def;

                cfg.LogoX = GetI("logoX", cfg.LogoX);
                cfg.LogoY = GetI("logoY", cfg.LogoY);
                cfg.LogoW = GetI("logoW", cfg.LogoW);
                cfg.LogoH = GetI("logoH", cfg.LogoH);
                cfg.HeaderBuyX = GetI("headerBuyX", cfg.HeaderBuyX);
                cfg.HeaderBuyY = GetI("headerBuyY", cfg.HeaderBuyY);
                cfg.HeaderSellX = GetI("headerSellX", cfg.HeaderSellX);
                cfg.HeaderSellY = GetI("headerSellY", cfg.HeaderSellY);
                cfg.RowsStartY = GetI("rowsStartY", cfg.RowsStartY);
                cfg.RowH = GetI("rowH", cfg.RowH);
                cfg.HeaderH = GetI("headerH", cfg.HeaderH);
                cfg.ColFlagX = GetI("colFlagX", cfg.ColFlagX);
                cfg.ColFlagW = GetI("colFlagW", cfg.ColFlagW);
                cfg.ColFlagH = GetI("colFlagH", cfg.ColFlagH);
                cfg.TickerEnabled = gl["ticker"]?["enabled"]?.GetValue<bool>() ?? cfg.TickerEnabled;
                cfg.TickerSpeed = gl["ticker"]?["speed"]?.GetValue<double>() ?? cfg.TickerSpeed;
                cfg.TickerBgColor = gl["ticker"]?["bgColor"]?.GetValue<string>() ?? cfg.TickerBgColor;
                cfg.TickerTextColor = gl["ticker"]?["textColor"]?.GetValue<string>() ?? cfg.TickerTextColor;
                cfg.ShineEnabled = gl["shine"]?["enabled"]?.GetValue<bool>() ?? cfg.ShineEnabled;
                cfg.ShineCount = gl["shine"]?["count"]?.GetValue<int>() ?? cfg.ShineCount;
                cfg.ShineStrength = gl["shine"]?["strength"]?.GetValue<double>() ?? cfg.ShineStrength;
                cfg.ShineWidth = gl["shine"]?["width"]?.GetValue<double>() ?? cfg.ShineWidth;
                cfg.TickerText = gl["ticker"]?["text"]?.GetValue<string>() ?? cfg.TickerText;
                cfg.TickerH = gl["ticker"]?["h"]?.GetValue<int>() ?? cfg.TickerH;
                cfg.TickerFontSize = gl["ticker"]?["fontSize"]?.GetValue<int>() ?? cfg.TickerFontSize;
                cfg.ColCodeX = GetI("colCodeX", cfg.ColCodeX);
                cfg.ColBuyX = GetI("colBuyX", cfg.ColBuyX);
                cfg.ColBuyW = GetI("colBuyW", cfg.ColBuyW);
                cfg.ColSellX = GetI("colSellX", cfg.ColSellX);
                cfg.ColSellW = GetI("colSellW", cfg.ColSellW);
                cfg.FszHdr = GetI("fszHdr", cfg.FszHdr);
                cfg.FszCode = GetI("fszCode", cfg.FszCode);
                cfg.FszValue = GetI("fszValue", cfg.FszValue);
                cfg.FszArrow = GetI("fszArrow", cfg.FszArrow);
                cfg.FontScaleX = GetD("fontScaleX", cfg.FontScaleX);
                cfg.TextStroke = GetI("textStroke", cfg.TextStroke);
                cfg.ValueShiftX = GetI("valueShiftX", cfg.ValueShiftX);
                cfg.Oversample = GetI("oversample", cfg.Oversample);
            }
        }
        catch { }
    }

    public static void Save(AppConfig cfg)
    {
        SaveAppSettings(cfg);
        SavePointConfig(cfg);
        SaveComposeConfig(cfg);
        UpdateScreenXml(cfg);
    }

    /// <summary>
    /// Keeps the LedShow screen.xml descriptor in sync with the settings: panel
    /// size, device type/name, controller IP, ports and credentials. Device-specific
    /// identifiers (mac, network_id, remote_ftp_dir, barcode, seq_id) are left intact.
    /// </summary>
    private static void UpdateScreenXml(AppConfig cfg)
    {
        // The legacy descriptor is shared by all points; the second board must not overwrite
        // the first board's panel size / IP in it (the Onbon pipeline does not read it anyway).
        if (cfg.BoardIndex == 1) return;
        try
        {
            var rel = ReadPointScreenXmlPath(cfg) ?? Path.Combine("config", "screen.xml");
            var path = Path.IsPathRooted(rel) ? rel : Path.Combine(AppDir, rel);
            if (!File.Exists(path)) return;

            var doc = XDocument.Load(path);
            var screen = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "screen");
            if (screen == null) return;

            void Set(string attr, string val)
            {
                if (screen.Attribute(attr) != null) screen.SetAttributeValue(attr, val);
            }

            Set("w", cfg.ScreenWidth.ToString());
            Set("h", cfg.ScreenHeight.ToString());
            Set("device_type", cfg.DeviceType.ToString());
            var model = ControllerCatalog.FindByDeviceType(cfg.DeviceType);
            if (model != null) { Set("device_name", model.Name); Set("place", model.Name); }
            Set("ip", cfg.ControllerIp);
            Set("ftp_ip", cfg.ControllerIp);
            Set("tcp_port", cfg.ControllerPort.ToString());
            Set("ftp_port", cfg.FtpPort.ToString());
            Set("user", cfg.FtpUser);
            Set("pwd", cfg.FtpPassword);
            // ip_mode: 1 = fixed/direct IP (固定IP), 2 = server mode (服务器模式).
            Set("ip_mode", ConnectionModeCatalog.FindByKey(cfg.ConnectionMode).IpMode.ToString());

            var program = screen.Descendants().FirstOrDefault(e => e.Name.LocalName == "program");
            if (program?.Attribute("DeviceType") != null)
                program.SetAttributeValue("DeviceType", cfg.DeviceType.ToString());

            doc.Save(path);
        }
        catch { /* screen.xml is optional metadata; never block saving on it */ }
    }

    private static string? ReadPointScreenXmlPath(AppConfig cfg)
    {
        try
        {
            var pc = Path.Combine(AppDir, "config", "points", $"{cfg.ActivePointId}.json");
            if (!File.Exists(pc)) return null;
            var root = JsonNode.Parse(File.ReadAllText(pc), null, _readOpts)!.AsObject();
            return root["LedUpdater"]?["ScreenXmlPath"]?.GetValue<string>();
        }
        catch { return null; }
    }


    private static void SaveAppSettings(AppConfig cfg)
    {
        // Read existing to preserve structure, then patch
        JsonObject root;
        try
        {
            var text = File.ReadAllText(AppSettingsPath);
            root = JsonNode.Parse(text, null, _readOpts)!.AsObject();
        }
        catch
        {
            root = new JsonObject();
        }

        // Board assignment: board 1 / board 2 points are kept apart from "the point being edited".
        root["ActivePointId"] = string.IsNullOrWhiteSpace(cfg.PrimaryPointId) ? cfg.ActivePointId : cfg.PrimaryPointId;
        if (cfg.HasSecondBoard) root["SecondPointId"] = cfg.SecondPointId;
        else root.Remove("SecondPointId");
        root["Urls"] = cfg.Urls;
        root["AdminPassword"] = cfg.AdminPassword;

        var led = root["Led"]?.AsObject() ?? new JsonObject();
        led["Family"] = cfg.ControllerFamily;
        root["Led"] = led;

        // NOTE: the "Telegram" section (Enabled/BotToken/ChatId) is intentionally NOT written
        // here — it is configured by hand in appsettings.json and must not be overwritten on Save.

        var lu = root["LedUpdater"]?.AsObject() ?? new JsonObject();
        lu["RunMode"] = cfg.RunMode;
        lu["PublishMode"] = cfg.PublishMode;
        lu["PollSeconds"] = cfg.PollSeconds;
        lu["RatesFetchIntervalMinutes"] = cfg.RatesFetchIntervalMinutes;
        lu["FtpUser"] = cfg.FtpUser;
        lu["FtpPassword"] = cfg.FtpPassword;
        lu["FtpPort"] = cfg.FtpPort;
        lu["UseTls"] = cfg.UseTls;
        lu["SkipIfUnchanged"] = cfg.SkipIfUnchanged;
        lu["ForceComposeEveryPoll"] = cfg.ForceComposeEveryPoll;
        lu["LayoutTestMode"] = cfg.LayoutTestMode;
        lu["PermanentInternet"] = cfg.PermanentInternet;
        lu["EnforceWifiOnly"] = cfg.EnforceWifiOnly;
        lu["RequirePrivateAddress"] = cfg.RequirePrivateAddress;
        lu["RatesApiUrl"] = cfg.RatesApiUrl;
        root["LedUpdater"] = lu;

        var ob = root["OnbonLed"]?.AsObject() ?? new JsonObject();
        ob["Enabled"] = cfg.OnbonEnabled;
        ob["UserName"] = cfg.OnbonUserName;
        ob["Password"] = cfg.OnbonPassword;
        ob["TempPath"] = cfg.TempPath;
        ob["RetryCount"] = cfg.RetryCount;
        ob["RetryDelayMs"] = cfg.RetryDelayMs;
        ob["ConnectionTimeoutMs"] = cfg.ConnectionTimeoutMs;
        ob["PollSeconds"] = cfg.OnbonPollSeconds;
        ob["AutoSend"] = cfg.AutoSend;
        ob["UseIsolatedSender"] = cfg.UseIsolatedSender;
        ob["SkipDuplicateUploads"] = cfg.SkipDuplicateUploads;
        ob["RejectSizeMismatchBeforePublish"] = cfg.RejectSizeMismatchBeforePublish;
        ob["ConnectionMode"] = cfg.ConnectionMode;
        root["OnbonLed"] = ob;

        File.WriteAllText(AppSettingsPath, root.ToJsonString(_writeOpts));
    }

    private static void SavePointConfig(AppConfig cfg)
    {
        var path = Path.Combine(AppDir, "config", "points", $"{cfg.ActivePointId}.json");

        JsonObject root;
        try
        {
            root = JsonNode.Parse(File.ReadAllText(path), null, _readOpts)!.AsObject();
        }
        catch
        {
            root = new JsonObject();
        }

        var led = root["Led"]?.AsObject() ?? new JsonObject();
        led["Family"] = cfg.ControllerFamily;
        root["Led"] = led;

        var lu = root["LedUpdater"]?.AsObject() ?? new JsonObject();
        lu["ControllerReloadUrl"] = cfg.ControllerReloadUrl;
        lu["ForceRemoteRoot"] = cfg.ForceRemoteRoot;
        lu["WifiSsid"] = cfg.WifiSsid;

        // These are required by ServiceOptions (ValidateOnStart). They are normally
        // present in each point's config, but guarantee them for freshly-added points
        // so the service never fails to start with "The WatchFolder field is required.".
        var id = cfg.ActivePointId;
        if (lu["WatchFolder"] == null) lu["WatchFolder"] = $"content/points/{id}/output";
        if (lu["ScreenXmlPath"] == null) lu["ScreenXmlPath"] = "config/screen.xml";
        if (lu["ComposeConfigPath"] == null) lu["ComposeConfigPath"] = $"layout/points/{id}.compose.json";
        if (lu["RatesJsonPath"] == null) lu["RatesJsonPath"] = $"content/points/{id}/rates.json";
        if (lu["NetworkJsonPath"] == null) lu["NetworkJsonPath"] = $"content/points/{id}/network.json";
        if (lu["RelayOutRoot"] == null) lu["RelayOutRoot"] = $"relay-output/{id}";
        root["LedUpdater"] = lu;

        var ob = root["OnbonLed"]?.AsObject() ?? new JsonObject();
        ob["ControllerIp"] = cfg.ControllerIp;
        ob["ControllerPort"] = cfg.ControllerPort;
        ob["ScreenWidth"] = cfg.ScreenWidth;
        ob["ScreenHeight"] = cfg.ScreenHeight;
        ob["DeviceType"] = cfg.DeviceType;
        ob["ConnectionMode"] = cfg.ConnectionMode;
        root["OnbonLed"] = ob;

        File.WriteAllText(path, root.ToJsonString(_writeOpts));
    }

    private static List<string> ReadStringArray(JsonNode? node)
    {
        var arr = node?.AsArray();
        if (arr == null) return [];
        return arr.Select(n => n?.GetValue<string>() ?? "").Where(s => s != "").ToList();
    }

    private static void SaveComposeConfig(AppConfig cfg)
    {
        var path = Path.Combine(AppDir, "layout", "points", $"{cfg.ActivePointId}.compose.json");
        if (!File.Exists(path)) return;
        var root = BuildComposeObject(cfg, path);
        File.WriteAllText(path, root.ToJsonString(_writeOpts));
    }

    /// <summary>
    /// Builds a temporary compose JSON file from the current edits, redirecting the
    /// output image to a temp path. Used for live preview without touching the
    /// production compose.json or the watched output image the board service sends.
    /// Returns (composePath, expectedOutputPath).
    /// </summary>
    public static (string composePath, string outputPath) WriteTempCompose(AppConfig cfg)
    {
        var basePath = Path.Combine(AppDir, "layout", "points", $"{cfg.ActivePointId}.compose.json");
        var root = BuildComposeObject(cfg, File.Exists(basePath) ? basePath : null);

        var outputPath = Path.Combine(Path.GetTempPath(), $"ecash_preview_{cfg.ActivePointId}.jpg");
        root["outputFile"] = outputPath;

        var composePath = Path.Combine(Path.GetTempPath(), $"ecash_preview_{cfg.ActivePointId}.compose.json");
        File.WriteAllText(composePath, root.ToJsonString(_writeOpts));
        return (composePath, outputPath);
    }

    private static JsonObject BuildComposeObject(AppConfig cfg, string? templatePath)
    {
        JsonObject root;
        try
        {
            root = templatePath != null && File.Exists(templatePath)
                ? JsonNode.Parse(File.ReadAllText(templatePath), null, _readOpts)!.AsObject()
                : new JsonObject();
        }
        catch
        {
            root = new JsonObject();
        }

        var canvas = root["canvas"]?.AsObject() ?? new JsonObject();
        canvas["width"] = cfg.CanvasWidth;
        canvas["height"] = cfg.CanvasHeight;
        root["canvas"] = canvas;

        // sourceDir/outputFile are required by DotnetComposer.ComposeConfig. Preserve
        // the template's values, but always guarantee they are present so a regenerated
        // compose.json never fails deserialization with "missing required SourceDir".
        if (root["sourceDir"] == null)
            root["sourceDir"] = "content/common";
        if (root["outputFile"] == null)
            root["outputFile"] = $"content/points/{cfg.ActivePointId}/output/final.jpg";

        cfg.NormalizeColumns();

        var gl = root["gridLayout"]?.AsObject() ?? new JsonObject();
        gl["mode"] = "columns";
        gl["columnCount"] = cfg.ColumnCount;

        // Columns array (codes + per-column buy/sell labels)
        var columnsArr = new JsonArray();
        for (int i = 0; i < cfg.ColumnCount; i++)
        {
            var codes = new JsonArray();
            foreach (var c in cfg.Columns[i]) codes.Add(c);

            var buy = new JsonArray();
            foreach (var l in cfg.ColumnBuyLabels[i]) buy.Add(l);

            var sell = new JsonArray();
            foreach (var l in cfg.ColumnSellLabels[i]) sell.Add(l);

            var colObj = new JsonObject { ["codes"] = codes, ["buy"] = buy, ["sell"] = sell };
            var colX = cfg.ColumnX.ElementAtOrDefault(i);
            if (colX.HasValue) colObj["x"] = colX.Value;
            columnsArr.Add(colObj);
        }
        gl["columns"] = columnsArr;

        // Keep legacy left/right in sync for any external tooling
        var leftArr = new JsonArray();
        foreach (var c in cfg.Columns.ElementAtOrDefault(0) ?? []) leftArr.Add(c);
        gl["left"] = leftArr;

        var rightArr = new JsonArray();
        foreach (var c in cfg.Columns.ElementAtOrDefault(1) ?? []) rightArr.Add(c);
        gl["right"] = rightArr;

        // Update flagFiles to match all selected currencies
        var flagFiles = new JsonObject();
        foreach (var code in cfg.AllCurrencies)
        {
            var existing = gl["flagFiles"]?.AsObject()?[code]?.GetValue<string>();
            flagFiles[code] = existing ?? $"{code.ToLowerInvariant()}.png";
        }
        gl["flagFiles"] = flagFiles;
        gl["singleRows"] = cfg.Columns.Max(c => c.Count);

        // Geometry
        gl["logoX"] = cfg.LogoX;
        gl["logoY"] = cfg.LogoY;
        gl["logoW"] = cfg.LogoW;
        gl["logoH"] = cfg.LogoH;
        gl["headerBuyX"] = cfg.HeaderBuyX;
        gl["headerBuyY"] = cfg.HeaderBuyY;
        gl["headerSellX"] = cfg.HeaderSellX;
        gl["headerSellY"] = cfg.HeaderSellY;
        gl["rowsStartY"] = cfg.RowsStartY;
        gl["rowH"] = cfg.RowH;
        gl["headerH"] = cfg.HeaderH;
        gl["colFlagX"] = cfg.ColFlagX;
        gl["colFlagW"] = cfg.ColFlagW;
        gl["colFlagH"] = cfg.ColFlagH;

        // Keep whatever else the point tuned in the ticker (colors, langs, height …) —
        // the UI only drives the switch, the speed and the text.
        var ticker = gl["ticker"]?.AsObject() ?? new JsonObject();
        ticker["enabled"] = cfg.TickerEnabled;
        ticker["speed"] = Math.Round(cfg.TickerSpeed, 1);
        ticker["bgColor"] = cfg.TickerBgColor;
        ticker["textColor"] = cfg.TickerTextColor;
        // 0 = automatic → drop the key so the composer picks its defaults
        if (cfg.TickerH > 0) ticker["h"] = cfg.TickerH; else ticker.Remove("h");
        if (cfg.TickerFontSize > 0) ticker["fontSize"] = cfg.TickerFontSize; else ticker.Remove("fontSize");
        if (string.IsNullOrWhiteSpace(cfg.TickerText)) ticker.Remove("text");
        else ticker["text"] = cfg.TickerText.Trim();
        gl["ticker"] = ticker;

        // Keep the point's own tuning (count, strength, width) - the UI drives the switch.
        var shine = gl["shine"]?.AsObject() ?? new JsonObject();
        shine["enabled"] = cfg.ShineEnabled;
        shine["count"] = cfg.ShineCount;
        shine["strength"] = Math.Round(cfg.ShineStrength, 2);
        shine["width"] = Math.Round(cfg.ShineWidth, 2);
        gl["shine"] = shine;
        gl["colCodeX"] = cfg.ColCodeX;
        gl["colBuyX"] = cfg.ColBuyX;
        gl["colBuyW"] = cfg.ColBuyW;
        gl["colSellX"] = cfg.ColSellX;
        gl["colSellW"] = cfg.ColSellW;
        gl["fszHdr"] = cfg.FszHdr;
        gl["fszCode"] = cfg.FszCode;
        gl["fszValue"] = cfg.FszValue;
        gl["fszArrow"] = cfg.FszArrow;
        gl["fontScaleX"] = cfg.FontScaleX;
        gl["textStroke"] = cfg.TextStroke;
        gl["valueShiftX"] = cfg.ValueShiftX;
        gl["oversample"] = cfg.Oversample;

        root["gridLayout"] = gl;
        return root;
    }

    public static readonly Dictionary<string, string> KnownCurrencies = new()
    {
        ["USD"] = "Доллар США",
        ["EUR"] = "Евро",
        ["RUB"] = "Российский рубль",
        ["CNY"] = "Китайский юань",
        ["KGS"] = "Кыргызский сом",
        ["TRY"] = "Турецкая лира",
        ["GBP"] = "Британский фунт",
        ["CHF"] = "Швейцарский франк",
        ["JPY"] = "Японская иена",
        ["KRW"] = "Южнокорейская вона",
        ["THB"] = "Тайский бат",
        ["AED"] = "Дирхам ОАЭ",
        ["INR"] = "Индийская рупия",
        ["KZT"] = "Казахстанский тенге",
        ["GEL"] = "Грузинский лари",
        ["CAD"] = "Канадский доллар",
        ["AUD"] = "Австралийский доллар",
        ["SEK"] = "Шведская крона",
        ["NOK"] = "Норвежская крона",
    };
}
