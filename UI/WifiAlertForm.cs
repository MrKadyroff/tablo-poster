using System.Media;

namespace LedImageUpdaterService.UI;

/// <summary>
/// Loud, standalone, always-on-top red alert shown when the PC is not connected to the
/// board's Wi-Fi network. It is intentionally hard to dismiss: there is no close button
/// and it cannot be closed until the PC joins the correct network. It also re-checks the
/// connection on its own every few seconds and closes automatically once connected — the
/// "Проверить снова" button just forces an immediate check.
///
/// This window is owned by the tray app (not the cashier window), so it appears even when
/// the cashier window is closed.
/// </summary>
internal sealed class WifiAlertForm : Form
{
    private readonly string _expectedSsid;
    private Label _lblCurrent = null!;
    private readonly System.Windows.Forms.Timer _autoTimer = new() { Interval = 4000 };
    private bool _connected;
    private bool _checking;

    private static readonly Color AlertRed = Color.FromArgb(176, 0, 32);

    public WifiAlertForm(string expectedSsid, string? currentSsid)
    {
        _expectedSsid = expectedSsid;
        InitializeComponent(currentSsid);

        _autoTimer.Tick += async (_, _) => await RecheckAsync();
        Load += (_, _) =>
        {
            _autoTimer.Start();
            try { SystemSounds.Exclamation.Play(); } catch { }
        };
        FormClosing += (_, e) =>
        {
            // Block closing until the PC is on the correct network.
            if (!_connected) { e.Cancel = true; }
            else _autoTimer.Stop();
        };
    }

    private void InitializeComponent(string? currentSsid)
    {
        SuspendLayout();

        Text = "Нет связи с табло";
        Size = new Size(540, 340);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        ControlBox = false;          // no X / system menu → cannot be closed manually
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = true;
        TopMost = true;
        BackColor = AlertRed;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10f);
        try { Icon = TrayApplicationContext.CreateAppIcon(); } catch { }

        var icon = new Label
        {
            Text = "⚠",
            Font = new Font("Segoe UI", 44f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 78,
            BackColor = Color.Transparent,
        };

        var title = new Label
        {
            Text = "WI-FI ТАБЛО НЕ ПОДКЛЮЧЁН",
            Font = new Font("Segoe UI", 15f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 36,
            BackColor = Color.Transparent,
        };

        var body = new Label
        {
            Text = "Курсы не доходят до экрана!\nПодключите этот компьютер к сети Wi-Fi табло:",
            Font = new Font("Segoe UI", 10.5f),
            ForeColor = Color.White,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = Color.Transparent,
        };

        var ssid = new Label
        {
            Text = _expectedSsid,
            Font = new Font("Segoe UI", 17f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 40,
            BackColor = Color.FromArgb(140, 0, 24),
        };

        _lblCurrent = new Label
        {
            Text = CurrentText(currentSsid),
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = Color.FromArgb(255, 220, 220),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 26,
            BackColor = Color.Transparent,
        };

        var hint = new Label
        {
            Text = "Окно закроется автоматически после подключения.",
            Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
            ForeColor = Color.FromArgb(255, 210, 210),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 22,
            BackColor = Color.Transparent,
        };

        var buttons = new Panel { Dock = DockStyle.Bottom, Height = 58, BackColor = Color.Transparent };
        var btnRecheck = new RoundedButton
        {
            Text = "🔄  Проверить снова",
            BackColor = Color.White,
            ForeColor = AlertRed,
            Font = new Font("Segoe UI Semibold", 11f),
            Size = new Size(240, 40),
            CornerRadius = 9,
        };
        btnRecheck.Click += async (_, _) => await RecheckAsync();
        buttons.Controls.Add(btnRecheck);
        buttons.Resize += (_, _) =>
            btnRecheck.Location = new Point((buttons.ClientSize.Width - btnRecheck.Width) / 2, 9);

        // Add in reverse so Dock(Top) stacks in the intended visual order.
        Controls.Add(buttons);
        Controls.Add(hint);
        Controls.Add(_lblCurrent);
        Controls.Add(ssid);
        Controls.Add(body);
        Controls.Add(title);
        Controls.Add(icon);

        ResumeLayout();
    }

    /// <summary>
    /// Closes the alert programmatically. The window normally refuses to close until the
    /// PC is back on the correct network; this is used by the watchdog when the board
    /// becomes reachable again (delivery succeeded) so the alert never lingers.
    /// </summary>
    public void ForceClose()
    {
        _connected = true; // allow FormClosing to proceed
        _autoTimer.Stop();
        try { Close(); } catch { /* ignore if already disposing */ }
    }

    private static string CurrentText(string? currentSsid) =>
        string.IsNullOrWhiteSpace(currentSsid)
            ? "Сейчас Wi-Fi не подключён."
            : $"Сейчас подключено: {currentSsid}";

    private async Task RecheckAsync()
    {
        if (_checking) return;
        _checking = true;
        try
        {
            var current = await WifiInfo.GetConnectedSsidAsync();
            if (!WifiInfo.IsWrongNetwork(_expectedSsid, current))
            {
                _connected = true;     // allow FormClosing to proceed
                Close();
                return;
            }
            _lblCurrent.Text = CurrentText(current);
        }
        finally
        {
            _checking = false;
        }
    }
}
