using LedImageUpdaterService.Models;
using Microsoft.Extensions.Options;

namespace LedImageUpdaterService.Services;

/// <summary>
/// Background service that periodically checks the watch folder for a new image
/// and sends it to the Onbon LED controller via <see cref="OnbonLedController"/>.
///
/// This service complements the existing FTP-based <see cref="Worker"/> — use it
/// when you want direct SDK delivery alongside (or instead of) FTP publishing.
///
/// Configuration:
///   LedUpdater:WatchFolder  — folder to watch for latest image
///   OnbonLed:PollSeconds    — how often to check (default 30 s)
///   OnbonLed:Enabled        — set false to disable on macOS
/// </summary>
public sealed class LedBoardService : BackgroundService
{
    private readonly ILogger<LedBoardService> _logger;
    private readonly OnbonLedController _controller;
    private readonly ServiceOptions _serviceOptions;
    private readonly OnbonOptions _onbonOptions;
    private readonly InMemoryLogStore _logStore;

    private string? _lastSentFilePath;
    private DateTime _lastSentWriteUtc;

    public LedBoardService(
        ILogger<LedBoardService> logger,
        OnbonLedController controller,
        IOptions<ServiceOptions> serviceOptions,
        IOptions<OnbonOptions> onbonOptions,
        InMemoryLogStore logStore)
    {
        _logger = logger;
        _controller = controller;
        _serviceOptions = serviceOptions.Value;
        _onbonOptions = onbonOptions.Value;
        _logStore = logStore;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_serviceOptions.LayoutTestMode)
        {
            Log(LogLevel.Information,
                "[LedBoardService] LayoutTestMode=true — SDK auto-send disabled for safe layout testing.");
            return;
        }

        if (!_onbonOptions.AutoSend)
        {
            Log(LogLevel.Information,
                "[LedBoardService] AutoSend=false — background polling disabled. " +
                "Use Swagger API endpoints to send images manually.");
            return;
        }

        Log(LogLevel.Information,
            $"[LedBoardService] Started. WatchFolder={_serviceOptions.WatchFolder} " +
            $"PollInterval={_onbonOptions.PollSeconds}s");

        // Give other services time to fully start before the first send attempt
        await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollAndSendAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, $"[LedBoardService] Unhandled error in poll loop: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromSeconds(_onbonOptions.PollSeconds), stoppingToken);
        }

        Log(LogLevel.Information, "[LedBoardService] Stopped.");
    }

    // ─── Core poll logic ─────────────────────────────────────────────────────

    private async Task PollAndSendAsync(CancellationToken ct)
    {
        var watchDir = _serviceOptions.WatchFolder;

        if (!Directory.Exists(watchDir))
        {
            Log(LogLevel.Warning, $"[LedBoardService] WatchFolder does not exist: {watchDir}");
            return;
        }

        var newest = GetNewestImage(watchDir);

        if (newest is null)
        {
            Log(LogLevel.Debug, $"[LedBoardService] No images found in {watchDir}");
            return;
        }

        // Skip if the file has not changed since the last successful send
        if (_serviceOptions.SkipIfUnchanged
            && newest.FullName == _lastSentFilePath
            && newest.LastWriteTimeUtc == _lastSentWriteUtc)
        {
            Log(LogLevel.Debug, $"[LedBoardService] Image unchanged, skipping: {newest.Name}");
            return;
        }

        Log(LogLevel.Information, $"[LedBoardService] New image detected: {newest.FullName}");

        // Use the same send pipeline as /api/led/upload for identical behavior.
        var status = await _controller.SendImageWithStatusAsync(newest.FullName, ct);

        if (status.Success)
        {
            _lastSentFilePath = newest.FullName;
            _lastSentWriteUtc = newest.LastWriteTimeUtc;
            if (status.DuplicateSkipped)
            {
                Log(LogLevel.Information,
                    $"[LedBoardService] Duplicate skipped: {newest.Name}. Details: {status.Message}");
            }
            else
            {
                Log(LogLevel.Information, $"[LedBoardService] Image sent successfully: {newest.Name}");
            }
        }
        else
        {
            Log(LogLevel.Warning,
                $"[LedBoardService] Failed to send image: {newest.Name}. " +
                $"ErrorType={status.ErrorType}; Details={status.Message}");
        }
    }

    /// <summary>Sends a specific image immediately, bypassing the changed-file check.</summary>
    public Task<bool> ForceUpdateAsync(string? imagePath = null, CancellationToken ct = default)
    {
        if (imagePath is not null)
        {
            Log(LogLevel.Information, $"[LedBoardService] ForceUpdate requested: {imagePath}");
            return SendForceViaUploadPipelineAsync(imagePath, ct);
        }

        var newest = GetNewestImage(_serviceOptions.WatchFolder);
        if (newest is null)
        {
            Log(LogLevel.Warning, "[LedBoardService] ForceUpdate: no images found in WatchFolder.");
            return Task.FromResult(false);
        }

        Log(LogLevel.Information, $"[LedBoardService] ForceUpdate: sending latest: {newest.FullName}");
        return SendForceViaUploadPipelineAsync(newest.FullName, ct);
    }

    private async Task<bool> SendForceViaUploadPipelineAsync(string imagePath, CancellationToken ct)
    {
        var status = await _controller.SendImageWithStatusAsync(imagePath, ct);
        if (!status.Success)
        {
            Log(LogLevel.Warning,
                $"[LedBoardService] ForceUpdate failed. ErrorType={status.ErrorType}; Details={status.Message}");
        }

        return status.Success;
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static FileInfo? GetNewestImage(string folder)
    {
        if (!Directory.Exists(folder)) return null;

        return new DirectoryInfo(folder)
            .EnumerateFiles("*", SearchOption.TopDirectoryOnly)
            .Where(f => f.Extension.ToLowerInvariant() is ".bmp" or ".png" or ".jpg" or ".jpeg")
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .FirstOrDefault();
    }

    private void Log(LogLevel level, string message)
    {
        _logger.Log(level, "{Message}", message);
        _logStore.Add(level, nameof(LedBoardService), message);
    }
}
