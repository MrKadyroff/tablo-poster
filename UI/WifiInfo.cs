using System.Diagnostics;

namespace LedImageUpdaterService.UI;

/// <summary>
/// Reads the currently connected Wi-Fi network name (SSID) on Windows by parsing
/// <c>netsh wlan show interfaces</c>. Returns null when no Wi-Fi is connected, when
/// there is no wireless adapter, or on any error — callers treat null as "not connected".
/// </summary>
internal static class WifiInfo
{
    /// <summary>Current connected SSID, or null if not connected / unavailable.</summary>
    public static Task<string?> GetConnectedSsidAsync() => Task.Run(GetConnectedSsid);

    public static string? GetConnectedSsid()
    {
        if (!OperatingSystem.IsWindows()) return null;

        try
        {
            var psi = new ProcessStartInfo("netsh", "wlan show interfaces")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            using var proc = Process.Start(psi);
            if (proc is null) return null;

            string output = proc.StandardOutput.ReadToEnd();
            if (!proc.WaitForExit(3000))
            {
                try { proc.Kill(true); } catch { /* ignore */ }
                return null;
            }

            return ParseSsid(output);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Extracts the connected SSID from <c>netsh</c> output. The "SSID" label is the same
    /// (an acronym) on localized Windows, so we match the key part before ':' and exclude
    /// "BSSID". Returns the first SSID found.
    /// </summary>
    internal static string? ParseSsid(string netshOutput)
    {
        foreach (var rawLine in netshOutput.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');
            int idx = line.IndexOf(':');
            if (idx <= 0) continue;

            var key = line[..idx].Trim();
            if (!key.Equals("SSID", StringComparison.OrdinalIgnoreCase)) continue; // skips "BSSID"

            var value = line[(idx + 1)..].Trim();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
        return null;
    }

    /// <summary>True when an SSID is configured and the PC is NOT connected to it.</summary>
    public static bool IsWrongNetwork(string? expectedSsid, string? currentSsid)
    {
        if (string.IsNullOrWhiteSpace(expectedSsid)) return false; // no check configured
        return string.IsNullOrWhiteSpace(currentSsid)
            || !currentSsid.Trim().Equals(expectedSsid.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Tries to (re)connect Windows to the given Wi-Fi network via
    /// <c>netsh wlan connect</c>, up to <paramref name="maxAttempts"/> times. A saved
    /// Wi-Fi profile for the SSID must already exist in Windows (profile name is assumed
    /// equal to the SSID). After each attempt it waits, then re-reads the connected SSID
    /// and returns true as soon as the PC is on the expected network. The full set of
    /// attempts runs per call, so a new failure episode always starts the count fresh.
    /// </summary>
    public static async Task<bool> TryReconnectAsync(
        string ssid, int maxAttempts, CancellationToken ct = default)
    {
        if (!OperatingSystem.IsWindows() || string.IsNullOrWhiteSpace(ssid)) return false;

        for (int attempt = 1; attempt <= maxAttempts && !ct.IsCancellationRequested; attempt++)
        {
            RunNetshConnect(ssid);

            // Give Windows a few seconds to associate before re-checking.
            try { await Task.Delay(TimeSpan.FromSeconds(4), ct); }
            catch (OperationCanceledException) { return false; }

            var current = await GetConnectedSsidAsync();
            if (!IsWrongNetwork(ssid, current))
                return true; // connected → caller resets its counter
        }

        return false;
    }

    private static void RunNetshConnect(string ssid)
    {
        try
        {
            // name = saved profile name (assumed == SSID); ssid pins the target network.
            var psi = new ProcessStartInfo("netsh", $"wlan connect name=\"{ssid}\" ssid=\"{ssid}\"")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            using var proc = Process.Start(psi);
            if (proc is null) return;

            proc.StandardOutput.ReadToEnd();
            proc.StandardError.ReadToEnd();
            if (!proc.WaitForExit(5000))
            {
                try { proc.Kill(true); } catch { /* ignore */ }
            }
        }
        catch
        {
            // Reconnect is best-effort; failures fall through to the alert.
        }
    }
}
