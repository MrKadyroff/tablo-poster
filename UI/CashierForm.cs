namespace LedImageUpdaterService.UI;

/// <summary>
/// Simplified operator window for cashiers. Exposes exactly two actions —
/// refresh the rates from the API and push the rendered image to the board —
/// plus a preview of the image being sent. The board layout/design is fixed
/// and can only be changed from the admin <see cref="SettingsForm"/>.
/// </summary>
internal sealed class CashierForm : Form
{
    private AppConfig _cfg = AppSettingsManager.Load();

    private Label _lblPoint = null!;
    private Label _lblStatus = null!;
    private PictureBox _preview = null!;
    private Button _btnRefresh = null!;
    private Button _btnSend = null!;
    private Button _btnPowerOn = null!;
    private Button _btnPowerOff = null!;
    private Button _btnHelp = null!;

    // Big colored diagnostic banner at the top of the window.
    private Panel _banner = null!;
    private Label _bannerLabel = null!;

    // Periodic health probe — runs only while the window is visible.
    private readonly System.Windows.Forms.Timer _healthTimer = new() { Interval = 7000 };
    private bool _healthBusy;
    private readonly Action? _restartService;
    private DateTime _lastRestartUtc = DateTime.MinValue;

    private static readonly Font UIFont = new("Segoe UI", 9f);

    private static readonly Color BannerGreen = Color.FromArgb(0, 150, 80);
    private static readonly Color BannerOrange = Color.FromArgb(204, 122, 0);
    private static readonly Color BannerRed = Color.FromArgb(196, 43, 43);

    /// <param name="restartService">
    /// Invoked (throttled) when the local service/host stops responding, so the board
    /// updater can be restarted in the background without the cashier doing anything.
    /// </param>
    public CashierForm(Action? restartService = null)
    {
        _restartService = restartService;
        InitializeComponent();
        ReloadConfig();
        LoadExistingPreview();

        _healthTimer.Tick += async (_, _) => await RefreshHealthAsync();
        VisibleChanged += (_, _) =>
        {
            if (Visible) { _healthTimer.Start(); _ = RefreshHealthAsync(); }
            else _healthTimer.Stop();
        };
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        Font = UIFont;
        Text = "eCash Tablo — Курсы";
        Size = new Size(520, 620);
        MinimumSize = new Size(480, 560);
        StartPosition = FormStartPosition.CenterScreen;
        ShowInTaskbar = true;
        Icon = TrayApplicationContext.CreateAppIcon();
        BackColor = UITheme.Bg;
        FormClosing += (_, e) =>
        {
            e.Cancel = true;
            Hide();
        };

        // ─── Header ────────────────────────────────────────────────────────
        var header = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = UITheme.Bg };
        UITheme.PaintHeader(header);

        int textX = 18;
        var logoPath = Path.Combine(AppContext.BaseDirectory, "content", "common", "logo.png");
        if (File.Exists(logoPath))
        {
            try
            {
                using var loaded = Image.FromStream(new MemoryStream(File.ReadAllBytes(logoPath)));
                var logoBox = new PictureBox
                {
                    Image = new Bitmap(loaded),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(48, 48),
                    Location = new Point(16, 10),
                    BackColor = Color.Transparent,
                };
                header.Controls.Add(logoBox);
                textX = 76;
            }
            catch { }
        }

        header.Controls.Add(new Label
        {
            Text = "eCash Tablo",
            ForeColor = UITheme.Accent,
            Font = new Font("Segoe UI Semibold", 16f, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(textX, 12),
            BackColor = Color.Transparent,
        });
        header.Controls.Add(new Label
        {
            Text = "ОБНОВЛЕНИЕ КУРСОВ · ОТПРАВКА НА ТАБЛО",
            ForeColor = Color.FromArgb(150, 200, 230),
            Font = new Font("Segoe UI", 8f),
            AutoSize = true,
            Location = new Point(textX + 2, 44),
            BackColor = Color.Transparent,
        });

        // ─── Active point row ──────────────────────────────────────────────
        var pointRow = new Panel { Dock = DockStyle.Top, Height = 32, Padding = new Padding(12, 6, 12, 0) };
        _lblPoint = new Label
        {
            Dock = DockStyle.Fill,
            ForeColor = UITheme.Text,
            Font = new Font("Segoe UI Semibold", 10f),
            TextAlign = ContentAlignment.MiddleLeft,
            Text = "Точка: —",
        };
        _btnHelp = new RoundedButton
        {
            Text = "?",
            Width = 26,
            Height = 26,
            BackColor = UITheme.Input,
            ForeColor = UITheme.Text,
            Font = new Font("Segoe UI Semibold", 10f),
            CornerRadius = 13,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
        };
        _btnHelp.Click += (_, _) => ShowHelp();
        pointRow.Controls.Add(_lblPoint);
        pointRow.Controls.Add(_btnHelp);
        pointRow.Resize += (_, _) =>
            _btnHelp.Location = new Point(pointRow.ClientSize.Width - _btnHelp.Width - 12, 3);

        // ─── Status banner (under header) ──────────────────────────────────
        _banner = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = BannerGreen, Padding = new Padding(10, 4, 10, 4) };
        _bannerLabel = new Label
        {
            Dock = DockStyle.Fill,
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Text = "Проверка связи с табло…",
        };
        _banner.Controls.Add(_bannerLabel);

