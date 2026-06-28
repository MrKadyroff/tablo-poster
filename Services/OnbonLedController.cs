using System.Net.Sockets;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using LedImageUpdaterService.Models;
using Microsoft.Extensions.Options;
using GdiBitmap = System.Drawing.Bitmap;
using GdiGraphics = System.Drawing.Graphics;
using GdiSize = System.Drawing.Size;
using GdiInterpolationMode = System.Drawing.Drawing2D.InterpolationMode;
using GdiSmoothingMode = System.Drawing.Drawing2D.SmoothingMode;
using GdiPixelFormat = System.Drawing.Imaging.PixelFormat;
using GdiImageFormat = System.Drawing.Imaging.ImageFormat;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;
using Size = SixLabors.ImageSharp.Size;

namespace LedImageUpdaterService.Services;

/// <summary>
/// Wraps the Onbon YQNetCom.dll SDK via P/Invoke for direct TCP communication
/// with BX-Y series LED controllers.
///
/// Platform safety:
///   • DllImport declarations compile on any OS.
///   • All actual SDK calls are guarded by <see cref="OperatingSystem.IsWindows()"/>
///     so the service starts cleanly on macOS/Linux for development.
///   • Set <c>OnbonLed:Enabled = false</c> in appsettings to suppress even the
///     "non-Windows" warning during local development.
/// </summary>
public sealed class OnbonLedController : IDisposable, ILedController
{
    private readonly ILogger<OnbonLedController> _logger;
    private readonly OnbonOptions _options;
    private readonly InMemoryLogStore _logStore;

    // Ensures only one SDK operation runs at a time (YQNetCom.dll is not thread-safe).
    private readonly SemaphoreSlim _sdkLock = new(1, 1);
    private readonly object _hashLock = new();

    private bool _sdkInitialized;
    private bool _disposed;
    private string? _lastSentImageHash;
    private int _lastSdkSendErrorCode;
    private readonly bool _isHelperProcess =
        string.Equals(Environment.GetEnvironmentVariable("ONBON_HELPER_MODE"), "1", StringComparison.Ordinal);

    // ─── Process-wide SDK state ──────────────────────────────────────────────
    // The native YQNetCom.dll keeps PROCESS-GLOBAL state: init_sdk()/release_sdk()
    // and the P/Invoke resolver must run exactly once per process. The tray app
    // rebuilds the whole host (and this singleton) on "Restart", so without these
    // guards the second InitializeSdk() throws (resolver already set) or crashes
    // the process by re-initializing the native SDK. These statics let a restarted
    // host safely reuse the SDK that was initialized on first launch.
    private static readonly object _sdkInitGate = new();
    private static bool _sdkInitializedProcessWide;
    private static bool _sdkResolverRegistered;
    private static bool _processExitReleaseHooked;
    private static bool _sdkReleasedProcessWide;

    // ─── P/Invoke declarations (YQNetCom.dll) ────────────────────────────────
    // These are only CALLED on Windows; declarations compile on every OS.

