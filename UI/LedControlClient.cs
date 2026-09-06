using System.Net.Http;
using System.Net.Http.Json;

namespace LedImageUpdaterService.UI;

/// <summary>
/// Thin client for the in-process REST API used by the settings UI to issue
/// operational commands to the LED board (power on/off, reboot).
/// </summary>
internal static class LedControlClient
{
    private static string BaseUrl(string? urls)
    {
        var url = (urls ?? "").Split(';').FirstOrDefault(u => !string.IsNullOrWhiteSpace(u))?.Trim();
        if (string.IsNullOrWhiteSpace(url)) url = "http://localhost:5050";
        return url.Replace("0.0.0.0", "localhost").Replace("+", "localhost").TrimEnd('/');
    }

    /// <summary>
    /// Aggregated health used by the cashier window banner. Distinguishes three failure
    /// modes: the local service/host is down, the controller (Wi-Fi) is unreachable, or
    /// the last board send failed.
    /// </summary>
    public static async Task<BoardHealth> GetBoardHealthAsync(string? urls)
    {
        var baseUrl = BaseUrl(urls);

        // 1) Is the in-process service responding, and is the controller reachable?
        bool serviceUp;
        bool controllerOnline = false;
        string details = "";
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
            var resp = await http.GetAsync($"{baseUrl}/api/led/connection");
            var text = await resp.Content.ReadAsStringAsync();
            serviceUp = true;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(text);
                var root = doc.RootElement;
                controllerOnline = root.TryGetProperty("isOnline", out var p) && p.GetBoolean();
                details = root.TryGetProperty("details", out var d) ? d.GetString() ?? "" : text;
            }
            catch { details = text; }
        }
        catch (Exception ex)
        {
            // The HTTP call itself failed → the local service/host is not responding.
            return new BoardHealth(ServiceUp: false, ControllerOnline: false,
                LastSuccessAt: null, LastFailureAt: null, LastFailureReason: null,
                Details: $"Сервис не отвечает: {ex.Message}");
        }

        // 2) Last auto-send result (best-effort; ignore errors).
        DateTimeOffset? lastSuccess = null, lastFailure = null;
        string? lastFailureReason = null;
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
            var text = await http.GetStringAsync($"{baseUrl}/api/led/timer-status");
            using var doc = System.Text.Json.JsonDocument.Parse(text);
            var root = doc.RootElement;
            lastSuccess = ReadDate(root, "lastSuccessAt");
            lastFailure = ReadDate(root, "lastFailureAt");
            lastFailureReason = ReadString(root, "lastFailureDetails")
                                ?? ReadString(root, "lastFailureErrorType");
        }
        catch { /* timer status is optional */ }

        return new BoardHealth(serviceUp, controllerOnline, lastSuccess, lastFailure, lastFailureReason, details);
    }

    private static DateTimeOffset? ReadDate(System.Text.Json.JsonElement root, string name)
    {
        foreach (var prop in root.EnumerateObject())
        {
            if (!string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase)) continue;
            if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.String
                && prop.Value.TryGetDateTimeOffset(out var dto))
                return dto;
            return null;
        }
        return null;
    }

    private static string? ReadString(System.Text.Json.JsonElement root, string name)
    {
        foreach (var prop in root.EnumerateObject())
        {
            if (!string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase)) continue;
            return prop.Value.ValueKind == System.Text.Json.JsonValueKind.String
                ? prop.Value.GetString()
                : null;
        }
        return null;
    }

    public static async Task<(bool isOnline, string details)> CheckConnectionAsync(string? urls)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var resp = await http.GetAsync($"{BaseUrl(urls)}/api/led/connection");
            var text = await resp.Content.ReadAsStringAsync();
            // Parse {"isOnline":true,"details":"..."}
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(text);
                var root = doc.RootElement;
                var online = root.TryGetProperty("isOnline", out var p) && p.GetBoolean();
                var details = root.TryGetProperty("details", out var d) ? d.GetString() ?? "" : text;
                return (online, details);
            }
            catch
            {
                return (resp.IsSuccessStatusCode, text);
            }
        }
        catch (Exception ex)
        {
            return (false, $"Сервис недоступен: {ex.Message}");
        }
    }

    /// <summary>
    /// Forces a board-link re-scan (<c>POST /api/led/board-link/recheck</c>) and returns the
    /// controller IP the service found on the table's Wi-Fi and the human-readable
    /// recommendation. <paramref name="ip"/> is the discovered/applied controller IP
    /// (null if nothing was found).
    /// </summary>
    public static async Task<(bool ok, string? ip, string message)> DetectControllerIpAsync(string? urls)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            var resp = await http.PostAsync($"{BaseUrl(urls)}/api/led/board-link/recheck", content: null);
            var text = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                return (false, null, string.IsNullOrWhiteSpace(text) ? resp.StatusCode.ToString() : text);

            using var doc = System.Text.Json.JsonDocument.Parse(text);
            var root = doc.RootElement;

            string? Str(string name) =>
                root.TryGetProperty(name, out var p) && p.ValueKind == System.Text.Json.JsonValueKind.String
                    ? p.GetString() : null;

            // Prefer the applied IP (already active at runtime), else the candidate the scan found.
            var ip = Str("appliedControllerIp") ?? Str("candidateControllerIp");
            var message = Str("recommendation") ?? text;
            bool tcpOk = root.TryGetProperty("tcpOk", out var t) && t.ValueKind == System.Text.Json.JsonValueKind.True;

            // "ok" even without a fresh IP if the configured one already matches and is reachable.
            return (ip is not null || tcpOk, ip, message);
        }
        catch (Exception ex)
        {
            return (false, null, $"Сервис недоступен: {ex.Message}");
        }
    }

    public static Task<(bool ok, string message)> SetPowerAsync(string? urls, bool on)
        => PostAsync(urls, "api/led/power", new { on });

    public static Task<(bool ok, string message)> RebootAsync(string? urls)
        => PostAsync(urls, "api/led/reboot", new { });

    /// <summary>
    /// Manually pushes the latest rendered image to the LED board (no auto-send timer needed).
    /// Used by the "Отправить на табло" button on the Design tab for points without permanent internet.
    /// </summary>
    public static Task<(bool ok, string message)> SendToBoardAsync(string? urls)
        => PostAsync(urls, "api/led/update", new { });

    private static async Task<(bool ok, string message)> PostAsync(string? urls, string path, object body)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(12) };
            var resp = await http.PostAsJsonAsync($"{BaseUrl(urls)}/{path}", body);
            var text = await resp.Content.ReadAsStringAsync();
            return (resp.IsSuccessStatusCode, string.IsNullOrWhiteSpace(text) ? resp.StatusCode.ToString() : text);
        }
        catch (Exception ex)
        {
            return (false, $"Сервис недоступен: {ex.Message}");
        }
    }
}

/// <summary>Aggregated board health for the cashier window banner.</summary>
internal sealed record BoardHealth(
    bool ServiceUp,
    bool ControllerOnline,
    DateTimeOffset? LastSuccessAt,
    DateTimeOffset? LastFailureAt,
    string? LastFailureReason,
    string Details);