        // ─── Action buttons + status (bottom) ──────────────────────────────
        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 192, BackColor = UITheme.Panel, Padding = new Padding(14, 12, 14, 10) };
        bottom.Paint += (s, e) =>
        {
            using var pen = new Pen(UITheme.Border, 1f);
            e.Graphics.DrawLine(pen, 0, 0, bottom.Width, 0);
        };

        _btnRefresh = MakeBigButton("⟳  Обновить курсы из API", UITheme.Accent2);
        _btnRefresh.Location = new Point(12, 16);
        _btnRefresh.Click += async (_, _) => await RefreshRatesAsync();

        _btnSend = MakeBigButton("📤  Загрузить на табло", Color.FromArgb(16, 163, 127));
        _btnSend.Location = new Point(12, 64);
        _btnSend.Click += async (_, _) => await SendToBoardAsync();

        // ─── Power control (secondary, with confirmation) ──────────────────
        // Kept small and visually separate from the big send button so cashiers
        // do not turn the screen on/off by accident.
        var powerLabel = new Label
        {
            Text = "Питание табло:",
            AutoSize = true,
            ForeColor = UITheme.TextDim,
            Font = new Font("Segoe UI", 8.5f),
            Location = new Point(14, 122),
        };
        _btnPowerOn = MakeSmallButton("Включить", Color.FromArgb(0, 130, 70));
        _btnPowerOn.Location = new Point(276, 116); // refined by bottom.Resize
        _btnPowerOn.Click += async (_, _) => await SetPowerWithConfirmAsync(true);
        _btnPowerOff = MakeSmallButton("Выключить", Color.FromArgb(150, 45, 45));
        _btnPowerOff.Location = new Point(388, 116); // refined by bottom.Resize
        _btnPowerOff.Click += async (_, _) => await SetPowerWithConfirmAsync(false);

        _lblStatus = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 22,
            ForeColor = UITheme.TextDim,
            TextAlign = ContentAlignment.MiddleLeft,
            Text = "Готово.",
        };

        bottom.Controls.Add(_btnRefresh);
        bottom.Controls.Add(_btnSend);
        bottom.Controls.Add(powerLabel);
        bottom.Controls.Add(_btnPowerOn);
        bottom.Controls.Add(_btnPowerOff);
        bottom.Controls.Add(_lblStatus);

        // Keep the big buttons full-width and the power buttons anchored right on resize.
        bottom.Resize += (_, _) =>
        {
            int w = bottom.ClientSize.Width - 24;
            _btnRefresh.Width = w;
            _btnSend.Width = w;
            int right = bottom.ClientSize.Width - 12;
            _btnPowerOff.Location = new Point(right - _btnPowerOff.Width, 116);
            _btnPowerOn.Location = new Point(right - _btnPowerOff.Width - _btnPowerOn.Width - 8, 116);
        };

        // ─── Preview (fill) ────────────────────────────────────────────────
        var previewHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = UITheme.Bg };
        _preview = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Black,
            BorderStyle = BorderStyle.FixedSingle,
        };
        previewHost.Controls.Add(_preview);

        // Dock order (added last = docked first/topmost among Top docks):
        // header → banner → pointRow at the top; bottom at the bottom; preview fills.
        Controls.Add(previewHost);
        Controls.Add(bottom);
        Controls.Add(pointRow);
        Controls.Add(_banner);
        Controls.Add(header);

        UITheme.Apply(this);

        ResumeLayout();
    }

    private static Button MakeBigButton(string text, Color bg) => new RoundedButton
    {
        Text = text,
        Height = 42,
        Width = 460,
        BackColor = bg,
        ForeColor = Color.White,
        Font = new Font("Segoe UI Semibold", 11f),
        CornerRadius = 10,
    };

    private static Button MakeSmallButton(string text, Color bg) => new RoundedButton
    {
        Text = text,
        Height = 30,
        Width = 104,
        BackColor = bg,
        ForeColor = Color.White,
        Font = new Font("Segoe UI", 9f),
        CornerRadius = 7,
    };

    /// <summary>Re-reads settings (point can change while the window is hidden).</summary>
    internal void ReloadConfig()
    {
        _cfg = AppSettingsManager.Load();
        _lblPoint.Text = $"Точка: {_cfg.ActivePointId}";
    }

    // ─── Help ───────────────────────────────────────────────────────────────

    private void ShowHelp()
    {
        using var dlg = new Form
        {
            Text = "Как это работает",
            Size = new Size(480, 520),
            MinimumSize = new Size(360, 360),
            StartPosition = FormStartPosition.CenterParent,
            BackColor = UITheme.Bg,
            Icon = TrayApplicationContext.CreateAppIcon(),
        };

        var rtb = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            BackColor = UITheme.Bg,
            ForeColor = UITheme.Text,
            Font = UIFont,
        };
        // Reuse the admin Wiki tab's markup renderer (SettingsForm.RenderWikiMarkup) so the
        // cashier help reads consistently with the admin documentation.
        SettingsForm.RenderWikiMarkup(rtb, "Как пользоваться окном кассира", HelpText);

        dlg.Controls.Add(rtb);
        dlg.ShowDialog(this);
    }

    private const string HelpText =
        """
        ## Кнопки

        - «⟳ Обновить курсы из API» — запрашивает свежие курсы и перерисовывает
          превью. Табло при этом ещё не обновляется — только картинка в окне.
        - «📤 Загрузить на табло» — отправляет то, что показано в превью,
          непосредственно на экран табло.
        - «Включить» / «Выключить» — питание самого табло (с подтверждением,
          чтобы не нажать случайно).

        ## Если что-то не так

        - На табло старые курсы — нажмите «Обновить курсы из API», затем
          «Загрузить на табло».
        - Табло не реагирует — посмотрите на цветную полосу под шапкой окна:
          она подсказывает, в чём проблема (нет Wi-Fi табло, ПО не отвечает,
          ошибка отправки).
        - Полоса красная и просит подключиться к Wi-Fi — подключите этот
          компьютер к сети табло, полоса исчезнет сама, когда связь появится.
        - Если ничего не помогает — обратитесь к администратору, полные
          настройки и журнал есть в окне «Настройки».
        """;

    // ─── Actions ────────────────────────────────────────────────────────────

    private async Task RefreshRatesAsync()
    {
        SetBusy(true);
        SetStatus("Запрос курсов из API…", false);
        try
        {
            var err = await RatesApiClient.FetchAsync(_cfg.ActivePointId, _cfg.RatesApiUrl);
            if (err != null)
            {
                SetStatus("✗ Ошибка получения курсов", true);
                MessageBox.Show(err, "Курсы из API", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Render the production image so the preview matches exactly what
            // "Загрузить на табло" will push, and the watch-folder file is fresh.
            SetStatus("Курсы получены. Отрисовка…", false);
            var (img, rerr) = await RenderProductionAsync();
            if (img != null) SetPreview(img);
            SetStatus(rerr == null
                ? $"✓ Курсы обновлены {DateTime.Now:HH:mm:ss}. Нажмите «Загрузить на табло»."
                : "Курсы обновлены, но превью не построено.", rerr != null);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task SendToBoardAsync()
    {
        SetBusy(true);
        SetStatus("Отправка на табло…", false);
        try
        {
            var (ok, msg) = await LedControlClient.SendToBoardAsync(_cfg.Urls);
            SetStatus((ok ? "✓ " : "✗ ") + msg, !ok);
        }
        finally
        {
            SetBusy(false);
        }
    }

    // ─── Preview helpers ──────────────────────────────────────────────────────

    private async Task<(Image? image, string? error)> RenderProductionAsync()
    {
        var composePath = Path.Combine(
            AppContext.BaseDirectory, "layout", "points", $"{_cfg.ActivePointId}.compose.json");
        var ratesPath = Path.Combine(
            AppContext.BaseDirectory, "content", "points", _cfg.ActivePointId, "rates.json");
        return await PreviewRenderer.RenderAsync(composePath, ratesPath);
    }

    // Loads the last rendered output image (if any) so the window shows the
    // current board content immediately on open.
    private void LoadExistingPreview()
    {
        try
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "content", "points", _cfg.ActivePointId, "output");
            if (!Directory.Exists(dir)) return;
            var latest = Directory.GetFiles(dir, "*.*")
                .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                         || f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                         || f.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();
            if (latest == null) return;
            using var loaded = Image.FromStream(new MemoryStream(File.ReadAllBytes(latest)));
            SetPreview(new Bitmap(loaded));
        }
        catch { }
    }

    private void SetPreview(Image img)
    {
        var old = _preview.Image;
        _preview.Image = img;
        old?.Dispose();
    }

    // ─── Status helpers ───────────────────────────────────────────────────────

    private void SetBusy(bool busy)
    {
        _btnRefresh.Enabled = !busy;
        _btnSend.Enabled = !busy;
        _btnPowerOn.Enabled = !busy;
        _btnPowerOff.Enabled = !busy;
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
    }

    // ─── Power control ────────────────────────────────────────────────────────

    private async Task SetPowerWithConfirmAsync(bool on)
    {
        var verb = on ? "включить" : "выключить";
        var confirm = MessageBox.Show(
            $"Точно {verb} табло?",
            "Питание табло",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2); // default = "Нет", to avoid accidental Enter
        if (confirm != DialogResult.Yes) return;

        SetBusy(true);
        SetStatus($"Команда «{verb} табло»…", false);
        try
        {
            var (ok, msg) = await LedControlClient.SetPowerAsync(_cfg.Urls, on);
            SetStatus((ok ? "✓ " : "✗ ") + (ok ? $"Табло {(on ? "включено" : "выключено")}." : msg), !ok);
        }
        finally
        {
            SetBusy(false);
        }
    }

    // ─── Health banner ──────────────────────────────────────────────────────────

    private async Task RefreshHealthAsync()
    {
        if (_healthBusy) return;
        _healthBusy = true;
        try
        {
            // Most specific cause first: is the PC on the board's Wi-Fi network at all?
            // The loud standalone popup is handled by the tray-level WifiWatchdog; here we
            // only reflect the same state in the in-window banner.
            if (!string.IsNullOrWhiteSpace(_cfg.WifiSsid))
            {
                var ssid = await WifiInfo.GetConnectedSsidAsync();
                if (WifiInfo.IsWrongNetwork(_cfg.WifiSsid, ssid))
                {
                    SetBanner(BannerRed,
                        $"🔌 Wi-Fi табло не подключён! Подключитесь к сети: {_cfg.WifiSsid}");
                    return;
                }
            }

            var health = await LedControlClient.GetBoardHealthAsync(_cfg.Urls);
            ApplyHealthToBanner(health);
        }
        catch
        {
            // Never let a health probe disrupt the UI.
        }
        finally
        {
            _healthBusy = false;
        }
    }

    private void ApplyHealthToBanner(BoardHealth h)
    {
        // 1) The local service/host is not responding → try a throttled background restart.
        if (!h.ServiceUp)
        {
            SetBanner(BannerOrange,
                "⚠ ПО табло не отвечает — выполняется перезапуск…\nПодождите несколько секунд.");
            TryRestartServiceThrottled();
            return;
        }

        // 2) The controller is unreachable → almost always a Wi-Fi problem.
        if (!h.ControllerOnline)
        {
            SetBanner(BannerRed,
                $"🔌 ТАБЛО НЕ НА СВЯЗИ — курсы не доходят до экрана!\n" +
                $"Подключите этот ПК к Wi-Fi табло ({_cfg.ControllerIp}).");
            return;
        }

        // 3) Connected, but the last automatic send failed.
        if (h.LastFailureAt is { } fail && (h.LastSuccessAt is not { } ok || fail > ok))
        {
            var reason = string.IsNullOrWhiteSpace(h.LastFailureReason) ? "неизвестная ошибка" : h.LastFailureReason;
            SetBanner(BannerRed,
                $"✗ Ошибка отправки на табло:\n{Trim(reason, 90)} — пытаюсь снова автоматически…");
            return;
        }

        // 4) All good.
        var when = h.LastSuccessAt is { } s ? $" (посл. обновление {s.ToLocalTime():HH:mm})" : "";
        SetBanner(BannerGreen, $"✓ Табло на связи, курсы обновляются{when}.");
    }

    private void TryRestartServiceThrottled()
    {
        if (_restartService is null) return;
        if ((DateTime.UtcNow - _lastRestartUtc).TotalSeconds < 30) return;
        _lastRestartUtc = DateTime.UtcNow;
        try { _restartService(); } catch { /* tray handles its own errors */ }
    }

    private void SetBanner(Color bg, string text)
    {
        _banner.BackColor = bg;
        _bannerLabel.Text = text;
    }

    private static string Trim(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";

    private void SetStatus(string text, bool error)
    {
        _lblStatus.Text = text;
        _lblStatus.ForeColor = error ? Color.Salmon : UITheme.Accent;
    }
}