    [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
    private static extern int init_sdk();

    [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
    private static extern int release_sdk();

    [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr create_playlist(int w, int h, int device_type);

    [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
    private static extern void delete_playlist(IntPtr playlist);

    [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr create_program(string name, string bg_color);

    [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
    private static extern void delete_program(IntPtr program_area);

    [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr create_pic();

    [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
    private static extern void delete_pic(IntPtr area_tree);

    /// <param name="area_tree">Pic area handle</param>
    /// <param name="stay_time">Dwell time in seconds (0 = display once)</param>
    /// <param name="display_effects">Transition effect type (0 = none)</param>
    /// <param name="display_speed">Transition speed 1–16 (1=fastest)</param>
    /// <param name="path">Absolute path to the image file</param>
    [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
    private static extern int add_pic_unit(IntPtr area_tree, int stay_time, int display_effects, int display_speed, string path);

    [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
    private static extern int add_pic(IntPtr tree, IntPtr area_tree, int x, int y, int w, int h, int transparency);

    [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr create_dynamic();

    [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
    private static extern void delete_dynamic(IntPtr area_tree);

    [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
    private static extern int add_dynamic(
        IntPtr tree, IntPtr area_tree,
        int dynamic_id, int x, int y, int w, int h,
        string relative_program, int run_mode, string update_frequency,
        int transparency);

    [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
    private static extern int add_dynamic_unit(
        IntPtr dynamic_area, int dynamic_type, int display_effects, int display_speed, int stay_time,
        string file_path, int gif_flag, string bg_color,
        int font_size, string font_name, string font_color, string font_attributes,
        string align_h, string align_v,
        int volumn, int scale_mode, int rolation_mode,
        string key_list, string proxyService, int isLocal);

    [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
    private static extern int update_dynamic(
        byte[] ip, ushort port, string user_name, string user_pwd,
        IntPtr dynamic_playlist, string immediately_play, int conver, int onlyUpdate);

    [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
    private static extern int add_program_in_playlist(
        IntPtr playlist, IntPtr program,
        int play_mode, int play_time,
        string aging_start_time, string aging_end_time,
        string period_ontime, string period_offtime,
        int play_week);

    /// <param name="send_style">0=normal, 2=insert, 3=offline, 4=send only</param>
    [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
    private static extern int send_program(
        byte[] ip, ushort port,
        string user_name, string user_pwd,
        string tmp_path, IntPtr playlist,
        int send_style,
        ref long free_size, ref long total_size,
        IntPtr default_program_infos, byte[] playlist_name,
        int is_make = 0, int is_save = 1);

    [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
    private static extern int clear_all_program(byte[] ip, ushort port, string user_name, string user_pwd);

    [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
    private static extern int get_firmware_version(
        byte[] ip, ushort port, string user_name, string user_pwd,
        byte[] firmware_version, byte[] app_version, byte[] fpga_version);

    [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
    private static extern int get_screen_status(
        byte[] ip, ushort port, string user_name, string user_pwd,
        ref int screen_onoff, ref int brigtness, ref int brigtness_mode,
        ref int volume, ref int screen_lockunlock, ref int program_lockunlock,
        ref int screen_output_type, ref int screen_player_mode,
        byte[] screen_time, byte[] screen_addr, byte[] screen_customer_onoff,
        byte[] screen_language, byte[] screen_gps);

    [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
    private static extern int get_screen_parameters(
        byte[] ip, ushort port, string user_name, string user_pwd, byte[] datas);

    [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
    private static extern int set_screen_brightness(
        byte[] ip, ushort port, string user_name, string user_pwd, int brightness);

    /// <param name="turnonoff">1 = power ON, 0 = power OFF</param>
    [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
    private static extern int set_screen_turnonoff(
        byte[] ip, ushort port, string user_name, string user_pwd, int turnonoff);

    [DllImport("YQNetCom.dll", CharSet = CharSet.Unicode)]
    private static extern int reboot(
        byte[] ip, ushort port, string user_name, string user_pwd);

    // ─── ControllerInfo struct (mirrors SDK ControllerInfo, Pack=1) ──────────
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct ControllerInfo
    {
        public int is_dhcp;
        public int wifi_is_dhcp;
        public int output_type;
        public int port;
        public int screen_w;
        public int screen_h;
        public int screen_volume;
        public int screen_brigtness;
        public int screen_brigtness_mode;
        public int fold_type;
        public int fold_count;
        public int logic_width;
        public int logic_height;
        public int screen_rotation;
        public int flag;
        public int screen_file_verify_switch;
        public ushort screen_type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] public byte[] storagemedia;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 34)] public byte[] barcode;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)] public byte[] ip;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)] public byte[] source_ip;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)] public byte[] sub_mark;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)] public byte[] gateway;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)] public byte[] mac;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)] public byte[] local_ip;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 66)] public byte[] pid;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] local_net;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)] public byte[] ap_wifi_ip;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)] public byte[] wifi_ip;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)] public byte[] wifi_sub_mark;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)] public byte[] wifi_gateway;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)] public byte[] dns_server;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)] public byte[] network_device;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)] public byte[] network_mode;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)] public byte[] controller_name;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)] public byte[] fold_width;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)] public byte[] fold_height;
    }

    // ─── Constructor ─────────────────────────────────────────────────────────

    public OnbonLedController(
        ILogger<OnbonLedController> logger,
        IOptions<OnbonOptions> options,
        InMemoryLogStore logStore)
    {
        _logger = logger;
        _options = options.Value;
        _logStore = logStore;

        Log(LogLevel.Information,
            $"[Onbon] Initializing. Controller={_options.ControllerIp}:{_options.ControllerPort} " +
            $"Screen={_options.ScreenWidth}x{_options.ScreenHeight} DeviceType={_options.DeviceType} " +
            $"Enabled={_options.Enabled} OS={RuntimeInformation.OSDescription}");

        if (!_options.Enabled)
        {
            Log(LogLevel.Warning,
                "[Onbon] SDK is disabled via configuration (OnbonLed:Enabled=false). " +
                "All operations will be no-ops. Suitable for macOS development.");
            return;
        }

        if (!OperatingSystem.IsWindows())
        {
            Log(LogLevel.Warning,
                "[Onbon] Non-Windows OS detected. YQNetCom.dll calls will be skipped at runtime. " +
                "Set OnbonLed:Enabled=false to suppress this warning during development.");
            return;
        }

        InitializeSdk();
    }

    // ─── Public API ──────────────────────────────────────────────────────────

    /// <summary>
    /// Checks TCP connectivity to the LED controller.
    /// Platform-independent — does not call YQNetCom.dll.
    /// </summary>
    public async Task<ConnectionCheckResult> CheckConnectionAsync(CancellationToken ct = default)
    {
        var ip = _options.ControllerIp;
        var port = _options.ControllerPort;

        Log(LogLevel.Information,
            $"[Connection] Probing {ip}:{port} (timeout={_options.ConnectionTimeoutMs} ms)...");

        try
        {
            using var tcp = new TcpClient();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(_options.ConnectionTimeoutMs);

            await tcp.ConnectAsync(ip, port, cts.Token);

            var details = $"Controller {ip}:{port} is reachable (TCP OK).";
            Log(LogLevel.Information, $"[Connection] {details}");
            return new ConnectionCheckResult(true, details);
        }
        catch (OperationCanceledException)
        {
            var details = $"Connection to {ip}:{port} timed out after {_options.ConnectionTimeoutMs} ms.";
            Log(LogLevel.Warning, $"[Connection] {details}");
            return new ConnectionCheckResult(false, details);
        }
        catch (Exception ex)
        {
            var details = $"Connection to {ip}:{port} failed: {ex.Message}";
            Log(LogLevel.Warning, $"[Connection] {details}");
            return new ConnectionCheckResult(false, details);
        }
    }

    /// <summary>
    /// Sends an image file to the LED controller and returns typed error details.
    /// This method is used by the upload endpoint to provide stable diagnostics.
    /// </summary>
    public async Task<LedSendStatus> SendImageWithStatusAsync(
        string imagePath, bool bypassDuplicateCheck = false, CancellationToken ct = default)
    {
        Log(LogLevel.Information,
            $"[SendImage] Requested: {imagePath}{(bypassDuplicateCheck ? " (manual — duplicate check bypassed)" : "")}");

        if (!File.Exists(imagePath))
        {
            Log(LogLevel.Error, $"[SendImage] File not found: {imagePath}");
            return LedSendStatus.Fail(LedSendErrorType.FileNotFound, $"File not found: {imagePath}");
        }

        if (!IsSdkAvailable("SendImage"))
            return LedSendStatus.Fail(LedSendErrorType.SdkNotInitialized,
                "SDK not initialized or unavailable on current platform.");

        var sizeValidation = await ValidateImageSizeBeforePublishAsync(imagePath, ct);
        if (sizeValidation is not null)
            return sizeValidation;

        // SDK works best with BMP — convert other formats before sending.
        string bmpPath = imagePath;
        string? jpgPath = null;
        bool isTemp = false;
        bool isJpgTemp = false;
        bool convertedFromOriginal = false;
        string? normalizedBmpHash = null;

        if (!imagePath.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
        {
            (bmpPath, isTemp) = (await ConvertToBmpAsync(imagePath, ct), true);
            convertedFromOriginal = true;
        }

        try
        {
            // Always compute the hash (so a successful send records it for the next
            // auto-send to dedup against), but only short-circuit on a duplicate when the
            // caller did not request a bypass. Manual sends pass bypassDuplicateCheck=true
            // so pressing "Отправить на табло" always re-pushes the same image.
            if (_options.SkipDuplicateUploads)
            {
                normalizedBmpHash = await ComputeSha256Async(bmpPath, ct);
                if (!bypassDuplicateCheck)
                {
                    lock (_hashLock)
                    {
                        if (_lastSentImageHash is not null && _lastSentImageHash == normalizedBmpHash)
                        {
                            Log(LogLevel.Information,
                                "[SendImage] Skipped duplicate image (same normalized BMP hash as previous send).");
                            return LedSendStatus.Ok("Skipped duplicate image (already sent).", duplicateSkipped: true);
                        }
                    }
                }
            }

            var bmpStatus = await SendPreparedImagePathAsync(
                bmpPath,
                imagePath,
                convertedFromOriginal,
                ct);

            if (bmpStatus.Success)
            {
                if (_options.SkipDuplicateUploads && normalizedBmpHash is not null)
                {
                    lock (_hashLock) _lastSentImageHash = normalizedBmpHash;
                }

                return LedSendStatus.Ok("Image sent successfully (format=bmp).");
            }

            Log(LogLevel.Warning,
                $"[SendImage] BMP attempt failed: {bmpStatus.Message}. Trying JPG...");

            jpgPath = await ConvertToJpegAsync(imagePath, ct);
            isJpgTemp = true;

            var jpgStatus = await SendPreparedImagePathAsync(
                jpgPath,
                imagePath,
                convertedFromOriginal: false,
                ct);

            if (jpgStatus.Success)
            {
                if (_options.SkipDuplicateUploads && normalizedBmpHash is not null)
                {
                    lock (_hashLock) _lastSentImageHash = normalizedBmpHash;
                }

                return LedSendStatus.Ok("Image sent successfully (format=jpg).");
            }

            var combinedMessage =
                $"BMP attempt failed: {bmpStatus.Message}; JPG attempt failed: {jpgStatus.Message}";

            return LedSendStatus.Fail(
                jpgStatus.ErrorType != LedSendErrorType.None ? jpgStatus.ErrorType : bmpStatus.ErrorType,
                combinedMessage);
        }
        catch (OperationCanceledException)
        {
            return LedSendStatus.Fail(LedSendErrorType.Cancelled, "Image send cancelled.");
        }
        catch (Exception ex)
        {
            Log(LogLevel.Error, $"[SendImage] Unexpected error: {ex}");
            return LedSendStatus.Fail(LedSendErrorType.UnexpectedError, ex.Message);
        }
        finally
        {
            if (isTemp && File.Exists(bmpPath))
                try { File.Delete(bmpPath); } catch { /* ignore temp cleanup error */ }

            if (isJpgTemp && jpgPath is not null && File.Exists(jpgPath))
                try { File.Delete(jpgPath); } catch { /* ignore temp cleanup error */ }
        }
    }

    private async Task<LedSendStatus?> ValidateImageSizeBeforePublishAsync(string imagePath, CancellationToken ct)
    {
        var info = await Image.IdentifyAsync(imagePath, ct);
        if (info is null)
        {
            const string msg = "Cannot read image dimensions before publish.";
            Log(LogLevel.Error, $"[SendImage] {msg}");
            return LedSendStatus.Fail(LedSendErrorType.UnexpectedError, msg);
        }

        if (info.Width == _options.ScreenWidth && info.Height == _options.ScreenHeight)
            return null;

        var sizeMsg =
            $"Image size {info.Width}x{info.Height} does not match screen size {_options.ScreenWidth}x{_options.ScreenHeight}.";

        if (_options.RejectSizeMismatchBeforePublish)
        {
            Log(LogLevel.Error, $"[SendImage] {sizeMsg}");
            return LedSendStatus.Fail(LedSendErrorType.InvalidImageSize, sizeMsg);
        }

        Log(LogLevel.Warning,
            $"[SendImage] {sizeMsg} Continuing because RejectSizeMismatchBeforePublish=false.");
        return null;
    }

    private async Task<LedSendStatus> SendPreparedImagePathAsync(
        string preparedPath,
        string originalImagePath,
        bool convertedFromOriginal,
        CancellationToken ct)
    {
        LedSendStatus status;
        if (_options.UseIsolatedSender && !_isHelperProcess)
        {
            status = await SendImageViaIsolatedProcessAsync(preparedPath, ct);
        }
        else
        {
            status = await SendImageInProcessAsync(preparedPath, ct);
        }

        // SDK error 0x1e (player command) is often image-format specific on
        // certain firmware builds. Try one fallback encoding path once.
        if (!status.Success
            && status.ErrorType == LedSendErrorType.SdkSendFailed
            && _lastSdkSendErrorCode == 0x1e
            && convertedFromOriginal)
        {
            Log(LogLevel.Warning,
                "[SendImage] SDK returned error 0x1e. Retrying once with alternate BMP encoding.");

            var altBmp = await ConvertToBmpWithImageSharpAsync(originalImagePath, ct, "alt");
            try
            {
                _lastSdkSendErrorCode = 0;
                status = _options.UseIsolatedSender && !_isHelperProcess
                    ? await SendImageViaIsolatedProcessAsync(altBmp, ct)
                    : await SendImageInProcessAsync(altBmp, ct);
            }
            finally
            {
                if (File.Exists(altBmp))
                    try { File.Delete(altBmp); } catch { /* ignore */ }
            }
        }

        // If isolated sender failed with SDK error, try once in-process to
        // bypass helper-process specifics and get a cleaner native call path.
        if (!status.Success
            && status.ErrorType == LedSendErrorType.SdkSendFailed
            && _options.UseIsolatedSender
            && !_isHelperProcess)
        {
            Log(LogLevel.Warning,
                "[SendImage] Isolated sender failed with SDK error. Retrying once in-process...");

            _lastSdkSendErrorCode = 0;
            status = await SendImageInProcessAsync(preparedPath, ct);
        }

        // Some controllers recover after clearing current playlist/programs.
        // Try one recovery cycle: clear screen, short delay, resend.
        if (!status.Success
            && status.ErrorType == LedSendErrorType.SdkSendFailed)
        {
            Log(LogLevel.Warning,
                "[SendImage] SDK send failed. Trying recovery: clear screen then resend once...");

            try
            {
                await ClearScreenAsync(ct);
                await Task.Delay(400, ct);

                _lastSdkSendErrorCode = 0;
                status = (_options.UseIsolatedSender && !_isHelperProcess)
                    ? await SendImageViaIsolatedProcessAsync(preparedPath, ct)
                    : await SendImageInProcessAsync(preparedPath, ct);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log(LogLevel.Warning,
                    $"[SendImage] Recovery resend threw {ex.GetType().Name}: {ex.Message}");
            }
        }

        return status;
    }

    private async Task<LedSendStatus> SendImageInProcessAsync(string bmpPath, CancellationToken ct)
    {
        await _sdkLock.WaitAsync(ct);
        try
        {
            _lastSdkSendErrorCode = 0;

            // In helper mode prefer a single attempt to avoid repeated native calls
            // after the first deterministic SDK error.
            var ok = _isHelperProcess
                ? await Task.Run(() => SendImageCore(bmpPath), ct)
                : await ExecuteWithRetryAsync("SendImage", () => SendImageCore(bmpPath), ct);

            if (ok)
                return LedSendStatus.Ok("Image sent successfully.");

            if (_lastSdkSendErrorCode != 0)
            {
                var details = $"SDK error {_lastSdkSendErrorCode}: {MapSdkError(_lastSdkSendErrorCode)}";
                return LedSendStatus.Fail(LedSendErrorType.SdkSendFailed, details);
            }

            return LedSendStatus.Fail(LedSendErrorType.SdkSendFailed,
                "SDK send returned failure. Check /api/led/logs for step details.");
        }
        catch (SEHException ex)
        {
            Log(LogLevel.Error, $"[SendImage] Native interop error: {ex.Message}");
            return LedSendStatus.Fail(LedSendErrorType.NativeInteropError, ex.Message);
        }
        catch (AccessViolationException ex)
        {
            Log(LogLevel.Error, $"[SendImage] Access violation in native SDK: {ex.Message}");
            return LedSendStatus.Fail(LedSendErrorType.NativeInteropError, ex.Message);
        }
        catch (Exception ex)
        {
            Log(LogLevel.Error, $"[SendImage] In-process send failed: {ex}");
            return LedSendStatus.Fail(LedSendErrorType.UnexpectedError, ex.Message);
        }
        finally
        {
            _sdkLock.Release();
        }
    }

    private async Task<LedSendStatus> SendImageViaIsolatedProcessAsync(string bmpPath, CancellationToken ct)
    {
        var processPath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(processPath) || !File.Exists(processPath))
        {
            return LedSendStatus.Fail(LedSendErrorType.UnexpectedError,
                "Cannot locate current executable for isolated send mode.");
        }

        var psi = new ProcessStartInfo(processPath)
        {
            WorkingDirectory = AppContext.BaseDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        psi.ArgumentList.Add(OnbonSendIsolationHelper.HelperFlag);
        psi.ArgumentList.Add("--image");
        psi.ArgumentList.Add(bmpPath);
        psi.Environment["ONBON_HELPER_MODE"] = "1";

        using var proc = Process.Start(psi);
        if (proc is null)
            return LedSendStatus.Fail(LedSendErrorType.UnexpectedError, "Failed to start isolated sender process.");

        Task<string> outTask = proc.StandardOutput.ReadToEndAsync(ct);
        Task<string> errTask = proc.StandardError.ReadToEndAsync(ct);
        await proc.WaitForExitAsync(ct);

        var output = (await outTask).Trim();
        var error = (await errTask).Trim();
        var details = string.IsNullOrWhiteSpace(error) ? output : error;

        if (proc.ExitCode == 0)
            return LedSendStatus.Ok(string.IsNullOrWhiteSpace(details) ? "Image sent successfully." : details);

        var mapped = proc.ExitCode switch
        {
            2 => LedSendErrorType.FileNotFound,
            3 => LedSendErrorType.SdkNotInitialized,
            4 => LedSendErrorType.SdkSendFailed,
            5 => LedSendErrorType.NativeInteropError,
            6 => LedSendErrorType.Cancelled,
            7 => LedSendErrorType.InvalidImageSize,
            _ => LedSendErrorType.IsolatedProcessCrashed,
        };

        if (mapped == LedSendErrorType.IsolatedProcessCrashed)
        {
            Log(LogLevel.Error,
                $"[SendImage] Isolated sender exited with code {proc.ExitCode}. Details: {details}");
        }

        return LedSendStatus.Fail(mapped,
            string.IsNullOrWhiteSpace(details)
                ? $"Isolated sender exited with code {proc.ExitCode}."
                : details);
    }

    private static async Task<string> ComputeSha256Async(string filePath, CancellationToken ct)
    {
        await using var fs = File.OpenRead(filePath);
        var hash = await SHA256.HashDataAsync(fs, ct);
        return Convert.ToHexString(hash);
    }

    // ─── Status / Info API ────────────────────────────────────────────────────

    /// <summary>
    /// Queries firmware versions from the LED controller via YQNetCom.dll.
    /// Returns <see langword="null"/> when the SDK is unavailable or the call fails.
    /// </summary>
    public async Task<FirmwareInfo?> GetFirmwareAsync(CancellationToken ct = default)
    {
        if (!IsSdkAvailable("GetFirmware")) return null;

        await _sdkLock.WaitAsync(ct);
        try
        {
            return await Task.Run(() =>
            {
                var fw = new byte[64];
                var app = new byte[64];
                var fpga = new byte[60];
                var ip = Encoding.ASCII.GetBytes(_options.ControllerIp);

                int err = get_firmware_version(
                    ip, (ushort)_options.ControllerPort,
                    _options.UserName, _options.Password,
                    fw, app, fpga);

                if (err != 0)
                {
                    Log(LogLevel.Error, $"[GetFirmware] SDK returned error {err}.");
                    return null;
                }

                var info = new FirmwareInfo(
                    Encoding.Default.GetString(fw).TrimEnd('\0'),
                    Encoding.Default.GetString(app).TrimEnd('\0'),
                    Encoding.Default.GetString(fpga).TrimEnd('\0'));

                Log(LogLevel.Information,
                    $"[GetFirmware] firmware={info.FirmwareVersion} app={info.AppVersion} fpga={info.FpgaVersion}");
                return (FirmwareInfo?)info;
            }, ct);
        }
        finally { _sdkLock.Release(); }
    }

    /// <summary>
    /// Reads the screen status (power state, brightness, lock flags, etc.) via SDK.
    /// Returns <see langword="null"/> when the SDK is unavailable or the call fails.
    /// </summary>
    public async Task<ScreenStatusInfo?> GetScreenStatusAsync(CancellationToken ct = default)
    {
        if (!IsSdkAvailable("GetScreenStatus")) return null;

        await _sdkLock.WaitAsync(ct);
        try
        {
            return await Task.Run(() =>
            {
                int onoff = 0, brightness = 0, brightnessMode = 0, volume = 0;
                int screenLock = 0, programLock = 0, outputType = 0, playerMode = 0;
                var time = new byte[1024];
                var addr = new byte[1024];
                var custOnOff = new byte[1024];
                var language = new byte[1024];
                var gps = new byte[1024];
                var ip = Encoding.ASCII.GetBytes(_options.ControllerIp);

                int err = get_screen_status(
                    ip, (ushort)_options.ControllerPort,
                    _options.UserName, _options.Password,
                    ref onoff, ref brightness, ref brightnessMode,
                    ref volume, ref screenLock, ref programLock,
                    ref outputType, ref playerMode,
                    time, addr, custOnOff, language, gps);

                if (err != 0)
                {
                    Log(LogLevel.Error, $"[GetScreenStatus] SDK returned error {err}.");
                    return null;
                }

                var info = new ScreenStatusInfo(
                    IsPoweredOn: onoff == 0,
                    Brightness: brightness,
                    BrightnessAuto: brightnessMode == 0,
                    Volume: volume,
                    ScreenLocked: screenLock == 1,
                    ProgramLocked: programLock == 1,
                    ControllerTime: Encoding.Default.GetString(time).TrimEnd('\0').Trim());

                var powerStr = info.IsPoweredOn ? "ON" : "OFF";
                Log(LogLevel.Information,
                    $"[GetScreenStatus] power={powerStr} " +
                    $"brightness={info.Brightness} locked={info.ScreenLocked}");
                return (ScreenStatusInfo?)info;
            }, ct);
        }
        finally { _sdkLock.Release(); }
    }

    /// <summary>
    /// Reads controller hardware info (IP, screen dimensions, barcode, etc.) via SDK.
    /// Returns <see langword="null"/> when the SDK is unavailable or the call fails.
    /// </summary>
    public async Task<ControllerHardwareInfo?> GetControllerInfoAsync(CancellationToken ct = default)
    {
        if (!IsSdkAvailable("GetControllerInfo")) return null;

        await _sdkLock.WaitAsync(ct);
        try
        {
            return await Task.Run(() =>
            {
                var data = new byte[1024 * 10];
                var ip = Encoding.ASCII.GetBytes(_options.ControllerIp);

                int err = get_screen_parameters(
                    ip, (ushort)_options.ControllerPort,
                    _options.UserName, _options.Password, data);

                if (err != 0)
                {
                    Log(LogLevel.Error, $"[GetControllerInfo] SDK returned error {err}.");
                    return null;
                }

                int size = Marshal.SizeOf<ControllerInfo>();
                IntPtr ptr = Marshal.AllocHGlobal(size);
                try
                {
                    Marshal.Copy(data, 0, ptr, size);
                    var ci = Marshal.PtrToStructure<ControllerInfo>(ptr);

                    string Str(byte[] b) => Encoding.Unicode.GetString(b).Split('\0')[0]
                        .Replace("&#x0;", "").Trim();

                    var info = new ControllerHardwareInfo(
                        Barcode: Str(ci.barcode),
                        ScreenIp: Str(ci.source_ip),
                        Port: ci.port,
                        ScreenWidth: ci.screen_w,
                        ScreenHeight: ci.screen_h,
                        ScreenType: ci.screen_type,
                        Brightness: ci.screen_brigtness,
                        StorageMedia: Str(ci.storagemedia),
                        MacAddress: Str(ci.mac));

                    Log(LogLevel.Information,
                        $"[GetControllerInfo] barcode={info.Barcode} " +
                        $"screen={info.ScreenWidth}x{info.ScreenHeight} mac={info.MacAddress}");
                    return (ControllerHardwareInfo?)info;
                }
                finally { Marshal.FreeHGlobal(ptr); }
            }, ct);
        }
        finally { _sdkLock.Release(); }
    }

    /// <summary>Sets the LED panel brightness (1–255).</summary>
    public async Task<bool> SetBrightnessAsync(int brightness, CancellationToken ct = default)
    {
        if (!IsSdkAvailable("SetBrightness")) return false;
        if (brightness < 1 || brightness > 255)
            throw new ArgumentOutOfRangeException(nameof(brightness), "Must be 1–255.");

        await _sdkLock.WaitAsync(ct);
        try
        {
            return await ExecuteWithRetryAsync("SetBrightness", () =>
            {
                var ip = Encoding.ASCII.GetBytes(_options.ControllerIp);
                int err = set_screen_brightness(
                    ip, (ushort)_options.ControllerPort,
                    _options.UserName, _options.Password, brightness);
                if (err == 0) { Log(LogLevel.Information, $"[SetBrightness] Set to {brightness}."); return true; }
                Log(LogLevel.Error, $"[SetBrightness] SDK error {err}.");
                return false;
            }, ct);
        }
        finally { _sdkLock.Release(); }
    }

    /// <summary>Powers the screen ON (<c>true</c>) or OFF (<c>false</c>).</summary>
    public async Task<bool> SetPowerAsync(bool on, CancellationToken ct = default)
    {
        if (!IsSdkAvailable("SetPower")) return false;

        await _sdkLock.WaitAsync(ct);
        try
        {
            return await ExecuteWithRetryAsync("SetPower", () =>
            {
                var ip = Encoding.ASCII.GetBytes(_options.ControllerIp);
                // SDK: 1=ON, 0=OFF
                int err = set_screen_turnonoff(
                    ip, (ushort)_options.ControllerPort,
                    _options.UserName, _options.Password, on ? 1 : 0);
                var stateStr = on ? "ON" : "OFF";
                if (err == 0) { Log(LogLevel.Information, $"[SetPower] Screen powered {stateStr}."); return true; }
                Log(LogLevel.Error, $"[SetPower] SDK error {err}.");
                return false;
            }, ct);
        }
        finally { _sdkLock.Release(); }
    }

    /// <summary>Reboots the LED controller.</summary>
    public async Task<bool> RebootControllerAsync(CancellationToken ct = default)
    {
        if (!IsSdkAvailable("Reboot")) return false;

        await _sdkLock.WaitAsync(ct);
        try
        {
            return await ExecuteWithRetryAsync("Reboot", () =>
            {
                var ip = Encoding.ASCII.GetBytes(_options.ControllerIp);
                int err = reboot(
                    ip, (ushort)_options.ControllerPort,
                    _options.UserName, _options.Password);
                if (err == 0) { Log(LogLevel.Information, "[Reboot] Controller reboot command sent."); return true; }
                Log(LogLevel.Error, $"[Reboot] SDK error {err}.");
                return false;
            }, ct);
        }
        finally { _sdkLock.Release(); }
    }

    /// <summary>Clears all programs from the controller display.</summary>
    public async Task<bool> ClearScreenAsync(CancellationToken ct = default)
    {
        Log(LogLevel.Information, "[SendImage] ClearScreen requested.");

        if (!IsSdkAvailable("ClearScreen")) return false;

        await _sdkLock.WaitAsync(ct);
        try
        {
            return await ExecuteWithRetryAsync("ClearScreen", ClearScreenCore, ct);
        }
        finally
        {
            _sdkLock.Release();
        }
    }

    // ─── SDK core (only called when IsWindows == true && _sdkInitialized) ────

    private bool SendImageCore(string mediaPath)
    {
        Log(LogLevel.Debug,
            $"[SendImage] [SDK] create_playlist(w={_options.ScreenWidth}, h={_options.ScreenHeight}, type={_options.DeviceType})");

        var tempPath = Path.GetFullPath(_options.TempPath);
        Directory.CreateDirectory(tempPath);

        // Keep SDK image path very short and stable (some firmware builds are
        // sensitive to long/complex temp paths).
        var sdkMediaPath = PrepareSdkMediaPathForNative(mediaPath, tempPath);

        var playlist = create_playlist(_options.ScreenWidth, _options.ScreenHeight, _options.DeviceType);
        if (playlist == IntPtr.Zero)
        {
            Log(LogLevel.Error, "[SendImage] [SDK] create_playlist returned null. Check DeviceType.");
            return false;
        }

        IntPtr program = IntPtr.Zero;
        IntPtr picArea = IntPtr.Zero;

        try
        {
            program = create_program("program_0", "0xff000000");
            if (program == IntPtr.Zero)
            {
                Log(LogLevel.Error, "[SendImage] [SDK] create_program returned null.");
                return false;
            }

            picArea = create_pic();
            if (picArea == IntPtr.Zero)
            {
                Log(LogLevel.Error, "[SendImage] [SDK] create_pic returned null.");
                return false;
            }

            // stay_time=0 (continuous), display_effects=0 (none), display_speed=16 (slowest/smooth)
            Log(LogLevel.Debug, $"[SendImage] [SDK] add_pic_unit(path={sdkMediaPath})...");
            int err = add_pic_unit(picArea, 0, 0, 16, sdkMediaPath);
            Log(LogLevel.Debug, $"[SendImage] [SDK] add_pic_unit → {err}");

            Log(LogLevel.Debug,
                $"[SendImage] [SDK] add_pic(x=0,y=0,w={_options.ScreenWidth},h={_options.ScreenHeight},alpha=100)...");
            err = add_pic(program, picArea, 0, 0, _options.ScreenWidth, _options.ScreenHeight, 100);
            Log(LogLevel.Debug, $"[SendImage] [SDK] add_pic → {err}");

            delete_pic(picArea);
            picArea = IntPtr.Zero;

            // play_mode=1 (loop), play_time=10s placeholder, play_week=127 (Mon–Sun)
            err = add_program_in_playlist(playlist, program, 1, 10, "", "", "", "", 127);
            Log(LogLevel.Debug, $"[SendImage] [SDK] add_program_in_playlist → {err}");

            var ipBytes = Encoding.ASCII.GetBytes(_options.ControllerIp);
            if (!tempPath.EndsWith(Path.DirectorySeparatorChar) && !tempPath.EndsWith(Path.AltDirectorySeparatorChar))
                tempPath += Path.DirectorySeparatorChar;

            long freeSize = 0, totalSize = 0;
            var playlistName = new byte[1024];

            Log(LogLevel.Information,
                $"[SendImage] [SDK] send_program → {_options.ControllerIp}:{_options.ControllerPort} " +
                $"tmpPath={tempPath} media={sdkMediaPath}");

            Log(LogLevel.Debug, "[SendImage] [SDK] send_program call started...");
            err = send_program(
                ipBytes, (ushort)_options.ControllerPort,
                _options.UserName, _options.Password,
                tempPath, playlist, 0,
                ref freeSize, ref totalSize,
                IntPtr.Zero, playlistName,
                is_make: 0, is_save: 1);

            if (err == 0)
            {
                Log(LogLevel.Information,
                    $"[SendImage] [SDK] SUCCESS. FreeSpace={freeSize} bytes, TotalSpace={totalSize} bytes.");
                return true;
            }

            if (err == 0x1e)
            {
                Log(LogLevel.Warning,
                    "[SendImage] [SDK] send_program returned 0x1e. Trying dynamic-area fallback...");

                int fallbackErr;
                if (TrySendViaDynamicFallback(sdkMediaPath, out fallbackErr))
                {
                    _lastSdkSendErrorCode = 0;
                    Log(LogLevel.Information,
                        "[SendImage] [SDK] Dynamic-area fallback succeeded.");
                    return true;
                }

                _lastSdkSendErrorCode = fallbackErr != 0 ? fallbackErr : err;
                Log(LogLevel.Error,
                    $"[SendImage] [SDK] Dynamic fallback failed. Error code: {_lastSdkSendErrorCode}.");
                return false;
            }

            _lastSdkSendErrorCode = err;
            Log(LogLevel.Error, $"[SendImage] [SDK] send_program FAILED. Error code: {err}.");
            return false;
        }
        finally
        {
            // In isolated helper mode process lifetime is short; skipping explicit
            // native cleanup avoids occasional access-violation crashes after failed send.
            if (_isHelperProcess)
            {
                Log(LogLevel.Debug, "[SendImage] [SDK] Helper mode: skipping native delete_* cleanup.");
            }
            else
            {
                if (picArea != IntPtr.Zero) try { delete_pic(picArea); } catch { /* ignore */ }
                if (program != IntPtr.Zero) try { delete_program(program); } catch { /* ignore */ }
                try { delete_playlist(playlist); } catch { /* ignore */ }
            }
        }
    }

    private static string MapSdkError(int err) => err switch
    {
        0x0b => "Invalid authentication",
        0x0c => "Access violation / permission denied",
        0x15 => "User does not exist",
        0x16 => "Wrong password",
        0x17 => "Storage media not found",
        0x18 => "File path error",
        0x1c => "Firmware not found",
        0x1d => "Failed to create user work directory",
        0x1e => "Player command error (often image/program format incompatibility)",
        0x1f => "Failed to obtain Wi-Fi list",
        0x20 => "Wi-Fi connect timeout",
        0x21 => "Hotspot not found",
        0x22 => "Wi-Fi password error",
        0x23 => "Network restarting",
        _ => "Unknown SDK error",
    };

    private bool TrySendViaDynamicFallback(string mediaPath, out int errorCode)
    {
        errorCode = 0;

        IntPtr playlist = IntPtr.Zero;
        IntPtr program = IntPtr.Zero;
        IntPtr dynamicArea = IntPtr.Zero;

        try
        {
            playlist = create_playlist(_options.ScreenWidth, _options.ScreenHeight, _options.DeviceType);
            if (playlist == IntPtr.Zero)
            {
                errorCode = -1;
                return false;
            }

            program = create_program("program_dynamic", "0xff000000");
            if (program == IntPtr.Zero)
            {
                errorCode = -1;
                return false;
            }

            dynamicArea = create_dynamic();
            if (dynamicArea == IntPtr.Zero)
            {
                errorCode = -1;
                return false;
            }

            // dynamic_type=0 (image)
            errorCode = add_dynamic_unit(
                dynamicArea,
                0,   // image
                0,   // display_effects
                16,  // display_speed
                0,   // stay_time
                mediaPath,
                0,   // gif_flag
                "0xff000000",
                12,
                "SimSun",
                "0xffffffff",
                "normal",
                "0",
                "0",
                0, 0, 0,
                "", "", 0);
            if (errorCode != 0) return false;

            errorCode = add_dynamic(
                program, dynamicArea,
                0,
                0, 0,
                _options.ScreenWidth,
                _options.ScreenHeight,
                "-1",
                0,
                "",
                255);
            if (errorCode != 0) return false;

            errorCode = add_program_in_playlist(playlist, program, 1, 10, "", "", "", "", 127);
            if (errorCode != 0) return false;

            var ipBytes = Encoding.ASCII.GetBytes(_options.ControllerIp);
            errorCode = update_dynamic(
                ipBytes,
                (ushort)_options.ControllerPort,
                _options.UserName,
                _options.Password,
                playlist,
                "",
                0,
                0);

            return errorCode == 0;
        }
        finally
        {
            if (!_isHelperProcess)
            {
                if (dynamicArea != IntPtr.Zero) try { delete_dynamic(dynamicArea); } catch { /* ignore */ }
                if (program != IntPtr.Zero) try { delete_program(program); } catch { /* ignore */ }
                if (playlist != IntPtr.Zero) try { delete_playlist(playlist); } catch { /* ignore */ }
            }
        }
    }

    private static string PrepareSdkMediaPathForNative(string sourcePath, string tempPath)
    {
        var ext = Path.GetExtension(sourcePath);
        if (string.IsNullOrWhiteSpace(ext))
            ext = ".bin";

        var targetPath = Path.Combine(tempPath, $"sdk_input{ext.ToLowerInvariant()}");
        var sourceFull = Path.GetFullPath(sourcePath);
        var targetFull = Path.GetFullPath(targetPath);

        if (!string.Equals(sourceFull, targetFull, StringComparison.OrdinalIgnoreCase))
            File.Copy(sourceFull, targetFull, overwrite: true);

        return targetFull;
    }

    private bool ClearScreenCore()
    {
        var ipBytes = Encoding.ASCII.GetBytes(_options.ControllerIp);
        Log(LogLevel.Information,
            $"[SendImage] [SDK] clear_all_program → {_options.ControllerIp}:{_options.ControllerPort}");

        int err = clear_all_program(ipBytes, (ushort)_options.ControllerPort,
            _options.UserName, _options.Password);

        if (err == 0)
        {
            Log(LogLevel.Information, "[SendImage] [SDK] Screen cleared successfully.");
            return true;
        }

        Log(LogLevel.Error, $"[SendImage] [SDK] clear_all_program FAILED. Error code: {err}.");
        return false;
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    // kernel32 helpers for native DLL search path manipulation (Windows only)
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool SetDllDirectory(string lpPathName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr AddDllDirectory(string newDirectory);

    private void LogNativeDependencyProbe(string baseDir)
    {
        var deps = new[]
        {
            "YQNetCom.dll",
            "libcurl.dll",
            "libcrypto-1_1-x64.dll",
            "libssl-1_1-x64.dll",
            "MSVCR100.dll",
            "MSVCP100.dll",
            "VCRUNTIME140.dll",
        };

        foreach (var dep in deps)
        {
            var localPath = Path.Combine(baseDir, dep);
            var localExists = File.Exists(localPath);

            bool globalLoadable;
            try
            {
                globalLoadable = NativeLibrary.TryLoad(dep, out var handle);
                if (globalLoadable) NativeLibrary.Free(handle);
            }
            catch
            {
                globalLoadable = false;
            }

            Log(LogLevel.Information,
                $"[Onbon] Native dep probe: {dep} localExists={localExists} globalLoadable={globalLoadable}");
        }
    }

    private void InitializeSdk()
    {
        lock (_sdkInitGate)
        {
            // Native SDK is process-global — initialize it once and reuse it across
            // host restarts. Re-running init_sdk()/SetDllImportResolver() in the same
            // process throws or crashes (see _sdkInitGate comment above).
            if (_sdkInitializedProcessWide)
            {
                _sdkInitialized = true;
                Log(LogLevel.Information,
                    "[Onbon] SDK already initialized in this process — reusing it (host restart). Skipping re-init.");
                return;
            }

            InitializeSdkCore();
        }
    }

    private void InitializeSdkCore()
    {
        var baseDir = AppContext.BaseDirectory;
        var processDir = Path.GetDirectoryName(Environment.ProcessPath ?? string.Empty) ?? string.Empty;

        Log(LogLevel.Information,
            $"[Onbon] Paths: BaseDir={baseDir} | ProcessDir={processDir}");

        // Tell Windows to look for native DLL dependencies in the exe folder
        // (helps when YQNetCom.dll needs VC++ runtime DLLs from the same folder).
        if (OperatingSystem.IsWindows())
        {
            SetDllDirectory(baseDir);
            if (!string.IsNullOrEmpty(processDir) && processDir != baseDir)
                AddDllDirectory(processDir);

            // Diagnostic snapshot helps pinpoint missing VC++ runtimes.
            LogNativeDependencyProbe(baseDir);
        }

        // Single-file apps extract managed code to %TEMP%, so the Windows DLL
        // loader's "app directory" is that temp folder — NOT the exe folder.
        // Register a resolver so P/Invoke explicitly loads from known paths.
        // SetDllImportResolver throws if called twice for the same assembly, so
        // guard it for the case where a previous init attempt registered it but
        // init_sdk() itself failed and a later host build retries.
        if (!_sdkResolverRegistered)
        {
            NativeLibrary.SetDllImportResolver(typeof(OnbonLedController).Assembly,
                (libraryName, assembly, searchPath) =>
            {
                if (libraryName is "YQNetCom.dll" or "YQNetCom")
                {
                    // Search candidates in priority order
                    var candidates = new[]
                    {
                        Path.Combine(baseDir, "YQNetCom.dll"),
                        Path.Combine(processDir, "YQNetCom.dll"),
                    };

                    foreach (var candidate in candidates
                        .Where(p => !string.IsNullOrEmpty(p))
                        .Select(Path.GetFullPath)
                        .Distinct())
                    {
                        var exists = File.Exists(candidate);
                        Log(LogLevel.Information,
                            $"[Onbon] DllImportResolver: {candidate} — exists={exists}");

                        if (!exists) continue;

                        try
                        {
                            var handle = NativeLibrary.Load(candidate);
                            Log(LogLevel.Information,
                                $"[Onbon] DllImportResolver: loaded OK from {candidate}");
                            return handle;
                        }
                        catch (Exception ex)
                        {
                            Log(LogLevel.Error,
                                $"[Onbon] DllImportResolver: file exists but Load() failed: {ex.Message}. " +
                                "Possible cause: missing VC++ Redistributable or wrong CPU arch.");
                        }
                    }

                    Log(LogLevel.Error,
                        $"[Onbon] DllImportResolver: YQNetCom.dll not found. " +
                        $"Place YQNetCom.dll in: {baseDir}");
                }
                return IntPtr.Zero;
            });
            _sdkResolverRegistered = true;
        }

        try
        {
            Log(LogLevel.Information, "[Onbon] Calling init_sdk()...");
            int result = init_sdk();

            if (result == 0)
            {
                _sdkInitialized = true;
                _sdkInitializedProcessWide = true;
                if (!_isHelperProcess) HookProcessExitRelease();
                Log(LogLevel.Information, "[Onbon] init_sdk() succeeded. SDK is ready.");
            }
            else
            {
                Log(LogLevel.Error, $"[Onbon] init_sdk() returned error code {result}.");
            }
        }
        catch (DllNotFoundException ex)
        {
            Log(LogLevel.Error,
                $"[Onbon] YQNetCom.dll not found: {ex.Message}. " +
                "Place YQNetCom.dll and SDK native DLLs next to the executable. " +
                "If files are present but loading still fails, install Microsoft Visual C++ Redistributables: 2010 x64 and 2015-2022 x64.");
        }
        catch (BadImageFormatException ex)
        {
            Log(LogLevel.Error,
                $"[Onbon] YQNetCom.dll CPU architecture mismatch: {ex.Message}. " +
                "The process bitness (x86/x64) must match the DLL.");
        }
        catch (Exception ex)
        {
            Log(LogLevel.Error, $"[Onbon] Unexpected init error: {ex}");
        }
    }

    /// <summary>
    /// Registers a one-time process-exit handler that releases the native SDK.
    /// Because the SDK is initialized once per process (and reused across host
    /// restarts), it must also be released exactly once — at process shutdown,
    /// not on every host dispose.
    /// </summary>
    private void HookProcessExitRelease()
    {
        if (_processExitReleaseHooked) return;
        _processExitReleaseHooked = true;
        AppDomain.CurrentDomain.ProcessExit += (_, _) => ReleaseSdkProcessWide();
    }

    private void ReleaseSdkProcessWide()
    {
        lock (_sdkInitGate)
        {
            if (!_sdkInitializedProcessWide || _sdkReleasedProcessWide) return;
            if (!OperatingSystem.IsWindows()) return;

            try
            {
                release_sdk();
                _logger.LogInformation("[Onbon] SDK released cleanly on process exit.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Onbon] Error during SDK release.");
            }
            finally
            {
                _sdkReleasedProcessWide = true;
            }
        }
    }

    /// <summary>Returns true only if the SDK is available and ready for calls.</summary>
    private bool IsSdkAvailable(string context)
    {
        if (!_options.Enabled)
        {
            Log(LogLevel.Debug, $"[{context}] SDK disabled (OnbonLed:Enabled=false). Skipping.");
            return false;
        }

        if (!OperatingSystem.IsWindows())
        {
            Log(LogLevel.Warning, $"[{context}] Non-Windows OS. SDK call skipped.");
            return false;
        }

        if (!_sdkInitialized)
        {
            Log(LogLevel.Error, $"[{context}] SDK not initialized. Cannot perform operation.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Executes <paramref name="action"/> in a thread-pool thread.
    /// Retries up to <c>OnbonLed:RetryCount + 1</c> times with <c>RetryDelayMs</c> between attempts.
    /// </summary>
    private async Task<bool> ExecuteWithRetryAsync(string operation, Func<bool> action, CancellationToken ct)
    {
        int maxAttempts = Math.Max(1, _options.RetryCount + 1);

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                if (attempt > 1)
                    Log(LogLevel.Information, $"[{operation}] Retry attempt {attempt}/{maxAttempts}...");

                // Run blocking P/Invoke on thread-pool to keep ASP.NET thread free
                bool ok = await Task.Run(action, ct);

                if (ok) return true;

                if (attempt < maxAttempts)
                {
                    Log(LogLevel.Warning,
                        $"[{operation}] Attempt {attempt} returned false. " +
                        $"Waiting {_options.RetryDelayMs} ms before retry...");
                    await Task.Delay(_options.RetryDelayMs, ct);
                }
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                Log(LogLevel.Warning,
                    $"[{operation}] Attempt {attempt} threw {ex.GetType().Name}: {ex.Message}");

                if (attempt < maxAttempts)
                    await Task.Delay(_options.RetryDelayMs, ct);
            }
        }

        Log(LogLevel.Error, $"[{operation}] All {maxAttempts} attempt(s) exhausted. Operation failed.");
        return false;
    }

    private async Task<string> ConvertToBmpAsync(string imagePath, CancellationToken ct)
    {
        var tempRoot = Path.IsPathRooted(_options.TempPath)
            ? _options.TempPath
            : Path.Combine(AppContext.BaseDirectory, _options.TempPath);
        Directory.CreateDirectory(tempRoot);

        var tmp = Path.Combine(
            tempRoot,
            $"onbon_{Path.GetFileNameWithoutExtension(imagePath)}_{Guid.NewGuid():N}.bmp");

        Log(LogLevel.Debug,
            $"[SendImage] Converting {Path.GetFileName(imagePath)} -> 24bpp BMP {tmp} " +
            $"with size {_options.ScreenWidth}x{_options.ScreenHeight}");

        // SDK is sensitive to BMP flavor. On Windows use GDI+ encoder, which
        // produces classic 24bpp BMP that Onbon firmware handles more reliably.
        if (OperatingSystem.IsWindows())
        {
            await Task.Run(() => ConvertToBmpWithGdi(imagePath, tmp), ct);

            return tmp;
        }

        // Non-Windows fallback for development environments.
        var converted = await ConvertToBmpWithImageSharpAsync(imagePath, ct);
        if (!string.Equals(converted, tmp, StringComparison.OrdinalIgnoreCase))
            File.Copy(converted, tmp, overwrite: true);

        return tmp;
    }

    private async Task<string> ConvertToBmpWithImageSharpAsync(string imagePath, CancellationToken ct, string? suffix = null)
    {
        var tempRoot = Path.IsPathRooted(_options.TempPath)
            ? _options.TempPath
            : Path.Combine(AppContext.BaseDirectory, _options.TempPath);
        Directory.CreateDirectory(tempRoot);

        var tmp = Path.Combine(
            tempRoot,
            $"onbon_{Path.GetFileNameWithoutExtension(imagePath)}_{suffix ?? "is"}_{Guid.NewGuid():N}.bmp");

        using var img = await Image.LoadAsync<Rgb24>(imagePath, ct);
        if (img.Width != _options.ScreenWidth || img.Height != _options.ScreenHeight)
        {
            img.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(_options.ScreenWidth, _options.ScreenHeight),
                Mode = ResizeMode.Stretch,
                Sampler = KnownResamplers.Bicubic,
            }));
        }

        await using (var fs = File.Create(tmp))
        {
            await img.SaveAsync(fs, new BmpEncoder
            {
                BitsPerPixel = BmpBitsPerPixel.Pixel24,
            }, ct);
        }

        return tmp;
    }

    private async Task<string> ConvertToJpegAsync(string imagePath, CancellationToken ct)
    {
        var tempRoot = Path.IsPathRooted(_options.TempPath)
            ? _options.TempPath
            : Path.Combine(AppContext.BaseDirectory, _options.TempPath);
        Directory.CreateDirectory(tempRoot);

        var tmp = Path.Combine(
            tempRoot,
            $"onbon_{Path.GetFileNameWithoutExtension(imagePath)}_jpg_{Guid.NewGuid():N}.jpg");

        using var img = await Image.LoadAsync<Rgb24>(imagePath, ct);
        if (img.Width != _options.ScreenWidth || img.Height != _options.ScreenHeight)
        {
            img.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(_options.ScreenWidth, _options.ScreenHeight),
                Mode = ResizeMode.Stretch,
                Sampler = KnownResamplers.Bicubic,
            }));
        }

        await using (var fs = File.Create(tmp))
        {
            await img.SaveAsync(fs, new JpegEncoder
            {
                Quality = 92,
            }, ct);
        }

        return tmp;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private void ConvertToBmpWithGdi(string sourcePath, string targetPath)
    {
        using var src = new GdiBitmap(sourcePath);
        using var dst = new GdiBitmap(
            _options.ScreenWidth,
            _options.ScreenHeight,
            GdiPixelFormat.Format24bppRgb);

        using var g = GdiGraphics.FromImage(dst);
        g.InterpolationMode = GdiInterpolationMode.HighQualityBicubic;
        g.SmoothingMode = GdiSmoothingMode.HighQuality;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        g.DrawImage(src, 0, 0, _options.ScreenWidth, _options.ScreenHeight);

        dst.Save(targetPath, GdiImageFormat.Bmp);
    }

    private void Log(LogLevel level, string message)
    {
        _logger.Log(level, "{Message}", message);
        _logStore.Add(level, nameof(OnbonLedController), message);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _sdkLock.Dispose();

        // Do NOT call release_sdk() here. The native SDK is process-global and is
        // reused across host restarts (the tray app rebuilds this singleton on
        // "Restart"). Releasing it on dispose would tear down the SDK that the
        // next host needs, and re-initializing it later crashes the process.
        // Release happens once, on actual process exit (see HookProcessExitRelease).
        if (_isHelperProcess && _sdkInitialized && OperatingSystem.IsWindows())
        {
            // Short-lived helper process: let the OS reclaim native state on exit.
            _logger.LogDebug("[Onbon] Helper mode: skipping release_sdk() on dispose.");
        }
    }
}

