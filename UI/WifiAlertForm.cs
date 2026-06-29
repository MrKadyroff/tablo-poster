using System.Media;

namespace LedImageUpdaterService.UI;

/// <summary>
/// Compact, always-on-top notice shown in the bottom-right corner after a failed push to the
/// board when the PC is not on the board's Wi-Fi network. Unlike before, the operator CAN
/// close it; the tray watchdog (<see cref="WifiWatchdog"/>) re-shows it ~2 minutes later if the
/// problem is still not resolved. The window also re-checks the connection on its own and
/// closes automatically once the PC rejoins the correct network.
///
/// This window is owned by the tray app (not the cashier window), so it appears even when the
/// cashier window is closed.
/// </summary>
internal sealed class WifiAlertForm : Form
{
    private readonly string _expectedSsid;
    private readonly System.Windows.Forms.Timer _autoTimer = new() { Interval = 4000 };
    private bool _connected;
    private bool _checking;

    /// <summary>
    /// True when the window closed because the PC rejoined the correct network. The watchdog
    /// uses this to decide whether to schedule a re-show (it does not, when resolved).
    /// </summary>
    internal bool ResolvedConnected => _connected;

    private static readonly Color WarnColor = Color.FromArgb(235, 110, 90);

    public WifiAlertForm(string expectedSsid, string? currentSsid)
    {
        _expectedSsid = expectedSsid;
        InitializeComponent();

        _autoTimer.Tick += async (_, _) => await RecheckAsync();
        Load += (_, _) =>
        {
            PositionBottomRight();
            _autoTimer.Start();
            try { SystemSounds.Exclamation.Play(); } catch { }
        };
        FormClosing += (_, _) => _autoTimer.Stop();
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        Text = "Нет связи с табло";
        Size = new Size(330, 144);
        FormBorderStyle = FormBorderStyle.FixedToolWindow; // small title bar, has a close button
        StartPosition = FormStartPosition.Manual;
        ShowInTaskbar = false;
        TopMost = true;
        BackColor = UITheme.Bg;
        ForeColor = UITheme.Text;
        Font = new Font("Segoe UI", 9.5f);
        try { Icon = TrayApplicationContext.CreateAppIcon(); } catch { }

        var title = new Label
        {
            Text = "Курсы на табло не обновляются",
            Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold),
            ForeColor = WarnColor,
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 42,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(14, 0, 10, 0),
        };

        var body = new Label
        {
            Text = "Подключитесь к сети Wi-Fi:",
            ForeColor = UITheme.Text,
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 26,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(14, 0, 10, 0),
        };

        var ssid = new Label
        {
            Text = _expectedSsid,
            Font = new Font("Segoe UI Semibold", 13f, FontStyle.Bold),
            ForeColor = UITheme.Accent,
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 34,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(14, 0, 10, 0),
        };

        // Dock(Top) stacks last-added on top → title, then body, then ssid.
        Controls.Add(ssid);
        Controls.Add(body);
        Controls.Add(title);

        ResumeLayout();
    }

    private void PositionBottomRight()
    {
        var wa = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1280, 720);
        Location = new Point(wa.Right - Width - 12, wa.Bottom - Height - 12);
    }

    /// <summary>
    /// Closes the alert programmatically (used by the watchdog when the board becomes
    /// reachable again, so the notice never lingers).
    /// </summary>
    public void ForceClose()
    {
        _connected = true; // marks the close as "resolved" → no re-show
        _autoTimer.Stop();
        try { Close(); } catch { /* ignore if already disposing */ }
    }

    private async Task RecheckAsync()
    {
        if (_checking) return;
        _checking = true;
        try
        {
            var current = await WifiInfo.GetConnectedSsidAsync();
            if (!WifiInfo.IsWrongNetwork(_expectedSsid, current))
            {
                _connected = true; // resolved → close and do not re-show
                Close();
            }
        }
        finally
        {
            _checking = false;
        }
    }
}