public enum LedSendErrorType
{
    None,
    FileNotFound,
    InvalidImageSize,
    SdkNotInitialized,
    NativeInteropError,
    SdkSendFailed,
    IsolatedProcessCrashed,
    Cancelled,
    UnexpectedError,
}

public sealed record LedSendStatus(
    bool Success,
    LedSendErrorType ErrorType,
    string Message,
    bool DuplicateSkipped)
{
    public static LedSendStatus Ok(string message, bool duplicateSkipped = false) =>
        new(true, LedSendErrorType.None, message, duplicateSkipped);

    public static LedSendStatus Fail(LedSendErrorType errorType, string message) =>
        new(false, errorType, message, false);
}

/// <summary>Result of a TCP connection check.</summary>
public sealed record ConnectionCheckResult(bool IsOnline, string Details);

/// <summary>Firmware version information from the LED controller.</summary>
public sealed record FirmwareInfo(
    string FirmwareVersion,
    string AppVersion,
    string FpgaVersion);

/// <summary>Live operational status of the LED screen.</summary>
public sealed record ScreenStatusInfo(
    bool IsPoweredOn,
    int Brightness,
    bool BrightnessAuto,
    int Volume,
    bool ScreenLocked,
    bool ProgramLocked,
    string ControllerTime);

/// <summary>Static hardware / configuration info read from the controller.</summary>
public sealed record ControllerHardwareInfo(
    string Barcode,
    string ScreenIp,
    int Port,
    int ScreenWidth,
    int ScreenHeight,
    ushort ScreenType,
    int Brightness,
    string StorageMedia,
    string MacAddress);
