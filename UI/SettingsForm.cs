namespace LedImageUpdaterService.UI;

internal sealed class SettingsForm : Form
{
    private readonly Action _onRestart;
    // Full app exit + restart used by the GitHub self-update flow (releases file locks).
    private readonly Action _onExitForUpdate;
    private AppConfig _cfg = new();

    // Header
    private ComboBox _cmbPoint = null!;
    private RoundedButton _btnBoard1 = null!, _btnBoard2 = null!, _btnRemoveSecond = null!;

    // Tab: Валюты
    private const int MaxColumns = 3;
    private readonly ListBox[] _lstColumns = new ListBox[MaxColumns];
    private readonly GroupBox[] _grpColumns = new CardBox[MaxColumns];
    private NumericStepper _numColumnCount = null!;
    private ComboBox _cmbAddTarget = null!;

    // Tab: Заголовки (per-column buy/sell labels)
    private readonly TextBox[] _txtBuyLabels = new TextBox[MaxColumns];
    private readonly TextBox[] _txtSellLabels = new TextBox[MaxColumns];
    private readonly GroupBox[] _grpHeaderCols = new CardBox[MaxColumns];

    // Tab: Табло
    private NumericStepper _numW = null!, _numH = null!;

    // Tab: Дизайн
    private LayoutEditorControl _editor = null!;
    private NumericStepper _numFszValue = null!, _numFszCode = null!, _numFszHdr = null!, _numFszArrow = null!;
    private NumericStepper _numFlagW = null!, _numFlagH = null!, _numLogoW = null!, _numLogoH = null!;
    private NumericStepper _numRowsStartY = null!, _numRowH = null!;
    // Per-column X placement (free column layout, e.g. centred logo with rates on both sides)
    private readonly NumericStepper[] _numColX = new NumericStepper[MaxColumns];
    private CheckBox _chkManualColX = null!;
    // Ticker (бегущая строка)
    private CheckBox _chkTicker = null!;
    private TrackBar _trkTickerSpeed = null!;
    private NumericStepper _numTickerH = null!, _numTickerFont = null!;
    private Button _btnTickerBg = null!, _btnTickerFg = null!;
    private FlowLayoutPanel _pnlTickerPresets = null!;
    private CheckBox _chkShine = null!;
    private NumericStepper _numShineCount = null!;
    private TrackBar _trkShineStrength = null!, _trkShineWidth = null!;
    private Label _lblShineStrength = null!, _lblShineWidth = null!;
    private Button _btnShineReset = null!;
    private Label _lblTickerSpeed = null!, _lblTickerText = null!;
    private Button _btnTickerText = null!;
    private bool _suppressSync;
    private TabControl _tabs = null!;
    private TabPage _designTab = null!;
    private CheckBox _chkAutoPreview = null!;
    private CheckBox _chkPermanentInternet = null!;
    private Label _lblPreviewStatus = null!;
    private Label _lblSendStatus = null!;
    private System.Windows.Forms.Timer _previewDebounce = null!;
    private bool _previewBusy;

    // Tab: Сервис
    private ComboBox _cmbRunMode = null!, _cmbPublishMode = null!;
    private NumericStepper _numPoll = null!, _numRatesFetch = null!;
    private CheckBox _chkLayout = null!, _chkAutoSend = null!, _chkSkipUnchanged = null!;
    private CheckBox _chkForceCompose = null!;

    // Tab: Подключение
    private TextBox _txtIp = null!, _txtFtpUser = null!, _txtFtpPass = null!;
    private TextBox _txtRatesUrl = null!, _txtReloadUrl = null!, _txtApiPort = null!;
    private TextBox _txtWifiSsid = null!;
    private NumericStepper _numCtrlPort = null!, _numFtpPort = null!, _numDevice = null!;
    private ComboBox _cmbModel = null!;
    private ComboBox _cmbConnMode = null!;
    private ComboBox _cmbFamily = null!;
    // Rows hidden/shown based on selected family.
    private Label? _lblCtrlPort, _lblModel, _lblDevice, _lblConnMode;
    private Label? _lblIp;
    private Button _btnDetectIp = null!;
    private FlowLayoutPanel _pnlIp = null!;
    private Label _lblPowerStatus = null!;
    private Label _lblConnTestResult = null!;
    private CheckBox _chkTls = null!;

    // Tab: Дополнительно
    private CheckBox _chkOnbonEnabled = null!, _chkIsolated = null!;
    private CheckBox _chkSkipDup = null!, _chkRejectSize = null!, _chkWifiOnly = null!, _chkPrivate = null!;
    private NumericStepper _numRetry = null!, _numRetryMs = null!, _numConnTimeout = null!, _numOnbonPoll = null!;
    private TextBox _txtOnbonUser = null!, _txtOnbonPass = null!;
    // Telegram notifications (token/chatId/enabled live in appsettings.json, edited by hand)
    private Button _btnTelegramTest = null!;

    // Tab: Журнал
    private RichTextBox _rtbLog = null!;

    // Tab: Вики
    private ListBox _lstWikiNav = null!;
    private RichTextBox _rtbWiki = null!;

    private static readonly Font UIFont = new("Segoe UI", 9f);
    private static readonly Font BoldFont = new("Segoe UI", 9f, FontStyle.Bold);

    public SettingsForm(Action onRestart, Action onExitForUpdate)
    {
        _onRestart = onRestart;
        _onExitForUpdate = onExitForUpdate;
        InitializeComponent();
        _cfg = AppSettingsManager.Load();
        PopulateForm();
        AttachChangeTracking(this);
        _changeTrackingAttached = true;
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        Font = UIFont;
        Text = "eCash Tablo — Настройки";
        Size = new Size(940, 740);
        MinimumSize = new Size(860, 660);
        StartPosition = FormStartPosition.CenterScreen;
        ShowInTaskbar = true;
        Icon = TrayApplicationContext.CreateAppIcon();
        KeyPreview = true;
        KeyDown += OnFormKeyDown;
        FormClosing += (_, e) =>
        {
            // The window is reused from the tray — never really close it, just hide.
            e.Cancel = true;
            if (e.CloseReason == CloseReason.UserClosing && !ConfirmDiscardChanges()) return;
            Hide();
        };

        BackColor = UITheme.Bg;

        // ─── Header panel ──────────────────────────────────────────────────
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 68,
            BackColor = UITheme.Bg,
        };
        UITheme.PaintHeader(header);

        int textX = 18;
        var logoPath = Path.Combine(AppContext.BaseDirectory, "content", "common", "logo.png");
        if (File.Exists(logoPath))
        {
            try
            {
                // Load via bytes so the PNG file isn't locked for the form's lifetime
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

        var lblTitle = new Label
        {
            Text = "eCash Tablo",
            ForeColor = UITheme.Accent,
            Font = new Font("Segoe UI Semibold", 16f, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(textX, 12),
            BackColor = Color.Transparent,
        };
        var lblSub = new Label
        {
            Text = "СИСТЕМА УПРАВЛЕНИЯ ТАБЛО · КУРСЫ ВАЛЮТ",
            ForeColor = Color.FromArgb(150, 200, 230),
            Font = new Font("Segoe UI", 8f),
            AutoSize = true,
            Location = new Point(textX + 2, 44),
            BackColor = Color.Transparent,
        };
        header.Controls.AddRange([lblTitle, lblSub]);

        // ─── Boards row: which screen is being edited, and its point ────────
        // Up to two boards run side by side; each has its own point (IP, size, layout, rates).
        var pointRow = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = UITheme.Panel, Padding = new Padding(18, 8, 18, 8) };
        _btnBoard1 = new RoundedButton { Width = 176, Height = 34, Location = new Point(18, 8), CornerRadius = 9, Font = new Font("Segoe UI Semibold", 9.5f) };
        _btnBoard2 = new RoundedButton { Width = 214, Height = 34, Location = new Point(200, 8), CornerRadius = 9, Font = new Font("Segoe UI Semibold", 9.5f) };
        _btnBoard1.Click += (_, _) => SelectBoard(0);
        _btnBoard2.Click += (_, _) => SelectBoard(1);
        var boardTips = new ToolTip();
        boardTips.SetToolTip(_btnBoard1, "Настройки и управление первым табло");
        boardTips.SetToolTip(_btnBoard2, "Настройки и управление вторым табло (свой экран, IP, раскладка, курсы)");

        var lblPoint = new Label
        {
            Text = "ТОЧКА",
            AutoSize = true,
            Location = new Point(430, 17),
            ForeColor = UITheme.TextDim,
            Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold),
        };
        _cmbPoint = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(486, 12),
            Width = 190,
            Font = new Font("Segoe UI Semibold", 9.5f),
        };
        _cmbPoint.Items.AddRange(AppSettingsManager.GetAvailablePoints());
        _cmbPoint.SelectedIndexChanged += (_, _) => OnBoardPointChosen();

        _btnRemoveSecond = new RoundedButton
        {
            Text = "✕  Отключить",
            Width = 140,
            Height = 34,
            Location = new Point(694, 8),
            CornerRadius = 9,
            BackColor = UITheme.Input,
            ForeColor = UITheme.Danger,
            Font = new Font("Segoe UI Semibold", 9.5f),
            Visible = false,
        };
        _btnRemoveSecond.Click += (_, _) => RemoveSecondBoard();
        boardTips.SetToolTip(_btnRemoveSecond, "Отключить второе табло (настройки его точки сохранятся)");
        pointRow.Controls.AddRange([_btnBoard1, _btnBoard2, lblPoint, _cmbPoint, _btnRemoveSecond]);

        // ─── Tab control ───────────────────────────────────────────────────
        // Pages are switched from the grouped side menu; the tab strip itself is hidden.
        var tabs = new HeaderlessTabControl { Dock = DockStyle.Fill, Font = UIFont };
        _tabs = tabs;
        var pgCurrencies = BuildCurrenciesTab();
        var pgHeaders = BuildHeadersTab();
        var pgDisplay = BuildDisplayTab();
        _designTab = BuildDesignTab();
        var pgService = BuildServiceTab();
        var pgConnection = BuildConnectionTab();
        var pgAdvanced = BuildAdvancedTab();
        var pgLog = BuildLogTab();
        var pgWiki = BuildWikiTab();
        tabs.TabPages.AddRange([pgCurrencies, pgHeaders, pgDisplay, _designTab,
                                pgService, pgConnection, pgAdvanced, pgLog, pgWiki]);
        tabs.SelectedIndexChanged += (_, _) =>
        {
            if (tabs.SelectedTab == _designTab)
                EnterDesignTab();
            if (tabs.SelectedTab == pgLog)
                _ = RefreshLogAsync();
        };

        var nav = new SideNav(tabs);
        nav.AddGroup("ТАБЛО");
        nav.AddItem("", "Валюты", pgCurrencies, "Ctrl+1");
        nav.AddItem("", "Заголовки", pgHeaders, "Ctrl+2");
        nav.AddItem("", "Размер табло", pgDisplay, "Ctrl+3");
        nav.AddItem("", "Дизайн", _designTab, "Ctrl+4");
        nav.AddGroup("СИСТЕМА");
        nav.AddItem("", "Сервис", pgService, "Ctrl+5");
        nav.AddItem("", "Подключение", pgConnection, "Ctrl+6");
        nav.AddItem("", "Дополнительно", pgAdvanced, "Ctrl+7");
        nav.AddGroup("СПРАВКА");
        nav.AddItem("", "Журнал", pgLog, "Ctrl+8");
        nav.AddItem("", "Вики", pgWiki, "Ctrl+9");

        // ─── Footer ────────────────────────────────────────────────────────
        var footer = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = UITheme.Panel, Padding = new Padding(16, 11, 16, 11) };
        footer.Paint += (s, e) =>
        {
            using var pen = new Pen(UITheme.Border, 1f);
            e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);
        };

        var btnSaveRestart = MakeButton("⟳  Сохранить и перезапустить", UITheme.Accent2, Color.White);
        btnSaveRestart.Location = new Point(16, 11);
        btnSaveRestart.Width = 248;
        btnSaveRestart.Height = 38;
        btnSaveRestart.Click += (_, _) => SaveAndRestart();
        new ToolTip().SetToolTip(btnSaveRestart, "Сохранить и сразу применить (Ctrl+Shift+S)");

        var btnSave = MakeButton("💾  Сохранить", Color.FromArgb(16, 163, 127), Color.White);
        btnSave.Location = new Point(272, 11);
        btnSave.Width = 132;
        btnSave.Height = 38;
        btnSave.Click += (_, _) => SaveOnly();
        new ToolTip().SetToolTip(btnSave, "Сохранить без перезапуска (Ctrl+S)");

        var btnClose = MakeButton("Закрыть", UITheme.Input, UITheme.Text);
        btnClose.Location = new Point(412, 11);
        btnClose.Width = 100;
        btnClose.Height = 38;
        btnClose.Click += (_, _) => Close();
        new ToolTip().SetToolTip(btnClose, "Закрыть окно (Esc)");

        // Save status / unsaved-changes indicator, right of the buttons.
        _lblSaveState = new Label
        {
            AutoSize = false,
            Location = new Point(526, 11),
            Size = new Size(380, 38),
            Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UITheme.TextDim,
            Font = new Font("Segoe UI", 9f),
        };

        footer.Controls.AddRange([btnSaveRestart, btnSave, btnClose, _lblSaveState]);

        Controls.Add(tabs);
        Controls.Add(nav);
        Controls.Add(pointRow);
        Controls.Add(header);
        Controls.Add(footer);

        // Apply the dark futuristic theme to the whole control tree
        UITheme.Apply(this);

        ResumeLayout();
    }

    // ─── Tab: Валюты ──────────────────────────────────────────────────────────

    private TabPage BuildCurrenciesTab()
    {
        var tab = new TabPage("Валюты") { Padding = new Padding(8) };

        var note = new Label
        {
            Text = "Двойной клик: слева — добавить, справа — убрать (Delete). Порядок в колонке = порядок на табло.",
            Dock = DockStyle.Top,
            Height = 24,
            ForeColor = UITheme.TextDim,
        };

        // Top strip: column count + add target
        var strip = new Panel { Dock = DockStyle.Top, Height = 46, Padding = new Padding(0, 4, 0, 4) };
        var lblCols = new Label { Text = "Колонок:", AutoSize = true, Location = new Point(4, 13) };
        _numColumnCount = MakeNumeric(1, MaxColumns);
        _numColumnCount.Location = new Point(72, 6);
        _numColumnCount.Width = 96;
        _numColumnCount.ValueChanged += (_, _) => ApplyColumnCount((int)_numColumnCount.Value);

        var lblTarget = new Label { Text = "Добавить в колонку:", AutoSize = true, Location = new Point(184, 13) };
        _cmbAddTarget = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 64, Location = new Point(314, 10) };
        strip.Controls.AddRange([lblCols, _numColumnCount, lblTarget, _cmbAddTarget]);

        // All currencies panel (left side)
        var grpAll = new CardBox { Text = "Все доступные", Dock = DockStyle.Left, Width = 230 };
        var lstAll = new ListBox { Dock = DockStyle.Fill, Font = UIFont, SelectionMode = SelectionMode.MultiExtended, IntegralHeight = false };
        // Search field above the list: filters by code or name as you type.
        var txtSearch = new TextBox { Dock = DockStyle.Top, Font = UIFont, PlaceholderText = "🔍  Поиск: USD, евро…", Tag = NoTrackTag };
        var searchGap = new Panel { Dock = DockStyle.Top, Height = 10 };
        grpAll.Controls.Add(lstAll);
        grpAll.Controls.Add(searchGap);
        grpAll.Controls.Add(txtSearch);

        // Add/remove buttons
        var btnPanel = new Panel { Dock = DockStyle.Left, Width = 132 };
        var btnAdd = MakeButton("Добавить  →", UITheme.Accent2, Color.White);
        btnAdd.SetBounds(12, 52, 108, 34);
        var btnRem = MakeButton("←  Убрать", UITheme.Danger, Color.White);
        btnRem.SetBounds(12, 94, 108, 34);
        btnPanel.Controls.AddRange([btnAdd, btnRem]);
        var tips = new ToolTip();
        tips.SetToolTip(btnAdd, "Добавить выбранные валюты в колонку (или двойной клик / Enter)");
        tips.SetToolTip(btnRem, "Убрать выбранные валюты из колонок (или Delete)");

        // Column listboxes host
        var columnsHost = new Panel { Dock = DockStyle.Fill };
        for (int i = MaxColumns - 1; i >= 0; i--)
        {
            int idx = i;
            var grp = new CardBox
            {
                Text = $"Колонка {idx + 1}",
                Dock = idx == 0 ? DockStyle.Fill : DockStyle.Left,
                Width = 150,
            };
            var lst = new ListBox { Dock = DockStyle.Fill, Font = UIFont, IntegralHeight = false };
            lst.DoubleClick += (_, _) => RemoveSelectedFromColumns();
            lst.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Delete) { RemoveSelectedFromColumns(); e.Handled = true; }
                else if (e.Alt && e.KeyCode == Keys.Up) { MoveItem(lst, -1); e.Handled = true; }
                else if (e.Alt && e.KeyCode == Keys.Down) { MoveItem(lst, 1); e.Handled = true; }
            };
            var upDown = MakeUpDownPanel(lst);
            grp.Controls.Add(lst);
            grp.Controls.Add(upDown);
            _lstColumns[idx] = lst;
            _grpColumns[idx] = grp;
            columnsHost.Controls.Add(grp);
        }

        // Populate lstAll
        var allCurrencies = AppSettingsManager.GetAvailableCurrencies();
        if (allCurrencies.Length == 0)
            allCurrencies = AppSettingsManager.KnownCurrencies.Keys.ToArray();
        var allItems = allCurrencies.Union(AppSettingsManager.KnownCurrencies.Keys).Distinct().OrderBy(c => c)
            .Select(code => $"{code}  {(AppSettingsManager.KnownCurrencies.TryGetValue(code, out var n) ? n : code)}")
            .ToList();
        lstAll.Items.AddRange([.. allItems]);

        txtSearch.TextChanged += (_, _) =>
        {
            var q = txtSearch.Text.Trim();
            lstAll.BeginUpdate();
            lstAll.Items.Clear();
            lstAll.Items.AddRange([.. allItems.Where(i => q.Length == 0
                || i.Contains(q, StringComparison.CurrentCultureIgnoreCase))]);
            lstAll.EndUpdate();
            if (lstAll.Items.Count == 1) lstAll.SelectedIndex = 0;
        };
        // Enter in the search box adds the (single / selected) match right away.
        txtSearch.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Down && lstAll.Items.Count > 0) { lstAll.Focus(); lstAll.SelectedIndex = 0; e.Handled = true; }
            if (e.KeyCode != Keys.Enter) return;
            if (lstAll.SelectedItems.Count == 0 && lstAll.Items.Count > 0) lstAll.SelectedIndex = 0;
            AddSelected();
            txtSearch.SelectAll();
            e.Handled = e.SuppressKeyPress = true;
        };

        void AddSelected()
        {
            int target = Math.Clamp((_cmbAddTarget.SelectedIndex >= 0 ? _cmbAddTarget.SelectedIndex : 0), 0, (int)_numColumnCount.Value - 1);
            var lst = _lstColumns[target];
            bool added = false;
            foreach (var item in lstAll.SelectedItems.Cast<string>())
            {
                var code = item.Split(' ')[0];
                if (lst.Items.Cast<string>().Any(s => s.Split(' ')[0] == code)) continue;
                lst.Items.Add(item);
                added = true;
            }
            if (added) MarkDirty();
        }

        btnAdd.Click += (_, _) => AddSelected();
        lstAll.DoubleClick += (_, _) => AddSelected();
        lstAll.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { AddSelected(); e.Handled = e.SuppressKeyPress = true; } };
        btnRem.Click += (_, _) => RemoveSelectedFromColumns();

        var mainPanel = new Panel { Dock = DockStyle.Fill };
        mainPanel.Controls.Add(columnsHost);
        mainPanel.Controls.Add(btnPanel);
        mainPanel.Controls.Add(grpAll);

        tab.Controls.Add(mainPanel);
        tab.Controls.Add(strip);
        tab.Controls.Add(note);

        return tab;
    }

    // Shows/hides column listboxes and rebuilds the add-target dropdown.
    private void ApplyColumnCount(int count)
    {
        count = Math.Clamp(count, 1, MaxColumns);
        for (int i = 0; i < MaxColumns; i++)
            _grpColumns[i].Visible = i < count;

        // Rebuild target combo
        var prev = _cmbAddTarget.SelectedIndex;
        _cmbAddTarget.Items.Clear();
        for (int i = 0; i < count; i++)
            _cmbAddTarget.Items.Add((i + 1).ToString());
        _cmbAddTarget.SelectedIndex = prev >= 0 && prev < count ? prev : 0;

        // Header tab columns follow the same count
        for (int i = 0; i < MaxColumns; i++)
            if (_grpHeaderCols[i] != null) _grpHeaderCols[i].Visible = i < count;

        // Column-X numerics follow the active column count too.
        ApplyColManualEnabled();
    }

    // ─── Tab: Заголовки ───────────────────────────────────────────────────────

    private TabPage BuildHeadersTab()
    {
        var tab = new TabPage("Заголовки") { Padding = new Padding(8) };

        var note = new Label
        {
            Text = "Заголовки «Покупаем/Продаём» для каждой колонки. По строке на язык (напр. казахский / русский / английский).",
            Dock = DockStyle.Top,
            Height = 36,
            ForeColor = UITheme.TextDim,
        };

        var host = new Panel { Dock = DockStyle.Fill };
        for (int i = MaxColumns - 1; i >= 0; i--)
        {
            int idx = i;
            var grp = new CardBox
            {
                Text = $"Колонка {idx + 1}",
                Dock = idx == 0 ? DockStyle.Fill : DockStyle.Left,
                Width = 230,
                Padding = new Padding(12, 36, 12, 12),
            };

            var lblBuy = new Label { Text = "Покупаем:", Dock = DockStyle.Top, Height = 18 };
            var txtBuy = new TextBox { Dock = DockStyle.Top, Multiline = true, Height = 70, ScrollBars = ScrollBars.Vertical, Font = UIFont };
            var lblSell = new Label { Text = "Продаём:", Dock = DockStyle.Top, Height = 18 };
            var txtSell = new TextBox { Dock = DockStyle.Top, Multiline = true, Height = 70, ScrollBars = ScrollBars.Vertical, Font = UIFont };

            // Dock=Top stacks last-added on top
            grp.Controls.Add(txtSell);
            grp.Controls.Add(lblSell);
            grp.Controls.Add(txtBuy);
            grp.Controls.Add(lblBuy);

            _txtBuyLabels[idx] = txtBuy;
            _txtSellLabels[idx] = txtSell;
            _grpHeaderCols[idx] = grp;
            host.Controls.Add(grp);
        }

        tab.Controls.Add(host);
        tab.Controls.Add(note);
        return tab;
    }

    private Panel MakeUpDownPanel(ListBox lst)
    {
        var pnl = new Panel { Dock = DockStyle.Bottom, Height = 44, Padding = new Padding(0, 8, 0, 0) };
        var btnUp = new RoundedButton { Text = "▲  Выше", Width = 92, Height = 30, Location = new Point(0, 10), BackColor = UITheme.Input, ForeColor = UITheme.Text, CornerRadius = 7 };
        var btnDn = new RoundedButton { Text = "▼  Ниже", Width = 92, Height = 30, Location = new Point(98, 10), BackColor = UITheme.Input, ForeColor = UITheme.Text, CornerRadius = 7 };
        btnUp.Click += (_, _) => MoveItem(lst, -1);
        btnDn.Click += (_, _) => MoveItem(lst, 1);
        var tips = new ToolTip();
        tips.SetToolTip(btnUp, "Поднять выбранную валюту (Alt+↑)");
        tips.SetToolTip(btnDn, "Опустить выбранную валюту (Alt+↓)");
        pnl.Controls.AddRange([btnUp, btnDn]);
        return pnl;
    }

    private void MoveItem(ListBox lst, int dir)
    {
        var idx = lst.SelectedIndex;
        if (idx < 0) return;
        var newIdx = idx + dir;
        if (newIdx < 0 || newIdx >= lst.Items.Count) return;
        var item = lst.Items[idx];
        lst.Items.RemoveAt(idx);
        lst.Items.Insert(newIdx, item);
        lst.SelectedIndex = newIdx;
        MarkDirty();
    }

    private void RemoveSelectedFromColumns()
    {
        bool removed = false;
        foreach (var lst in _lstColumns)
            foreach (var item in lst.SelectedItems.Cast<string>().ToList())
            {
                lst.Items.Remove(item);
                removed = true;
            }
        if (removed) MarkDirty();
    }

    // ─── Tab: Табло ───────────────────────────────────────────────────────────

    private TabPage BuildDisplayTab()
    {
        var tab = new TabPage("Размер табло") { Padding = new Padding(16) };
        var grp = new CardBox { Text = "Размер холста (пикселей)", Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(16, 38, 16, 14) };

        _numW = MakeNumeric(8, 4096);
        _numH = MakeNumeric(8, 4096);

        var rows = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
        rows.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        rows.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddRow(rows, "Ширина (px):", _numW);
        AddRow(rows, "Высота (px):", _numH);

        // One-click presets for the boards already in use.
        var presets = new FlowLayoutPanel { AutoSize = true, WrapContents = true, Margin = new Padding(0, 10, 0, 0) };
        presets.Controls.Add(new Label { Text = "Типовые:", AutoSize = true, ForeColor = UITheme.TextDim, Margin = new Padding(0, 9, 8, 0) });
        foreach (var (w, h) in new[] { (128, 160), (128, 256), (320, 400), (560, 80) })
        {
            var b = MakeButton($"{w} × {h}", UITheme.Input, UITheme.Text);
            b.Width = 96;
            b.Margin = new Padding(0, 2, 8, 2);
            b.Click += (_, _) => { _numW.Value = w; _numH.Value = h; };
            presets.Controls.Add(b);
        }
        AddFullRow(rows, presets);

        grp.Controls.Add(rows);

        var hint = new Label
        {
            Text = "Размер должен совпадать с ScreenWidth/ScreenHeight в настройках подключения и с физическими размерами табло.",
            Dock = DockStyle.Top,
            ForeColor = UITheme.TextDim,
            Height = 36,
        };
        tab.Controls.Add(grp);
        tab.Controls.Add(hint);
        return tab;
    }

    // ─── Tab: Дизайн ──────────────────────────────────────────────────────────

    private TabPage BuildDesignTab()
    {
        var tab = new TabPage("Дизайн") { Padding = new Padding(0) };

        // Editor on the left
        _editor = new LayoutEditorControl { Dock = DockStyle.Fill };
        _editor.GeometryChanged += (_, _) =>
        {
            MarkDirty();
            SyncDesignNumericsFromConfig();
            ScheduleLivePreview();
        };

        // Side panel on the right — a single scrolling column so nothing can overlap.
        var side = new Panel { Dock = DockStyle.Right, Width = 340, Padding = new Padding(14, 12, 14, 14), AutoScroll = true, BackColor = UITheme.Panel };
        side.Paint += (s, e) =>
        {
            using var pen = new Pen(UITheme.Border, 1f);
            e.Graphics.DrawLine(pen, 0, 0, 0, side.Height);
        };

        var col = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
        col.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        const int textWrap = 290;

        // Local helper: append a control as its own full-width, auto-sized row.
        void AddSide(Control c, bool fill = true)
        {
            int row = col.RowCount;
            col.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            col.RowCount = row + 1;
            if (fill) c.Dock = DockStyle.Top;
            col.Controls.Add(c, 0, row);
        }

        Label Caption(string text) => new()
        {
            Text = text,
            AutoSize = false,
            Height = 24,
            ForeColor = UITheme.TextDim,
            Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(2, 12, 2, 2),
            Dock = DockStyle.Top,
        };

        Label WrapHint(string text) => new()
        {
            Text = text,
            AutoSize = true,
            MaximumSize = new Size(textWrap, 0),
            ForeColor = UITheme.TextDim,
            Font = new Font("Segoe UI", 8.5f),
            Margin = new Padding(2, 4, 2, 6),
        };

        Button SideButton(string text, Color bg, Color fg)
        {
            var b = MakeButton(text, bg, fg);
            b.Height = 38;
            b.Margin = new Padding(0, 4, 0, 0);
            return b;
        }

        CheckBox SideCheck(string text) => new()
        {
            Text = text,
            AutoSize = true,
            Checked = true,
            ForeColor = UITheme.Text,
            Margin = new Padding(2, 6, 2, 2),
        };

        // ── Section: превью и раскладка ────────────────────────────────────────
        var btnAuto = SideButton("⊞  Раскидка по размеру", UITheme.Accent2, Color.White);
        btnAuto.Click += (_, _) =>
        {
            PullCurrencies();
            PullDisplay();
            _editor.Bind(_cfg);
            _editor.AutoLayout();
            SyncDesignNumericsFromConfig();
        };

        var btnPreview = SideButton("👁  Обновить превью", Color.FromArgb(16, 163, 127), Color.White);
        btnPreview.Click += (_, _) => _ = RenderPreviewAsync(silent: false);

        var btnApi = SideButton("⟳  Загрузить курсы из API", Color.FromArgb(124, 92, 173), Color.White);
        btnApi.Click += (_, _) => _ = FetchRatesAndPreviewAsync();

        _chkAutoPreview = SideCheck("Авто-обновление при правках");
        _chkAutoPreview.Tag = NoTrackTag;   // view preference, not a setting
        _lblPreviewStatus = new Label { AutoSize = true, ForeColor = UITheme.TextDim, Text = "", Margin = new Padding(2, 2, 2, 2) };

        // ── Section: отправка на табло ─────────────────────────────────────────
        var btnSend = SideButton("📤  Отправить на табло", Color.FromArgb(0, 168, 132), Color.White);
        btnSend.Click += (_, _) => _ = SendToBoardAsync();

        _chkPermanentInternet = SideCheck("Интернет постоянный (автоотправка)");
        _lblSendStatus = new Label { AutoSize = true, ForeColor = UITheme.TextDim, Text = "", Margin = new Padding(2, 2, 2, 2) };
        var sendHint = WrapHint("Снимите галочку для точек без постоянного интернета: курсы и картинка "
            + "обновляются, а на табло отправляйте вручную кнопкой выше. Применяется после «Сохранить и перезапустить».");

        // Debounce timer for live preview while editing
        _previewDebounce = new System.Windows.Forms.Timer { Interval = 500 };
        _previewDebounce.Tick += (_, _) =>
        {
            _previewDebounce.Stop();
            _ = RenderPreviewAsync(silent: true);
        };

        // Numeric controls — cards auto-size to their content (no fixed heights to overflow).
        CardBox NumCard(string title)
            => new() { Text = title, Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = new Padding(0, 6, 0, 0), Padding = new Padding(10, 34, 10, 12) };

        var grpFonts = NumCard("Размеры шрифтов");
        var fonts = NumGrid(); fonts.Dock = DockStyle.Top;
        _numFszValue = MakeNumeric(1, 200);
        _numFszCode = MakeNumeric(1, 200);
        _numFszHdr = MakeNumeric(1, 200);
        _numFszArrow = MakeNumeric(1, 200);
        AddRow(fonts, "Курс (цифры):", _numFszValue);
        AddRow(fonts, "Код валюты:", _numFszCode);
        AddRow(fonts, "Заголовок:", _numFszHdr);
        AddRow(fonts, "Стрелка:", _numFszArrow);
        grpFonts.Controls.Add(fonts);

        var grpFlag = NumCard("Флаг и лого");
        var flag = NumGrid(); flag.Dock = DockStyle.Top;
        _numFlagW = MakeNumeric(2, 4096);
        _numFlagH = MakeNumeric(2, 4096);
        _numLogoW = MakeNumeric(2, 4096);
        _numLogoH = MakeNumeric(2, 4096);
        AddRow(flag, "Флаг ширина:", _numFlagW);
        AddRow(flag, "Флаг высота:", _numFlagH);
        AddRow(flag, "Лого ширина:", _numLogoW);
        AddRow(flag, "Лого высота:", _numLogoH);
        grpFlag.Controls.Add(flag);

        var grpRows = NumCard("Строки");
        var rowsG = NumGrid(); rowsG.Dock = DockStyle.Top;
        _numRowsStartY = MakeNumeric(0, 4096);
        _numRowH = MakeNumeric(2, 4096);
        AddRow(rowsG, "Старт строк Y:", _numRowsStartY);
        AddRow(rowsG, "Высота строки:", _numRowH);
        grpRows.Controls.Add(rowsG);

        // ── Размещение колонок (X) — свободная раскладка ──────────────────────
        var grpColX = NumCard("Размещение колонок (X)");
        var colxG = NumGrid(); colxG.Dock = DockStyle.Top;
        _chkManualColX = new CheckBox { Text = "Задавать X колонок вручную", AutoSize = true };
        _chkManualColX.CheckedChanged += (_, _) => { ApplyColManualEnabled(); ScheduleLivePreview(); };
        colxG.Controls.Add(_chkManualColX, 0, colxG.RowCount);
        colxG.SetColumnSpan(_chkManualColX, 2);
        colxG.RowCount++;
        for (int i = 0; i < MaxColumns; i++)
        {
            _numColX[i] = MakeNumeric(0, 4096);
            _numColX[i].ValueChanged += (_, _) => { if (!_suppressSync) { PullColumnX(); ScheduleLivePreview(); } };
            AddRow(colxG, $"Колонка {i + 1} X:", _numColX[i]);
        }
        var btnLogoCenter = MakeButton("⊞ Лого по центру (широкое)", UITheme.Input, UITheme.Text);
        btnLogoCenter.Height = 34;
        btnLogoCenter.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        btnLogoCenter.Margin = new Padding(3, 6, 3, 3);
        btnLogoCenter.Click += (_, _) => ArrangeLogoCenter();
        colxG.Controls.Add(btnLogoCenter, 0, colxG.RowCount);
        colxG.SetColumnSpan(btnLogoCenter, 2);
        colxG.RowCount++;
        grpColX.Controls.Add(colxG);

        // ── Бегущая строка — полоса сверху, табло рисуется как GIF ────────────
        var grpTicker = NumCard("Бегущая строка");
        var tickerG = NumGrid(); tickerG.Dock = DockStyle.Top;

        void AddWide(Control c)
        {
            tickerG.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tickerG.Controls.Add(c, 0, tickerG.RowCount);
            tickerG.SetColumnSpan(c, 2);
            tickerG.RowCount++;
        }

        _chkTicker = new CheckBox { Text = "Показывать бегущую строку", AutoSize = true, Margin = new Padding(3, 3, 3, 6) };
        _chkTicker.CheckedChanged += (_, _) => OnTickerChanged();

        _btnTickerText = MakeButton("✎  Текст строки…", UITheme.Input, UITheme.Text);
        _btnTickerText.Height = 34;
        _btnTickerText.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _btnTickerText.Margin = new Padding(3, 2, 3, 3);
        _btnTickerText.Click += (_, _) => EditTickerText();

        _lblTickerText = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(textWrap - 30, 0),
            ForeColor = UITheme.TextDim,
            Font = new Font("Segoe UI", 8.5f),
            Margin = new Padding(3, 2, 3, 6),
        };

        _lblTickerSpeed = new Label { AutoSize = true, ForeColor = UITheme.TextDim, Margin = new Padding(3, 4, 3, 0) };
        _trkTickerSpeed = new TrackBar
        {
            Minimum = 5,               // tenths of a pixel per frame → 0.5 … 10
            Maximum = 100,
            TickFrequency = 5,
            SmallChange = 1,
            LargeChange = 5,
            AutoSize = false,
            Height = 34,
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            BackColor = UITheme.Card,
        };
        _trkTickerSpeed.ValueChanged += (_, _) => OnTickerChanged();

        AddWide(_chkTicker);
        AddWide(_btnTickerText);
        AddWide(_lblTickerText);
        AddWide(_lblTickerSpeed);
        AddWide(_trkTickerSpeed);

        // Colours: pick the band and text colour, or a ready-made pair with one click.
        _btnTickerBg = MakeButton("", UITheme.Input, UITheme.Text);
        _btnTickerFg = MakeButton("", UITheme.Input, UITheme.Text);
        _btnTickerBg.Width = _btnTickerFg.Width = 150;
        _btnTickerBg.Height = _btnTickerFg.Height = 32;
        _btnTickerBg.Click += (_, _) => PickTickerColor(background: true);
        _btnTickerFg.Click += (_, _) => PickTickerColor(background: false);
        AddRow(tickerG, "Цвет полосы:", _btnTickerBg);
        AddRow(tickerG, "Цвет текста:", _btnTickerFg);

        _pnlTickerPresets = new FlowLayoutPanel { AutoSize = true, WrapContents = true, MaximumSize = new Size(textWrap - 30, 0), Margin = new Padding(0, 2, 0, 4) };
        var presetTips = new ToolTip();
        foreach (var (name, bg, fg) in new[]
        {
            ("Оранжевый (eCash)", "#F58220", "#FFFFFF"),
            ("Красный", "#D32F2F", "#FFFFFF"),
            ("Синий", "#1F6FEB", "#FFFFFF"),
            ("Зелёный", "#16A34A", "#FFFFFF"),
            ("Чёрный с жёлтым", "#000000", "#FFD400"),
            ("Белый", "#FFFFFF", "#111111"),
        })
        {
            var b = MakeButton("Аа", ColorTranslator.FromHtml(bg), ColorTranslator.FromHtml(fg));
            b.Width = 44; b.Height = 30;
            b.Margin = new Padding(3, 3, 3, 3);
            b.Click += (_, _) => { _cfg.TickerBgColor = bg; _cfg.TickerTextColor = fg; UpdateTickerUi(); MarkDirty(); ScheduleLivePreview(); };
            presetTips.SetToolTip(b, name);
            _pnlTickerPresets.Controls.Add(b);
        }
        AddWide(_pnlTickerPresets);

        // Size: band height and text size. 0 = automatic.
        _numTickerH = MakeNumeric(0, 1024);
        _numTickerFont = MakeNumeric(0, 1024);
        _numTickerH.ValueChanged += (_, _) => OnTickerChanged();
        _numTickerFont.ValueChanged += (_, _) => OnTickerChanged();
        AddRow(tickerG, "Высота полосы:", _numTickerH);
        AddRow(tickerG, "Размер текста:", _numTickerFont);
        AddWide(new Label
        {
            Text = "0 = авто (полоса ≈14 % высоты табло, текст — 72 % полосы). Размеры в пикселях табло. Крупный текст делает анимацию длиннее и тяжелее — на табло она грузится дольше.",
            AutoSize = true,
            MaximumSize = new Size(textWrap - 30, 0),
            ForeColor = UITheme.TextDim,
            Font = new Font("Segoe UI", 8.5f),
            Margin = new Padding(3, 2, 3, 4),
        });
        grpTicker.Controls.Add(tickerG);

        // ── Мерцание флагов — блик пробегает по нескольким флагам, табло рисуется как GIF ──
        var grpShine = NumCard("Мерцание флагов");
        var shineG = NumGrid(); shineG.Dock = DockStyle.Top;
        _chkShine = new CheckBox { Text = "Блик на флагах (анимация)", AutoSize = true, Margin = new Padding(3, 3, 3, 6) };
        _chkShine.CheckedChanged += (_, _) => OnShineChanged();

        void AddShineWide(Control c)
        {
            shineG.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            shineG.Controls.Add(c, 0, shineG.RowCount);
            shineG.SetColumnSpan(c, 2);
            shineG.RowCount++;
        }

        TrackBar ShineTrack(int min, int max) => new()
        {
            Minimum = min,
            Maximum = max,
            TickFrequency = 10,
            SmallChange = 1,
            LargeChange = 10,
            AutoSize = false,
            Height = 34,
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            BackColor = UITheme.Card,
        };

        _numShineCount = MakeNumeric(1, 12);
        _numShineCount.ValueChanged += (_, _) => OnShineChanged();
        _lblShineStrength = new Label { AutoSize = true, ForeColor = UITheme.TextDim, Margin = new Padding(3, 4, 3, 0) };
        _trkShineStrength = ShineTrack(5, 100);
        _trkShineStrength.ValueChanged += (_, _) => OnShineChanged();
        _lblShineWidth = new Label { AutoSize = true, ForeColor = UITheme.TextDim, Margin = new Padding(3, 4, 3, 0) };
        _trkShineWidth = ShineTrack(5, 100);
        _trkShineWidth.ValueChanged += (_, _) => OnShineChanged();

        _btnShineReset = MakeButton("↺  По умолчанию", UITheme.Input, UITheme.Text);
        _btnShineReset.Height = 32;
        _btnShineReset.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _btnShineReset.Margin = new Padding(3, 6, 3, 3);
        _btnShineReset.Click += (_, _) =>
        {
            _numShineCount.Value = 3;
            _trkShineStrength.Value = 75;
            _trkShineWidth.Value = 35;
        };

        AddShineWide(_chkShine);
        AddRow(shineG, "Сколько флагов:", _numShineCount);
        AddShineWide(_lblShineStrength);
        AddShineWide(_trkShineStrength);
        AddShineWide(_lblShineWidth);
        AddShineWide(_trkShineWidth);
        AddShineWide(_btnShineReset);
        grpShine.Controls.Add(shineG);

        var hint = WrapHint("Перетаскивайте блоки мышью. Уголок выделенного блока — изменение размера. "
            + "«Обновить превью» рисует реальное изображение по текущим курсам.");

        // Wire numerics → config
        foreach (var n in new[] { _numFszValue, _numFszCode, _numFszHdr, _numFszArrow,
                                  _numFlagW, _numFlagH, _numLogoW, _numLogoH,
                                  _numRowsStartY, _numRowH })
        {
            n.ValueChanged += (_, _) => OnDesignNumericChanged();
        }

        // Assemble the column top-to-bottom (natural order — no dock stacking tricks).
        AddSide(Caption("ПРЕВЬЮ И РАСКЛАДКА"));
        AddSide(btnAuto);
        AddSide(btnPreview);
        AddSide(btnApi);
        AddSide(_chkAutoPreview);
        AddSide(_lblPreviewStatus, fill: false);
        AddSide(Caption("ОТПРАВКА НА ТАБЛО"));
        AddSide(btnSend);
        AddSide(_chkPermanentInternet);
        AddSide(_lblSendStatus, fill: false);
        AddSide(sendHint, fill: false);
        AddSide(Caption("ГЕОМЕТРИЯ"));
        AddSide(grpFonts);
        AddSide(grpFlag);
        AddSide(grpRows);
        AddSide(grpColX);
        AddSide(Caption("АНИМАЦИЯ"));
        AddSide(grpTicker);
        AddSide(grpShine);
        AddSide(hint, fill: false);

        side.Controls.Add(col);

        tab.Controls.Add(_editor);
        tab.Controls.Add(side);
        return tab;
    }

    private static TableLayoutPanel NumGrid()
    {
        var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        return t;
    }

    // Brings the Design tab to the front. Used when auto-opening the window for
    // points without permanent internet so the manual "Отправить на табло" button
    // is immediately in reach.
    internal void SelectDesignTab()
    {
        if (_tabs != null && _designTab != null)
            _tabs.SelectedTab = _designTab;
    }

    private void EnterDesignTab()
    {
        // Bring in latest canvas size + currency count so the editor reflects them
        PullCurrencies();
        PullDisplay();
        PullColumnX();
        _editor.Bind(_cfg);
        SyncDesignNumericsFromConfig();

        // Show a live preview immediately when entering the tab
        if (_chkAutoPreview.Checked)
            _ = RenderPreviewAsync(silent: true);
    }

    private void OnDesignNumericChanged()
    {
        if (_suppressSync) return;
        _cfg.FszValue = (int)_numFszValue.Value;
        _cfg.FszCode = (int)_numFszCode.Value;
        _cfg.FszHdr = (int)_numFszHdr.Value;
        _cfg.FszArrow = (int)_numFszArrow.Value;
        _cfg.ColFlagW = (int)_numFlagW.Value;
        _cfg.ColFlagH = (int)_numFlagH.Value;
        _cfg.LogoW = (int)_numLogoW.Value;
        _cfg.LogoH = (int)_numLogoH.Value;
        _cfg.RowsStartY = (int)_numRowsStartY.Value;
        _cfg.RowH = (int)_numRowH.Value;
        _editor.Invalidate();
        ScheduleLivePreview();
    }

    // Enables column-X numerics only for active columns when manual mode is on.
    private void ApplyColManualEnabled()
    {
        if (_chkManualColX is null || _numColX[0] is null) return; // not built yet
        bool manual = _chkManualColX.Checked;
        int active = Math.Clamp((int)_numColumnCount.Value, 1, MaxColumns);
        for (int i = 0; i < MaxColumns; i++)
            _numColX[i].Enabled = manual && i < active;
        if (!_suppressSync) PullColumnX();
    }

    // Collects per-column X from the UI into the config. Empty list = auto placement.
    private void PullColumnX()
    {
        if (!_chkManualColX.Checked)
        {
            _cfg.ColumnX = [];
            return;
        }

        int active = Math.Clamp((int)_numColumnCount.Value, 1, MaxColumns);
        _cfg.ColumnX = [];
        for (int i = 0; i < active; i++)
            _cfg.ColumnX.Add((int)_numColX[i].Value);
    }

    // Preset for wide boards: logo centred, columns pushed to the sides.
    private void ArrangeLogoCenter()
    {
        PullCurrencies();
        int cnt = Math.Clamp((int)_numColumnCount.Value, 1, MaxColumns);
        int w = (int)_numW.Value;
        int h = (int)_numH.Value;
        int logoW = (int)_numLogoW.Value;
        int logoH = (int)_numLogoH.Value;
        int colW = Math.Max(1, _cfg.ColSellX + _cfg.ColSellW); // approx column content width

        var xs = new List<int>();
        if (cnt == 1)
        {
            xs.Add(0);
        }
        else
        {
            int leftCount = cnt / 2 + (cnt % 2);   // ceil → extra column goes left
            int rightCount = cnt - leftCount;
            for (int i = 0; i < leftCount; i++) xs.Add(i * colW);
            for (int j = 0; j < rightCount; j++) xs.Add(Math.Max(0, w - (rightCount - j) * colW));
        }

        _suppressSync = true;
        _chkManualColX.Checked = true;
        for (int i = 0; i < MaxColumns; i++)
            _numColX[i].Value = Math.Clamp(xs.ElementAtOrDefault(i), 0, 4096);
        _suppressSync = false;

        // Centre the logo on the canvas.
        _cfg.LogoX = Math.Max(0, (w - logoW) / 2);
        _cfg.LogoY = Math.Max(0, (h - logoH) / 2);

        ApplyColManualEnabled();
        PullColumnX();
        _editor.Bind(_cfg);
        _editor.Invalidate();
        ScheduleLivePreview();
    }

    private void ScheduleLivePreview()
    {
        if (_suppressSync) return;
        if (_chkAutoPreview == null || !_chkAutoPreview.Checked) return;
        _previewDebounce.Stop();
        _previewDebounce.Start();
    }

    private void SyncDesignNumericsFromConfig()
    {
        _suppressSync = true;
        SetNum(_numFszValue, _cfg.FszValue);
        SetNum(_numFszCode, _cfg.FszCode);
        SetNum(_numFszHdr, _cfg.FszHdr);
        SetNum(_numFszArrow, _cfg.FszArrow);
        SetNum(_numFlagW, _cfg.ColFlagW);
        SetNum(_numFlagH, _cfg.ColFlagH);
        SetNum(_numLogoW, _cfg.LogoW);
        SetNum(_numLogoH, _cfg.LogoH);
        SetNum(_numRowsStartY, _cfg.RowsStartY);
        SetNum(_numRowH, _cfg.RowH);
        _chkTicker.Checked = _cfg.TickerEnabled;
        _chkShine.Checked = _cfg.ShineEnabled;
        SetNum(_numShineCount, _cfg.ShineCount);
        _trkShineStrength.Value = Math.Clamp((int)Math.Round(_cfg.ShineStrength * 100), _trkShineStrength.Minimum, _trkShineStrength.Maximum);
        _trkShineWidth.Value = Math.Clamp((int)Math.Round(_cfg.ShineWidth * 100), _trkShineWidth.Minimum, _trkShineWidth.Maximum);
        UpdateShineUi();
        _trkTickerSpeed.Value = Math.Clamp((int)Math.Round(_cfg.TickerSpeed * 10),
            _trkTickerSpeed.Minimum, _trkTickerSpeed.Maximum);
        SetNum(_numTickerH, _cfg.TickerH);
        SetNum(_numTickerFont, _cfg.TickerFontSize);
        UpdateTickerUi();
        _suppressSync = false;
    }

    private void OnShineChanged()
    {
        UpdateShineUi();
        if (_suppressSync) return;
        _cfg.ShineEnabled = _chkShine.Checked;
        _cfg.ShineCount = (int)_numShineCount.Value;
        _cfg.ShineStrength = _trkShineStrength.Value / 100.0;
        _cfg.ShineWidth = _trkShineWidth.Value / 100.0;
        ScheduleLivePreview();
    }

    private void UpdateShineUi()
    {
        bool on = _chkShine.Checked;
        _numShineCount.Enabled = on;
        _trkShineStrength.Enabled = on;
        _trkShineWidth.Enabled = on;
        _btnShineReset.Enabled = on;
        _lblShineStrength.Text = $"Яркость блика: {_trkShineStrength.Value} %";
        _lblShineWidth.Text = $"Ширина блика: {_trkShineWidth.Value} % флага";
    }

    // Swatch button: filled with the chosen colour, shows the hex code in a readable ink.
    private static void PaintColorButton(Button btn, string hex)
    {
        Color c;
        try { c = ColorTranslator.FromHtml(hex); } catch { c = Color.Gray; }
        btn.BackColor = c;
        btn.ForeColor = (c.R * 299 + c.G * 587 + c.B * 114) / 1000 > 150 ? Color.Black : Color.White;
        btn.Text = hex.ToUpperInvariant();
        btn.Invalidate();
    }

    private void PickTickerColor(bool background)
    {
        var current = background ? _cfg.TickerBgColor : _cfg.TickerTextColor;
        using var dlg = new ColorDialog { FullOpen = true, AnyColor = true };
        try { dlg.Color = ColorTranslator.FromHtml(current); } catch { }
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        var hex = $"#{dlg.Color.R:X2}{dlg.Color.G:X2}{dlg.Color.B:X2}";
        if (background) _cfg.TickerBgColor = hex; else _cfg.TickerTextColor = hex;
        UpdateTickerUi();
        MarkDirty();
        ScheduleLivePreview();
    }

    private void OnTickerChanged()
    {
        UpdateTickerUi();
        if (_suppressSync) return;
        _cfg.TickerEnabled = _chkTicker.Checked;
        _cfg.TickerSpeed = _trkTickerSpeed.Value / 10.0;
        _cfg.TickerH = (int)_numTickerH.Value;
        _cfg.TickerFontSize = (int)_numTickerFont.Value;
        ScheduleLivePreview();
    }

    private void UpdateTickerUi()
    {
        bool on = _chkTicker.Checked;
        _trkTickerSpeed.Enabled = on;
        _btnTickerText.Enabled = on;
        _numTickerH.Enabled = on;
        _btnTickerBg.Enabled = on;
        _btnTickerFg.Enabled = on;
        foreach (Control c in _pnlTickerPresets.Controls) c.Enabled = on;
        PaintColorButton(_btnTickerBg, _cfg.TickerBgColor);
        PaintColorButton(_btnTickerFg, _cfg.TickerTextColor);
        _numTickerFont.Enabled = on;
        _lblTickerSpeed.Text = $"Скорость: {_trkTickerSpeed.Value / 10.0:0.#} px/кадр";

        var text = _cfg.TickerText?.Trim() ?? "";
        _lblTickerText.Text = text.Length == 0
            ? "Текст: «Обмен валют» на 6 языках (по умолчанию)"
            : "Текст: " + (text.Length > 80 ? text[..80].Replace('\n', ' ') + "…" : text.Replace('\n', ' '));
    }

    // Dialog for the ticker message: one line = one message, separated by ★ on the board.
    private void EditTickerText()
    {
        using var dlg = new Form
        {
            Text = "Текст бегущей строки",
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MinimizeBox = false,
            MaximizeBox = false,
            ClientSize = new Size(520, 300),
            BackColor = UITheme.Bg,
            ForeColor = UITheme.Text,
            Font = new Font("Segoe UI", 9.5f),
        };

        var info = new Label
        {
            Dock = DockStyle.Top,
            Height = 54,
            Padding = new Padding(12, 10, 12, 0),
            ForeColor = UITheme.TextDim,
            Text = "Каждая строка — отдельное сообщение, на табло они идут через «★».\n"
                 + "Оставьте пустым — будет «Обмен валют» на 6 языках.",
        };

        var txt = new TextBox
        {
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            AcceptsReturn = true,
            Dock = DockStyle.Fill,
            Text = (_cfg.TickerText ?? "").Replace("\r\n", "\n").Replace("\n", "\r\n"),
        };
        var txtHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 4, 12, 8) };
        txtHost.Controls.Add(txt);

        var btnOk = MakeButton("Сохранить", UITheme.Accent2, Color.White);
        btnOk.Width = 120;
        btnOk.DialogResult = DialogResult.OK;
        var btnClear = MakeButton("Очистить", UITheme.Input, UITheme.Text);
        btnClear.Width = 110;
        btnClear.Click += (_, _) => txt.Clear();
        var btnCancel = MakeButton("Отмена", UITheme.Input, UITheme.Text);
        btnCancel.Width = 110;
        btnCancel.DialogResult = DialogResult.Cancel;

        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(8, 10, 8, 8),
            BackColor = UITheme.Panel,
        };
        footer.Controls.AddRange([btnOk, btnCancel, btnClear]);

        dlg.Controls.Add(txtHost);
        dlg.Controls.Add(info);
        dlg.Controls.Add(footer);
        dlg.AcceptButton = null;          // Enter adds a new line in the text box
        dlg.CancelButton = btnCancel;
        UITheme.Apply(dlg);

        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        MarkDirty();
        _cfg.TickerText = string.Join("\n", txt.Lines
            .Select(l => l.Trim())
            .Where(l => l.Length > 0));
        UpdateTickerUi();
        ScheduleLivePreview();
    }

    private static void SetNum(NumericStepper n, int v)
        => n.Value = Math.Clamp(v, (int)n.Minimum, (int)n.Maximum);

    private async Task RenderPreviewAsync(bool silent)
    {
        if (_previewBusy) return;
        _previewBusy = true;
        try
        {
            // Bring current edits into _cfg and persist only the layout so the
            // composer renders exactly what is on screen.
            PullCurrencies();
            PullDisplay();
            if (_cfg.AllCurrencies.Count == 0)
            {
                SetPreviewStatus("Нет выбранных валют", true);
                return;
            }

            SetPreviewStatus("Рендеринг…", false);
            // Render to a temp compose/output so production files and the board
            // are not touched until the user explicitly saves.
            var (composePath, _) = AppSettingsManager.WriteTempCompose(_cfg);
            var ratesPath = System.IO.Path.Combine(
                AppContext.BaseDirectory, "content", "points", _cfg.ActivePointId, "rates.json");

            var (image, error) = await PreviewRenderer.RenderAsync(composePath, ratesPath);
            if (error != null)
            {
                SetPreviewStatus("Ошибка рендера", true);
                if (!silent)
                    MessageBox.Show(error, "Превью", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _editor.SetBackground(image);
            SetPreviewStatus($"Обновлено {DateTime.Now:HH:mm:ss}", false);
        }
        finally
        {
            _previewBusy = false;
        }
    }

    private async Task FetchRatesAndPreviewAsync()
    {
        SetPreviewStatus("Запрос курсов…", false);
        var apiUrl = string.IsNullOrWhiteSpace(_txtRatesUrl.Text) ? _cfg.RatesApiUrl : _txtRatesUrl.Text.Trim();

        var err = await RatesApiClient.FetchAsync(_cfg.ActivePointId, apiUrl);
        if (err != null)
        {
            SetPreviewStatus("Ошибка API", true);
            MessageBox.Show(err, "Курсы из API", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        SetPreviewStatus("Курсы загружены", false);
        await RenderPreviewAsync(silent: false);
    }

    private void SetPreviewStatus(string text, bool error)
    {
        _lblPreviewStatus.Text = text;
        _lblPreviewStatus.ForeColor = error ? Color.Salmon : Color.LightGray;
    }

    // Manually pushes the latest rendered image to the board via the local REST API.
    // Works regardless of the "Интернет постоянный" checkbox, so operators of points
    // without permanent internet can update the board on demand.
    private async Task SendToBoardAsync()
    {
        SetSendStatus("Отправка на табло…", false);
        var (ok, msg) = await LedControlClient.SendToBoardAsync(_cfg.BoardUrls);
        SetSendStatus((ok ? "✓ " : "✗ ") + msg, !ok);
    }

    private void SetSendStatus(string text, bool error)
    {
        _lblSendStatus.Text = text;
        _lblSendStatus.ForeColor = error ? Color.Salmon : UITheme.Accent;
    }

    // ─── Tab: Сервис ──────────────────────────────────────────────────────────

    private TabPage BuildServiceTab()
    {
        var tab = new TabPage("Сервис") { Padding = new Padding(12) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _cmbRunMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };
        _cmbRunMode.Items.AddRange(["RenderOnly", "Uploader", "Full"]);

        _cmbPublishMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };
        _cmbPublishMode.Items.AddRange(["WifiFtp", "WifiRelay"]);

        _numPoll = MakeNumeric(5, 3600);
        _numRatesFetch = MakeNumeric(1, 120);

        _chkLayout = new CheckBox { Text = "Режим тестирования (не отправлять на табло)", AutoSize = true };
        _chkAutoSend = new CheckBox { Text = "Автоотправка на табло", AutoSize = true };
        _chkSkipUnchanged = new CheckBox { Text = "Пропускать без изменений", AutoSize = true };
        _chkForceCompose = new CheckBox { Text = "Перерисовывать каждый цикл", AutoSize = true };

        AddFullRow(layout, MakeDivider("РЕЖИМ"));
        AddRow(layout, "Режим работы:", _cmbRunMode);
        AddRow(layout, "Режим публикации:", _cmbPublishMode);
        AddFullRow(layout, MakeDivider("РАСПИСАНИЕ"));
        AddRow(layout, "Интервал опроса (сек):", _numPoll);
        AddRow(layout, "Обновление курсов (мин):", _numRatesFetch);
        AddFullRow(layout, MakeDivider("ОТПРАВКА"));
        AddRow(layout, "", _chkLayout);
        AddRow(layout, "", _chkAutoSend);
        AddRow(layout, "", _chkSkipUnchanged);
        AddRow(layout, "", _chkForceCompose);

        var tips = new ToolTip { AutoPopDelay = 15000 };
        tips.SetToolTip(_cmbRunMode, "RenderOnly — только рисовать картинку; Uploader — только отправлять; Full — всё вместе");
        tips.SetToolTip(_chkLayout, "Картинка рисуется, но на табло не отправляется — для проверки разметки");
        tips.SetToolTip(_chkSkipUnchanged, "Не отправлять на табло, если курсы и картинка не изменились");
        tips.SetToolTip(_chkForceCompose, "Перерисовывать картинку на каждом цикле, даже без изменений курсов");

        tab.Controls.Add(WrapScroll(layout));
        return tab;
    }

    // ─── Tab: Подключение ─────────────────────────────────────────────────────

    private TabPage BuildConnectionTab()
    {
        var tab = new TabPage("Подключение") { Padding = new Padding(12) };
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _txtIp = new TextBox { Width = 160 };
        _txtWifiSsid = new TextBox { Width = 220 };
        _numCtrlPort = MakeNumeric(1, 65535);
        _numDevice = MakeNumeric(1000, 65535);
        _cmbModel = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 280 };
        foreach (var m in ControllerCatalog.Models)
            _cmbModel.Items.Add(ControllerCatalog.DisplayName(m));
        _cmbModel.Items.Add(ControllerCatalog.CustomLabel);
        _cmbModel.SelectedIndexChanged += (_, _) => OnModelSelected();
        _cmbConnMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 280 };
        foreach (var m in ConnectionModeCatalog.Modes)
            _cmbConnMode.Items.Add(m.Label);
        _cmbFamily = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 280 };
        foreach (var f in ControllerFamilyCatalog.Families)
            _cmbFamily.Items.Add(f.Label);
        _txtFtpUser = new TextBox { Width = 160 };
        _txtFtpPass = new TextBox { Width = 160, PasswordChar = '●' };
        _numFtpPort = MakeNumeric(1, 65535);
        _chkTls = new CheckBox { Text = "Использовать TLS", AutoSize = true };
        _txtRatesUrl = new TextBox { Width = 350 };
        _txtReloadUrl = new TextBox { Width = 350 };
        _txtApiPort = new TextBox { Width = 160 };

        // ── Connection test ────────────────────────────────────────────────
        AddFullRow(layout, MakeDivider("ПРОВЕРКА СОЕДИНЕНИЯ"));

        var testRow = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 2, 0, 2) };
        var btnTest = MakeButton("🔍 Проверить соединение", UITheme.Accent2, Color.White);
        btnTest.Width = 200;
        btnTest.Click += (_, _) => _ = TestConnectionAsync();
        testRow.Controls.Add(btnTest);
        AddFullRow(layout, testRow);

        _lblConnTestResult = new Label { Text = "", AutoSize = true, ForeColor = UITheme.TextDim, Margin = new Padding(2, 0, 0, 6) };
        AddFullRow(layout, _lblConnTestResult);

        // ── Power control (operational, calls the local REST API) ──────────
        AddFullRow(layout, MakeDivider("ПИТАНИЕ ТАБЛО"));

        var powerRow = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 2, 0, 2) };
        var btnOn = MakeButton("🔆 Включить", Color.FromArgb(16, 163, 127), Color.White);
        btnOn.Width = 120;
        var btnOff = MakeButton("⏻ Выключить", UITheme.Danger, Color.White);
        btnOff.Width = 120;
        var btnReboot = MakeButton("↻ Перезагрузить", UITheme.Input, UITheme.Text);
        btnReboot.Width = 140;
        btnOn.Click += (_, _) => _ = PowerAsync(true);
        btnOff.Click += (_, _) => _ = PowerAsync(false);
        btnReboot.Click += (_, _) => _ = RebootBoardAsync();
        powerRow.Controls.AddRange([btnOn, btnOff, btnReboot]);
        AddFullRow(layout, powerRow);

        _lblPowerStatus = new Label { Text = "", AutoSize = true, ForeColor = UITheme.TextDim, Margin = new Padding(2, 0, 0, 4) };
        AddFullRow(layout, _lblPowerStatus);

        // ── Controller family ──────────────────────────────────────────────
        AddFullRow(layout, MakeDivider("КОНТРОЛЛЕР LED"));
        AddRow(layout, "Семейство (подключение):", _cmbFamily);

        // Onbon fields
        // IP field + "detect" button: the button asks the running service to scan the
        // board's own Wi-Fi (BoardLinkMonitor) and fills in the IP it finds (usually the
        // AP's gateway, e.g. 192.168.43.1). Handy on-site: connect to the board AP, press this.
        _btnDetectIp = MakeButton("🔍 Найти контроллер", Color.FromArgb(0, 150, 136), Color.White);
        _btnDetectIp.Width = 220;
        _btnDetectIp.Margin = new Padding(6, 0, 0, 0);
        _btnDetectIp.Click += (_, _) => _ = DetectControllerIpAsync();
        _pnlIp = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0),
            WrapContents = false,
        };
        _pnlIp.Controls.Add(_txtIp);
        _pnlIp.Controls.Add(_btnDetectIp);
        _lblIp = AddRow(layout, "IP-адрес контроллера: ", _pnlIp);
        _lblCtrlPort = AddRow(layout, "Порт контроллера:", _numCtrlPort);
         AddRow(layout, "Wi-Fi SSID табло:", _txtWifiSsid);
        _lblModel = AddRow(layout, "Модель контроллера:", _cmbModel);
        _lblDevice = AddRow(layout, "Код устройства:", _numDevice);
        _lblConnMode = AddRow(layout, "Тип подключения:", _cmbConnMode);

        _cmbFamily.SelectedIndexChanged += (_, _) => ApplyFamilyVisibility();

        // ── FTP ─────────────────────────────────────────────────────────
        AddFullRow(layout, MakeDivider("FTP"));

        AddRow(layout, "FTP пользователь:", _txtFtpUser);
        AddRow(layout, "FTP пароль:", _txtFtpPass);
        AddRow(layout, "FTP порт:", _numFtpPort);
        AddRow(layout, "", _chkTls);

        // ── API ──────────────────────────────────────────────────────────
        AddFullRow(layout, MakeDivider("API"));

        AddRow(layout, "URL API курсов:", _txtRatesUrl);
        AddRow(layout, "URL перезагрузки:", _txtReloadUrl);
        AddRow(layout, "Адрес сервиса (URL):", _txtApiPort);

        scroll.Controls.Add(layout);
        tab.Controls.Add(scroll);
        return tab;
    }

    private async Task TestConnectionAsync()
    {
        _lblConnTestResult.Text = "Проверяю…";
        _lblConnTestResult.ForeColor = Color.LightGray;

        var (isOnline, details) = await LedControlClient.CheckConnectionAsync(_cfg.BoardUrls);

        _lblConnTestResult.Text = (isOnline ? "✓ Онлайн  " : "✗ Оффлайн  ") + details;
        _lblConnTestResult.ForeColor = isOnline ? UITheme.Accent : Color.Salmon;
    }

    // Asks the running service to scan the board's own Wi-Fi for the controller and fills
    // the IP field with what it finds (BoardLinkMonitor). Handy on-site: connect to the
    // board AP, press this.
    private async Task DetectControllerIpAsync()
    {
        _btnDetectIp.Enabled = false;
        _lblConnTestResult.ForeColor = Color.LightGray;
        _lblConnTestResult.Text = "Ищу контроллер в сети табло…";
        try
        {
            var (ok, ip, message) = await LedControlClient.DetectControllerIpAsync(_cfg.BoardUrls);
            if (!string.IsNullOrWhiteSpace(ip))
            {
                _txtIp.Text = ip;
                _lblConnTestResult.ForeColor = UITheme.Accent;
                _lblConnTestResult.Text = $"✓ Контроллер найден: {ip}. {message} (не забудьте «Сохранить»)";
            }
            else
            {
                _lblConnTestResult.ForeColor = ok ? UITheme.Accent : Color.Salmon;
                _lblConnTestResult.Text = (ok ? "✓ " : "✗ ") + message;
            }
        }
        catch (Exception ex)
        {
            _lblConnTestResult.ForeColor = Color.Salmon;
            _lblConnTestResult.Text = $"Ошибка поиска: {ex.Message}";
        }
        finally
        {
            _btnDetectIp.Enabled = true;
        }
    }

    // When a known model is picked, auto-fill the device code and lock the field.
    // "Другой (вручную)" unlocks the code field for custom controllers.
    private void OnModelSelected()
    {
        if (_suppressSync) return;
        var idx = _cmbModel.SelectedIndex;
        if (idx >= 0 && idx < ControllerCatalog.Models.Count)
        {
            var model = ControllerCatalog.Models[idx];
            _numDevice.Value = Math.Clamp(model.DeviceType, (int)_numDevice.Minimum, (int)_numDevice.Maximum);
            _numDevice.Enabled = false;
        }
        else
        {
            // Custom
            _numDevice.Enabled = true;
        }
    }

    // Selects the dropdown entry matching the current device code (or "Другой").
    private void SyncModelFromDeviceType()
    {
        _suppressSync = true;
        var model = ControllerCatalog.FindByDeviceType(_cfg.DeviceType);
        if (model != null)
        {
            _cmbModel.SelectedIndex = ControllerCatalog.Models.ToList().IndexOf(model);
            _numDevice.Enabled = false;
        }
        else
        {
            _cmbModel.SelectedIndex = _cmbModel.Items.Count - 1; // "Другой"
            _numDevice.Enabled = true;
        }
        _suppressSync = false;
    }

    // ─── Board power control ──────────────────────────────────────────────────

    private async Task PowerAsync(bool on)
    {
        SetPowerStatus(on ? "Включаю табло…" : "Выключаю табло…", false);
        var (ok, msg) = await LedControlClient.SetPowerAsync(_cfg.BoardUrls, on);
        SetPowerStatus((ok ? "✓ " : "✗ ") + msg, !ok);
    }

    private async Task RebootBoardAsync()
    {
        if (MessageBox.Show("Перезагрузить контроллер табло?", "Перезагрузка",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        SetPowerStatus("Перезагружаю контроллер…", false);
        var (ok, msg) = await LedControlClient.RebootAsync(_cfg.BoardUrls);
        SetPowerStatus((ok ? "✓ " : "✗ ") + msg, !ok);
    }

    private void SetPowerStatus(string text, bool error)
    {
        _lblPowerStatus.Text = text;
        _lblPowerStatus.ForeColor = error ? Color.Salmon : UITheme.Accent;
    }

    // ─── Tab: Дополнительно ───────────────────────────────────────────────────

    private TabPage BuildAdvancedTab()
    {
        var tab = new TabPage("Дополнительно") { Padding = new Padding(12) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _chkOnbonEnabled = new CheckBox { Text = "SDK включён (Windows)", AutoSize = true };
        _txtOnbonUser = new TextBox { Width = 140 };
        _txtOnbonPass = new TextBox { Width = 140, PasswordChar = '●' };
        _numRetry = MakeNumeric(0, 10);
        _numRetryMs = MakeNumeric(100, 30000);
        _numConnTimeout = MakeNumeric(100, 30000);
        _numOnbonPoll = MakeNumeric(5, 3600);
        _chkIsolated = new CheckBox { Text = "Изолированный процесс SDK", AutoSize = true };
        _chkSkipDup = new CheckBox { Text = "Пропускать дублирующиеся изображения", AutoSize = true };
        _chkRejectSize = new CheckBox { Text = "Отклонять несовпадение размера", AutoSize = true };
        _chkWifiOnly = new CheckBox { Text = "Только через Wi-Fi адаптер", AutoSize = true };
        _chkPrivate = new CheckBox { Text = "Требовать приватный IP", AutoSize = true };

        AddFullRow(layout, MakeDivider("SDK КОНТРОЛЛЕРА"));
        AddRow(layout, "", _chkOnbonEnabled);
        AddRow(layout, "Логин SDK:", _txtOnbonUser);
        AddRow(layout, "Пароль SDK:", _txtOnbonPass);
        AddFullRow(layout, MakeDivider("ПОВТОРЫ И ТАЙМАУТЫ"));
        AddRow(layout, "Повторных попыток:", _numRetry);
        AddRow(layout, "Задержка попытки (мс):", _numRetryMs);
        AddRow(layout, "Таймаут подключения (мс):", _numConnTimeout);
        AddRow(layout, "Интервал опроса SDK (сек):", _numOnbonPoll);
        AddFullRow(layout, MakeDivider("ЗАЩИТА ОТПРАВКИ"));
        AddRow(layout, "", _chkIsolated);
        AddRow(layout, "", _chkSkipDup);
        AddRow(layout, "", _chkRejectSize);
        AddRow(layout, "", _chkWifiOnly);
        AddRow(layout, "", _chkPrivate);

        // ─── Telegram notifications ────────────────────────────────────────
        // Token / Chat ID / Enabled are configured by hand in appsettings.json (section
        // "Telegram"). Here we only offer a test button that reads those saved values.
        _btnTelegramTest = MakeButton("✈ Тест Telegram", Color.FromArgb(0, 136, 204), Color.White);
        _btnTelegramTest.Width = 200;
        _btnTelegramTest.Click += async (_, _) => await SendTelegramTestAsync();

        AddFullRow(layout, MakeDivider("TELEGRAM-УВЕДОМЛЕНИЯ"));
        AddRow(layout, "", new Label
        {
            Text = "Token, Chat ID и включение задаются в appsettings.json (раздел \"Telegram\").",
            AutoSize = true,
            ForeColor = UITheme.TextDim,
        });
        AddRow(layout, "", _btnTelegramTest);

        // ─── Обновление приложения ─────────────────────────────────────────
        AddFullRow(layout, MakeDivider("ОБНОВЛЕНИЕ ПРИЛОЖЕНИЯ"));
        AddRow(layout, "", new Label
        {
            Text = $"Текущая версия: v{UpdateService.CurrentVersion.ToString(3)}. " +
                   "Обновление сохраняет настройки и разметку.",
            AutoSize = true,
            ForeColor = UITheme.TextDim,
        });
        var btnCheckUpdate = MakeButton("🔄 Проверить обновления", UITheme.Accent2, Color.White);
        btnCheckUpdate.Width = 220;
        btnCheckUpdate.Click += (_, _) =>
        {
            using var dlg = new UpdateForm(_onExitForUpdate);
            dlg.ShowDialog(this);
        };
        AddRow(layout, "", btnCheckUpdate);

        tab.Controls.Add(WrapScroll(layout));
        return tab;
    }

    /// <summary>Puts a top-docked, auto-sized layout into a scrollable host so nothing is cut off.</summary>
    private static Panel WrapScroll(Control content)
    {
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        content.Dock = DockStyle.Top;
        scroll.Controls.Add(content);
        return scroll;
    }

    /// <summary>
    /// Sends a test message using the token/chat id currently typed in the form, so the
    /// admin can verify Telegram works before saving/restarting. Does not require a restart.
    /// </summary>
    private async Task SendTelegramTestAsync()
    {
        // Read freshly from appsettings.json so the test uses the hand-edited values.
        var cfg = AppSettingsManager.Load();
        var token = cfg.TelegramBotToken.Trim();
        var chatId = cfg.TelegramChatId.Trim();
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(chatId))
        {
            MessageBox.Show(
                "В appsettings.json (раздел \"Telegram\") не заполнены BotToken и/или ChatId.",
                "Telegram", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _btnTelegramTest.Enabled = false;
        try
        {
            using var http = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var url = $"https://api.telegram.org/bot{token}/sendMessage";
            var payload = new { chat_id = chatId, text = $"✅ eCash Tablo: тестовое сообщение ({_cfg.ActivePointId})." };
            using var resp = await http.PostAsJsonAsync(url, payload);
            if (resp.IsSuccessStatusCode)
                MessageBox.Show("Сообщение отправлено. Проверьте Telegram.", "Telegram",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            {
                var body = await resp.Content.ReadAsStringAsync();
                MessageBox.Show($"Не удалось отправить: HTTP {(int)resp.StatusCode}.\n{body}", "Telegram",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка отправки: {ex.Message}", "Telegram",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _btnTelegramTest.Enabled = true;
        }
    }

    // ─── Tab: Журнал ──────────────────────────────────────────────────────────

    private TabPage BuildLogTab()
    {
        var tab = new TabPage("Журнал") { Padding = new Padding(8) };

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 46, WrapContents = false, Padding = new Padding(0, 2, 0, 6) };
        var btnRefresh = MakeButton("⟳  Обновить (F5)", UITheme.Accent2, Color.White);
        btnRefresh.Width = 150;
        btnRefresh.Click += (_, _) => _ = RefreshLogAsync();

        var btnCopy = MakeButton("⧉  Копировать", UITheme.Input, UITheme.Text);
        btnCopy.Width = 130;
        btnCopy.Click += (_, _) =>
        {
            if (string.IsNullOrEmpty(_rtbLog.Text)) return;
            Clipboard.SetText(_rtbLog.Text);
            _lblLogState.Text = "Скопировано в буфер обмена";
        };

        _chkLogErrorsOnly = new CheckBox { Text = "Только ошибки", AutoSize = true, Tag = NoTrackTag, Margin = new Padding(14, 9, 0, 0) };
        _chkLogErrorsOnly.CheckedChanged += (_, _) => RenderLog();
        _chkLogAuto = new CheckBox { Text = "Автообновление (5 сек)", AutoSize = true, Tag = NoTrackTag, Margin = new Padding(14, 9, 0, 0) };
        _logTimer = new System.Windows.Forms.Timer { Interval = 5000 };
        _logTimer.Tick += (_, _) => { if (Visible && _tabs.SelectedTab == tab) _ = RefreshLogAsync(); };
        _chkLogAuto.CheckedChanged += (_, _) => _logTimer.Enabled = _chkLogAuto.Checked;

        _lblLogState = new Label { AutoSize = true, ForeColor = UITheme.TextDim, Margin = new Padding(14, 11, 0, 0) };

        foreach (var b in new Control[] { btnRefresh, btnCopy }) { b.Height = 34; b.Margin = new Padding(0, 0, 8, 0); }
        toolbar.Controls.AddRange([btnRefresh, btnCopy, _chkLogErrorsOnly, _chkLogAuto, _lblLogState]);

        _rtbLog = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            BackColor = UITheme.Panel,
            ForeColor = UITheme.Text,
            Font = new Font("Consolas", 9f),
            ScrollBars = RichTextBoxScrollBars.Both,
            WordWrap = false,
            DetectUrls = false,
        };

        tab.Controls.Add(_rtbLog);
        tab.Controls.Add(toolbar);
        return tab;
    }

    private CheckBox _chkLogErrorsOnly = null!, _chkLogAuto = null!;
    private Label _lblLogState = null!;
    private System.Windows.Forms.Timer _logTimer = null!;
    private List<LogLine> _logLines = [];
    private bool _logLoading;

    private sealed record LogLine(DateTimeOffset Timestamp, string Level, string Category, string Message);

    private async Task RefreshLogAsync()
    {
        if (_logLoading) return;
        _logLoading = true;
        try
        {
            var uri = _cfg.BoardUrls.TrimEnd('/') + "/api/led/logs?count=300";
            using var http = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(3) };
            var json = await http.GetStringAsync(uri);
            _logLines = System.Text.Json.JsonSerializer.Deserialize<List<LogLine>>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
            _lblLogState.ForeColor = UITheme.TextDim;
            _lblLogState.Text = $"Обновлено {DateTime.Now:HH:mm:ss} · записей: {_logLines.Count}";
            RenderLog();
        }
        catch (Exception ex)
        {
            _logLines = [];
            _rtbLog.Clear();
            _rtbLog.SelectionColor = Color.Salmon;
            _rtbLog.AppendText($"Не удалось загрузить журнал: {ex.Message}\n\nУбедитесь, что сервис запущен (значок в трее).");
            _lblLogState.ForeColor = Color.Salmon;
            _lblLogState.Text = "Сервис недоступен";
        }
        finally { _logLoading = false; }
    }

    // Newest first, one line per record, coloured by level so problems stand out.
    private void RenderLog()
    {
        bool errorsOnly = _chkLogErrorsOnly.Checked;
        var lines = _logLines
            .Where(l => !errorsOnly || l.Level is "Warning" or "Error" or "Critical")
            .OrderByDescending(l => l.Timestamp);

        _rtbLog.SuspendLayout();
        _rtbLog.Clear();
        foreach (var l in lines)
        {
            var (tag, color) = l.Level switch
            {
                "Error" or "Critical" => ("ОШИБКА", Color.FromArgb(240, 97, 109)),
                "Warning" => ("ВНИМ. ", Color.FromArgb(251, 191, 36)),
                "Debug" or "Trace" => ("отладк", UITheme.TextDim),
                _ => ("инфо  ", UITheme.Text),
            };
            _rtbLog.SelectionColor = UITheme.TextDim;
            _rtbLog.AppendText($"{l.Timestamp.ToLocalTime():dd.MM HH:mm:ss}  ");
            _rtbLog.SelectionColor = color;
            _rtbLog.AppendText($"{tag}  {l.Message}\n");
        }
        if (_rtbLog.TextLength == 0)
        {
            _rtbLog.SelectionColor = UITheme.TextDim;
            _rtbLog.AppendText(errorsOnly ? "Ошибок и предупреждений нет 👍" : "Журнал пуст.");
        }
        _rtbLog.SelectionStart = 0;
        _rtbLog.ScrollToCaret();
        _rtbLog.ResumeLayout();
    }

    // ─── Tab: Вики ────────────────────────────────────────────────────────────

    // A built-in help/wiki: a navigation list of section headings on the left and
    // a formatted text pane on the right. Selecting a heading jumps to that block.
    private TabPage BuildWikiTab()
    {
        var tab = new TabPage("📖 Вики") { Padding = new Padding(8) };

        // Navigation (left): list of section headings
        var navHost = new Panel { Dock = DockStyle.Left, Width = 190 };
        var navTitle = new Label
        {
            Text = "РАЗДЕЛЫ",
            Dock = DockStyle.Top,
            Height = 22,
            ForeColor = UITheme.Accent,
            Font = BoldFont,
        };
        _lstWikiNav = new ListBox
        {
            Dock = DockStyle.Fill,
            Font = UIFont,
            BorderStyle = BorderStyle.FixedSingle,
            IntegralHeight = false,
        };
        foreach (var section in WikiSections)
            _lstWikiNav.Items.Add(section.Title);
        _lstWikiNav.SelectedIndexChanged += (_, _) => ShowWikiSection(_lstWikiNav.SelectedIndex);
        navHost.Controls.Add(_lstWikiNav);
        navHost.Controls.Add(navTitle);

        var spacer = new Panel { Dock = DockStyle.Left, Width = 8 };

        // Content (right): formatted article
        _rtbWiki = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = UITheme.Panel,
            ForeColor = UITheme.Text,
            Font = new Font("Segoe UI", 9.5f),
            ScrollBars = RichTextBoxScrollBars.Vertical,
            DetectUrls = false,
        };

        tab.Controls.Add(_rtbWiki);
        tab.Controls.Add(spacer);
        tab.Controls.Add(navHost);

        if (_lstWikiNav.Items.Count > 0)
            _lstWikiNav.SelectedIndex = 0;
        return tab;
    }

    private void ShowWikiSection(int index)
    {
        if (index < 0 || index >= WikiSections.Length) return;
        RenderWikiMarkup(_rtbWiki, WikiSections[index].Title, WikiSections[index].Body);
    }

    // Tiny markup renderer:
    //   "## "  → sub-heading (bold accent)
    //   "- "   → bullet
    //   "1." …  → kept as-is (numbered step)
    //   blank  → spacing
    internal static void RenderWikiMarkup(RichTextBox rtb, string title, string body)
    {
        rtb.Clear();

        var headFont = new Font("Segoe UI Semibold", 14f, FontStyle.Bold);
        var subFont = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold);
        var bodyFont = new Font("Segoe UI", 9.5f);

        void Append(string text, Font font, Color color)
        {
            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionLength = 0;
            rtb.SelectionFont = font;
            rtb.SelectionColor = color;
            rtb.AppendText(text);
        }

        Append(title + "\n\n", headFont, UITheme.Accent);

        foreach (var raw in body.Replace("\r\n", "\n").Split('\n'))
        {
            var line = raw.TrimEnd();
            if (line.Length == 0)
            {
                Append("\n", bodyFont, UITheme.Text);
            }
            else if (line.StartsWith("## "))
            {
                Append("\n" + line[3..] + "\n", subFont, UITheme.Accent2);
            }
            else if (line.StartsWith("- "))
            {
                Append("   •  " + line[2..] + "\n", bodyFont, UITheme.Text);
            }
            else
            {
                Append(line + "\n", bodyFont, UITheme.Text);
            }
        }

        rtb.SelectionStart = 0;
        rtb.ScrollToCaret();
    }

    private readonly record struct WikiSection(string Title, string Body);

    private static readonly WikiSection[] WikiSections =
    [
        new("Обзор",
            """
            eCash Tablo — программа для автоматического вывода курсов валют на
            светодиодное табло (контроллеры Onbon BX-Y).

            Приложение живёт в системном трее (значок у часов). В фоне работает
            сервис, который по расписанию берёт свежие курсы, рисует картинку по
            вашей разметке и отправляет её на табло по сети.

            ## Как это работает (поток данных)
            - 1. Курсы валют берутся из API (или из файла rates.json).
            - 2. По разметке рисуется изображение final.jpg.
            - 3. Изображение отправляется на контроллер табло (FTP / Wi-Fi).
            - 4. Цикл повторяется через заданный интервал.

            ## Где что настраивается
            Каждая вкладка вверху отвечает за свой этап: «Валюты» и «Заголовки» —
            содержимое; «Размер табло» и «Дизайн» — внешний вид; «Сервис»,
            «Подключение», «Дополнительно» — работа сервиса и связь с табло;
            «Журнал» — что происходит сейчас.
            """),

        new("С чего начать (по шагам)",
            """
            Минимальная настройка нового табло — сверху вниз по вкладкам:

            ## Шаг 1 — Валюты
            Откройте вкладку «Валюты». Выберите нужные валюты из списка слева и
            кнопкой «→ Доб» добавьте их в колонку. Порядок в списке = порядок на
            табло (меняется стрелками ▲▼).

            ## Шаг 2 — Заголовки
            На вкладке «Заголовки» задайте подписи «Покупаем» и «Продаём» (можно
            на нескольких языках — по строке на язык).

            ## Шаг 3 — Размер табло
            На вкладке «Размер табло» укажите ширину и высоту в пикселях — ровно
            как у физического табло.

            ## Шаг 4 — Дизайн
            На вкладке «Дизайн» нажмите «Раскидка по размеру», поправьте блоки
            мышью при необходимости и нажмите «Обновить превью».

            ## Шаг 5 — Подключение
            На вкладке «Подключение» введите IP и порт контроллера, выберите
            модель, данные FTP. Нажмите «Проверить соединение».

            ## Шаг 6 — Сохранить
            Внизу окна нажмите «Сохранить и перезапустить». Настройки применятся,
            сервис перезапустится и начнёт обновлять табло.
            """),

        new("Активная точка",
            """
            Выпадающий список вверху окна.

            Одно приложение может обслуживать несколько точек (несколько табло).
            Каждая точка хранит свои настройки и свою разметку отдельно.

            При переключении точки всё окно перезагружает её конфиг — вы
            редактируете именно выбранную точку. Сохранение тоже относится только
            к активной точке.
            """),

        new("Валюты",
            """
            Что показывать на табло и в каком порядке.

            ## Колонки
            Поле «Колонок» (1–3) задаёт число колонок на табло. Слева — все
            доступные валюты, справа — колонки с выбранными валютами.

            ## Как добавить/убрать
            - Выделите валюту слева, выберите номер колонки в «Добавить в колонку»
              и нажмите «→ Доб».
            - Чтобы убрать — выделите валюту в колонке и нажмите «← Уб».
            - Порядок внутри колонки меняется стрелками ▲▼ (это порядок строк на
              табло).
            """),

        new("Заголовки",
            """
            Подписи столбцов курса для каждой колонки: «Покупаем» и «Продаём».

            Для каждой колонки — два поля. В каждом поле можно указать несколько
            строк — по строке на язык (например: казахский / русский / английский).
            На табло они выводятся над соответствующим столбцом.

            Число колонок здесь совпадает с числом на вкладке «Валюты».
            """),

        new("Размер табло",
            """
            Размер холста (изображения) в пикселях.

            Ширина и высота должны точно совпадать с физическим разрешением табло
            и с настройками контроллера (ScreenWidth/ScreenHeight на вкладке
            «Подключение»). Если размер не совпадает, картинка отобразится
            неправильно или будет отклонена контроллером.
            """),

        new("Дизайн",
            """
            Визуальный редактор разметки и предпросмотр.

            ## Редактор (слева)
            Перетаскивайте блоки мышью (лого, флаги, коды, столбцы курса).
            Уголок выделенного блока — изменение размера.

            ## Кнопки (справа)
            - «Раскидка по размеру» — автоматически расставляет блоки под текущий
              размер табло и число валют.
            - «Обновить превью» — рисует реальное изображение по текущим курсам.
            - «Загрузить курсы из API» — тянет свежие курсы, чтобы превью было
              актуальным.
            - «Авто-обновление при правках» — превью перерисовывается само при
              изменениях.

            ## Шрифты, флаг/лого, строки
            Числовые поля точно настраивают размеры шрифтов (курс, код, заголовок,
            стрелка), размеры флага и лого, старт и высоту строк.

            ## Размещение колонок (X)
            «Задавать X колонок вручную» — свободная раскладка (например, лого по
            центру, курсы по бокам). Кнопка «Лого по центру» — готовый пресет для
            широких табло.

            ## Отправить на табло
            Кнопка «Отправить на табло» вручную отправляет текущую картинку на
            контроллер. Нужна для точек без постоянного интернета: снимите галочку
            «Интернет постоянный (автоотправка)» и отправляйте вручную.

            Важно: превью рисуется во временные файлы и НЕ трогает табло, пока вы
            не нажмёте «Сохранить» или «Отправить на табло».
            """),

        new("Сервис",
            """
            Как работает фоновый сервис.

            ## Режим работы
            - RenderOnly — только рисует картинку, на табло не отправляет.
            - Uploader — только отправляет уже готовую картинку.
            - Full — полный цикл: рисует и отправляет.

            ## Режим публикации
            - WifiFtp — отправка на контроллер по FTP через Wi-Fi.
            - WifiRelay — отправка через релей/посредника.

            ## Интервалы
            - «Интервал опроса» — как часто выполняется рабочий цикл (сек).
            - «Обновление курсов» — как часто тянутся свежие курсы (мин).

            ## Галочки
            - «Режим тестирования» — ничего не отправлять на табло.
            - «Автоотправка на табло» — отправлять без ручного подтверждения.
            - «Пропускать без изменений» — не слать кадр, если он не изменился.
            - «Перерисовывать каждый цикл» — всегда заново рисовать картинку.
            """),

        new("Подключение",
            """
            Связь с контроллером табло.

            ## Проверка соединения
            Кнопка «Проверить соединение» опрашивает контроллер и показывает,
            онлайн он или нет.

            ## Питание табло
            Кнопки «Включить» / «Выключить» / «Перезагрузить» управляют табло.
            Требуется запущенный сервис и связь с контроллером.

            ## Контроллер LED
            - IP-адрес и порт контроллера.
            - Wi-Fi SSID табло.
            - Модель контроллера — выбор из списка автоматически подставляет «Код
              устройства». «Другой (вручную)» разблокирует поле кода.
            - Тип подключения.

            ## FTP
            Логин, пароль и порт FTP, опция TLS. По FTP на контроллер
            загружается картинка.

            ## API
            - URL API курсов — откуда брать курсы.
            - URL перезагрузки — адрес для перезагрузки контента.
            - Адрес сервиса (URL) — локальный адрес сервиса (для превью, журнала,
              питания).
            """),

        new("Дополнительно",
            """
            Тонкие настройки SDK, надёжности и интеграций.

            ## SDK (Onbon)
            - «SDK включён» — использовать нативный SDK Onbon (только Windows).
            - Логин/пароль SDK.
            - «Изолированный процесс SDK» — запускать отправку в отдельном
              процессе (стабильнее, защищает от зависаний нативного кода).

            ## Надёжность
            - Повторных попыток, задержка попытки, таймаут подключения.
            - «Пропускать дублирующиеся изображения».
            - «Отклонять несовпадение размера» — защита от неверного разрешения.
            - «Только через Wi-Fi адаптер» / «Требовать приватный IP» — ограничивают
              отправку безопасной локальной сетью.

            ## Telegram-уведомления
            Token, Chat ID и включение задаются вручную в appsettings.json (раздел
            «Telegram»). Кнопка «Тест Telegram» проверяет отправку.

            ## Обновление приложения
            «Проверить обновления» — загрузка новой версии с GitHub. Настройки и
            разметка при обновлении сохраняются.
            """),

        new("Журнал",
            """
            Что сервис делает прямо сейчас.

            Кнопка «Обновить журнал» загружает последние записи лога через
            локальный REST API сервиса. Здесь видно циклы рендера, отправку на
            табло, ошибки связи и т.п.

            Если журнал не загружается — убедитесь, что сервис запущен (значок в
            трее) и порт REST API на вкладке «Подключение» указан верно.
            """),

        new("Сохранение и перезапуск",
            """
            Кнопки внизу окна.

            - «Сохранить и перезапустить» — сохраняет настройки и перезапускает
              сервис, чтобы изменения вступили в силу немедленно.
            - «Сохранить» — только сохраняет; изменения применятся после
              перезапуска сервиса.
            - «Закрыть» — прячет окно (приложение продолжает работать в трее).

            Большинство настроек требуют перезапуска сервиса, поэтому обычно
            используйте «Сохранить и перезапустить».
            """),
    ];

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private bool _populatingForm;

    private void PopulateForm()
    {
        if (_populatingForm) return;
        _populatingForm = true;
        try { PopulateFormCore(); }
        finally { _populatingForm = false; }
        UpdateBoardChips();
        SetDirty(_assignmentChanged);
    }

    // ─── Two boards ───────────────────────────────────────────────────────────

    // True while the board assignment (which point is board 1 / board 2) differs from what is saved.
    private bool _assignmentChanged;

    private void UpdateBoardChips()
    {
        _btnBoard1.Text = $"Табло 1 · {_cfg.PrimaryPointId}";
        _btnBoard2.Text = _cfg.HasSecondBoard ? $"Табло 2 · {_cfg.SecondPointId}" : "＋  Добавить второе табло";

        void Style(RoundedButton b, bool selected, bool ghost)
        {
            b.BackColor = selected ? UITheme.Accent2 : UITheme.Input;
            b.ForeColor = selected ? Color.White : ghost ? UITheme.Accent : UITheme.Text;
            b.Invalidate();
        }
        Style(_btnBoard1, _cfg.BoardIndex == 0, false);
        Style(_btnBoard2, _cfg.BoardIndex == 1, !_cfg.HasSecondBoard);
        _btnRemoveSecond.Visible = _cfg.BoardIndex == 1 && _cfg.HasSecondBoard;
    }

    // Asks what to do with unsaved edits before the form is reloaded for another board/point.
    private bool ConfirmLeaveBoard()
    {
        if (!_dirty) return true;
        var answer = MessageBox.Show(this,
            $"Есть несохранённые изменения для точки «{_cfg.ActivePointId}».\n\nСохранить их перед переключением?",
            "eCash Tablo", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        if (answer == DialogResult.Cancel) return false;
        return answer != DialogResult.Yes || SaveOnly();
    }

    private void ReloadBoard(int boardIndex, string primary, string second)
    {
        _cfg = AppSettingsManager.Load(boardIndex, primary, second);
        PopulateForm();
    }

    private void SelectBoard(int index)
    {
        if (index == _cfg.BoardIndex) return;
        if (index == 1 && !_cfg.HasSecondBoard) { AddSecondBoard(); return; }
        if (!ConfirmLeaveBoard()) return;
        ReloadBoard(index, _cfg.PrimaryPointId, _cfg.SecondPointId);
    }

    private void AddSecondBoard()
    {
        var free = AppSettingsManager.GetAvailablePoints()
            .Where(p => !p.Equals(_cfg.PrimaryPointId, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (free.Length == 0)
        {
            MessageBox.Show(this, "Нет другой точки для второго табло. Добавьте точку (см. ADDING_POINT.md).",
                "eCash Tablo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (!ConfirmLeaveBoard()) return;

        _assignmentChanged = true;
        ReloadBoard(1, _cfg.PrimaryPointId, free[0]);
        _lblSaveState.ForeColor = UITheme.TextDim;
        MessageBox.Show(this,
            $"Второе табло добавлено: точка «{free[0]}».\n\n" +
            "Выберите нужную точку в списке «Точка», настройте IP и размер экрана на вкладке «Подключение», " +
            "затем нажмите «Сохранить и перезапустить».",
            "eCash Tablo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void RemoveSecondBoard()
    {
        if (MessageBox.Show(this,
                $"Отключить второе табло (точка «{_cfg.SecondPointId}»)?\n\nНастройки этой точки сохранятся — её можно подключить снова.",
                "eCash Tablo", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        if (!ConfirmLeaveBoard()) return;

        _assignmentChanged = true;
        ReloadBoard(0, _cfg.PrimaryPointId, "");
    }

    // The point list of the board being edited changed.
    private void OnBoardPointChosen()
    {
        if (_revertingPoint || _populatingForm) return;
        var chosen = _cmbPoint.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(chosen) || chosen == _cfg.ActivePointId) return;

        void Revert()
        {
            _revertingPoint = true;
            _cmbPoint.SelectedItem = _cfg.ActivePointId;
            _revertingPoint = false;
        }

        var other = _cfg.BoardIndex == 0 ? _cfg.SecondPointId : _cfg.PrimaryPointId;
        if (!string.IsNullOrEmpty(other) && chosen.Equals(other, StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(this, $"Точка «{chosen}» уже используется другим табло.",
                "eCash Tablo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Revert();
            return;
        }
        if (!ConfirmLeaveBoard()) { Revert(); return; }

        _assignmentChanged = true;
        ReloadBoard(_cfg.BoardIndex,
            _cfg.BoardIndex == 0 ? chosen : _cfg.PrimaryPointId,
            _cfg.BoardIndex == 1 ? chosen : _cfg.SecondPointId);
    }

    // ─── Save / unsaved changes / shortcuts ───────────────────────────────────

    private Label _lblSaveState = null!;
    private bool _dirty;
    private bool _revertingPoint;
    private bool _changeTrackingAttached;

    /// <summary>
    /// Hooks every settings input so any edit flips the "unsaved changes" marker.
    /// Controls that are view-only preferences (auto preview, log refresh…) are skipped.
    /// </summary>
    private void AttachChangeTracking(Control root)
    {
        foreach (Control c in root.Controls)
        {
            if (c.Tag as string == NoTrackTag || c == _cmbPoint) continue;
            switch (c)
            {
                case NumericStepper n: n.ValueChanged += (_, _) => MarkDirty(); break;
                case TextBox tb when !tb.ReadOnly: tb.TextChanged += (_, _) => MarkDirty(); break;
                case CheckBox chk: chk.CheckedChanged += (_, _) => MarkDirty(); break;
                case ComboBox cb: cb.SelectedIndexChanged += (_, _) => MarkDirty(); break;
                case TrackBar tr: tr.ValueChanged += (_, _) => MarkDirty(); break;
            }
            if (c is not NumericStepper && c.HasChildren) AttachChangeTracking(c);
        }
    }

    private const string NoTrackTag = "no-track";

    private void MarkDirty()
    {
        if (_populatingForm || !_changeTrackingAttached) return;
        SetDirty(true);
    }

    private void SetDirty(bool dirty)
    {
        _dirty = dirty;
        if (_lblSaveState == null) return;
        if (dirty)
        {
            _lblSaveState.ForeColor = Color.FromArgb(251, 191, 36);
            _lblSaveState.Text = "●  Есть несохранённые изменения  (Ctrl+S — сохранить)";
        }
        else if (_lblSaveState.Text.StartsWith('●'))
        {
            _lblSaveState.Text = "";
        }
    }

    private void ShowSaved(string text)
    {
        _lblSaveState.ForeColor = UITheme.Success;
        _lblSaveState.Text = text;
    }

    // With two boards the controllers must differ — otherwise one board would receive the other's picture.
    private bool ConfirmDistinctIps()
    {
        if (!_cfg.HasSecondBoard) return true;
        var otherPoint = _cfg.BoardIndex == 0 ? _cfg.SecondPointId : _cfg.PrimaryPointId;
        var otherIp = Services.BoardTopology.ControllerIpOf(AppContext.BaseDirectory, otherPoint);
        if (string.IsNullOrEmpty(otherIp)
            || !string.Equals(otherIp, _cfg.ControllerIp?.Trim(), StringComparison.OrdinalIgnoreCase))
            return true;

        return MessageBox.Show(this,
            $"У табло «{_cfg.ActivePointId}» и «{otherPoint}» один и тот же IP контроллера: {otherIp}.\n\n" +
            "Картинка одного табло может уйти на другое, поэтому второе табло при таком IP не запустится.\n\n" +
            "Сохранить всё равно?",
            "eCash Tablo — одинаковый IP", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2) == DialogResult.Yes;
    }

    private bool SaveOnly()
    {
        if (!CollectForm() || !ConfirmDistinctIps()) return false;
        AppSettingsManager.Save(_cfg);
        _assignmentChanged = false;
        SetDirty(false);
        ShowSaved($"✓  Сохранено в {DateTime.Now:HH:mm}. Применится после перезапуска сервиса.");
        return true;
    }

    private void SaveAndRestart()
    {
        if (!CollectForm() || !ConfirmDistinctIps()) return;
        AppSettingsManager.Save(_cfg);
        _assignmentChanged = false;
        SetDirty(false);
        Hide();
        _onRestart();
    }

    /// <summary>Asks what to do with unsaved edits. Returns false when the user cancels.</summary>
    private bool ConfirmDiscardChanges()
    {
        if (!_dirty) return true;
        var answer = MessageBox.Show(this,
            "Есть несохранённые изменения.\n\nСохранить их?",
            "eCash Tablo", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        if (answer == DialogResult.Cancel) return false;
        if (answer == DialogResult.Yes) return SaveOnly();
        // "No": throw the edits away so the next open shows what is really saved.
        _assignmentChanged = false;
        _cfg = AppSettingsManager.Load();
        PopulateForm();
        return true;
    }

    private void OnFormKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.S)
        {
            if (e.Shift) SaveAndRestart(); else SaveOnly();
            e.Handled = e.SuppressKeyPress = true;
            return;
        }
        if (e.KeyCode == Keys.Escape && !e.Control && !e.Shift)
        {
            Close();
            e.Handled = e.SuppressKeyPress = true;
            return;
        }
        if (e.KeyCode == Keys.F5)
        {
            if (_tabs.SelectedTab == _designTab) _ = RenderPreviewAsync(silent: false);
            else if (_tabs.SelectedIndex == 7) _ = RefreshLogAsync();
            e.Handled = true;
            return;
        }
        // Ctrl+1 … Ctrl+9 jump straight to a page
        if (e.Control && e.KeyCode is >= Keys.D1 and <= Keys.D9)
        {
            int idx = e.KeyCode - Keys.D1;
            if (idx < _tabs.TabPages.Count) _tabs.SelectedIndex = idx;
            e.Handled = e.SuppressKeyPress = true;
        }
    }

    private void PopulateFormCore()
    {
        // Point
        var idx = _cmbPoint.Items.IndexOf(_cfg.ActivePointId);
        if (idx >= 0) _cmbPoint.SelectedIndex = idx;
        else if (_cmbPoint.Items.Count > 0) _cmbPoint.SelectedIndex = 0;

        // Currencies
        _cfg.NormalizeColumns();
        _numColumnCount.Value = Math.Clamp(_cfg.ColumnCount, 1, MaxColumns);
        ApplyColumnCount(_cfg.ColumnCount);
        for (int i = 0; i < MaxColumns; i++)
        {
            _lstColumns[i].Items.Clear();
            if (i < _cfg.Columns.Count)
                foreach (var code in _cfg.Columns[i])
                    _lstColumns[i].Items.Add(FormatCurrency(code));
        }

        // Header labels (per column)
        for (int i = 0; i < MaxColumns; i++)
        {
            _txtBuyLabels[i].Text = string.Join(Environment.NewLine,
                i < _cfg.ColumnBuyLabels.Count ? _cfg.ColumnBuyLabels[i] : AppConfig.DefaultBuyLabels);
            _txtSellLabels[i].Text = string.Join(Environment.NewLine,
                i < _cfg.ColumnSellLabels.Count ? _cfg.ColumnSellLabels[i] : AppConfig.DefaultSellLabels);
        }

        // Per-column X placement
        _suppressSync = true;
        _chkManualColX.Checked = _cfg.ColumnX.Any(x => x.HasValue);
        for (int i = 0; i < MaxColumns; i++)
            _numColX[i].Value = Math.Clamp(_cfg.ColumnX.ElementAtOrDefault(i) ?? 0, 0, 4096);
        _suppressSync = false;
        ApplyColManualEnabled();

        // Display
        _numW.Value = Math.Clamp(_cfg.CanvasWidth, 8, 4096);
        _numH.Value = Math.Clamp(_cfg.CanvasHeight, 8, 4096);

        // Service
        _cmbRunMode.SelectedItem = _cfg.RunMode;
        if (_cmbRunMode.SelectedIndex < 0) _cmbRunMode.SelectedIndex = 0;
        _cmbPublishMode.SelectedItem = _cfg.PublishMode;
        if (_cmbPublishMode.SelectedIndex < 0) _cmbPublishMode.SelectedIndex = 0;
        _numPoll.Value = Math.Clamp(_cfg.PollSeconds, 5, 3600);
        _numRatesFetch.Value = Math.Clamp(_cfg.RatesFetchIntervalMinutes, 1, 120);
        _chkLayout.Checked = _cfg.LayoutTestMode;
        _chkPermanentInternet.Checked = _cfg.PermanentInternet;
        _chkAutoSend.Checked = _cfg.AutoSend;
        _chkSkipUnchanged.Checked = _cfg.SkipIfUnchanged;
        _chkForceCompose.Checked = _cfg.ForceComposeEveryPoll;

        // Connection
        _cmbFamily.SelectedIndex = ControllerFamilyCatalog.IndexOfKey(_cfg.ControllerFamily);
        ApplyFamilyVisibility();
        _txtIp.Text = _cfg.ControllerIp;
        _txtWifiSsid.Text = _cfg.WifiSsid;
        _numCtrlPort.Value = Math.Clamp(_cfg.ControllerPort, 1, 65535);
        _numDevice.Value = Math.Clamp(_cfg.DeviceType, 1000, 65535);
        SyncModelFromDeviceType();
        _cmbConnMode.SelectedIndex = ConnectionModeCatalog.Modes.ToList()
            .IndexOf(ConnectionModeCatalog.FindByKey(_cfg.ConnectionMode));
        _txtFtpUser.Text = _cfg.FtpUser;
        _txtFtpPass.Text = _cfg.FtpPassword;
        _numFtpPort.Value = Math.Clamp(_cfg.FtpPort, 1, 65535);
        _chkTls.Checked = _cfg.UseTls;
        _txtRatesUrl.Text = _cfg.RatesApiUrl;
        _txtReloadUrl.Text = _cfg.ControllerReloadUrl;
        // Board 2's address is derived (board 1's port + 1) and not edited here.
        _txtApiPort.Text = _cfg.BoardUrls;
        _txtApiPort.Enabled = _cfg.BoardIndex == 0;

        // Advanced
        _chkOnbonEnabled.Checked = _cfg.OnbonEnabled;
        _txtOnbonUser.Text = _cfg.OnbonUserName;
        _txtOnbonPass.Text = _cfg.OnbonPassword;
        _numRetry.Value = Math.Clamp(_cfg.RetryCount, 0, 10);
        _numRetryMs.Value = Math.Clamp(_cfg.RetryDelayMs, 100, 30000);
        _numConnTimeout.Value = Math.Clamp(_cfg.ConnectionTimeoutMs, 100, 30000);
        _numOnbonPoll.Value = Math.Clamp(_cfg.OnbonPollSeconds, 5, 3600);
        _chkIsolated.Checked = _cfg.UseIsolatedSender;
        _chkSkipDup.Checked = _cfg.SkipDuplicateUploads;
        _chkRejectSize.Checked = _cfg.RejectSizeMismatchBeforePublish;
        _chkWifiOnly.Checked = _cfg.EnforceWifiOnly;
        _chkPrivate.Checked = _cfg.RequirePrivateAddress;

        // Design
        SyncDesignNumericsFromConfig();
        _editor.SetBackground(null);
        _editor.Bind(_cfg);

        // Programmatically setting NumericUpDown.Value resets its internal TextBox to the
        // system default colors — re-apply the theme so it doesn't flash white.
        UITheme.Apply(this);
    }

    private void PullCurrencies()
    {
        _cfg.ColumnCount = Math.Clamp((int)_numColumnCount.Value, 1, MaxColumns);
        _cfg.Columns = [];
        for (int i = 0; i < _cfg.ColumnCount; i++)
            _cfg.Columns.Add(_lstColumns[i].Items.Cast<string>().Select(s => s.Split(' ')[0]).ToList());

        // Per-column header labels (split textbox lines, drop blanks)
        _cfg.ColumnBuyLabels = [];
        _cfg.ColumnSellLabels = [];
        for (int i = 0; i < _cfg.ColumnCount; i++)
        {
            _cfg.ColumnBuyLabels.Add(SplitLabelLines(_txtBuyLabels[i].Text, AppConfig.DefaultBuyLabels));
            _cfg.ColumnSellLabels.Add(SplitLabelLines(_txtSellLabels[i].Text, AppConfig.DefaultSellLabels));
        }
        _cfg.NormalizeColumns();
    }

    private static List<string> SplitLabelLines(string text, List<string> fallback)
    {
        var lines = (text ?? "")
            .Replace("\r\n", "\n").Split('\n')
            .Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
        return lines.Count > 0 ? lines : fallback;
    }

    private void PullDisplay()
    {
        _cfg.CanvasWidth = (int)_numW.Value;
        _cfg.CanvasHeight = (int)_numH.Value;
        _cfg.ScreenWidth = _cfg.CanvasWidth;
        _cfg.ScreenHeight = _cfg.CanvasHeight;
    }

    private bool CollectForm()
    {
        // Currencies
        PullCurrencies();

        if (_cfg.AllCurrencies.Count == 0)
        {
            MessageBox.Show("Выберите хотя бы одну валюту.", "eCash Tablo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        // Display
        PullDisplay();
        PullColumnX();

        // Service
        _cfg.RunMode = _cmbRunMode.SelectedItem?.ToString() ?? "RenderOnly";
        _cfg.PublishMode = _cmbPublishMode.SelectedItem?.ToString() ?? "WifiFtp";
        _cfg.PollSeconds = (int)_numPoll.Value;
        _cfg.RatesFetchIntervalMinutes = (int)_numRatesFetch.Value;
        _cfg.LayoutTestMode = _chkLayout.Checked;
        _cfg.PermanentInternet = _chkPermanentInternet.Checked;
        _cfg.AutoSend = _chkAutoSend.Checked;
        _cfg.SkipIfUnchanged = _chkSkipUnchanged.Checked;
        _cfg.ForceComposeEveryPoll = _chkForceCompose.Checked;

        // Connection
        if (_cmbFamily.SelectedIndex >= 0)
            _cfg.ControllerFamily = ControllerFamilyCatalog.Families[_cmbFamily.SelectedIndex].Key;
        _cfg.ControllerIp = _txtIp.Text.Trim();
        _cfg.WifiSsid = _txtWifiSsid.Text.Trim();
        _cfg.ControllerPort = (int)_numCtrlPort.Value;
        _cfg.DeviceType = (int)_numDevice.Value;
        if (_cmbConnMode.SelectedIndex >= 0)
            _cfg.ConnectionMode = ConnectionModeCatalog.Modes[_cmbConnMode.SelectedIndex].Key;
        _cfg.FtpUser = _txtFtpUser.Text;
        _cfg.FtpPassword = _txtFtpPass.Text;
        _cfg.FtpPort = (int)_numFtpPort.Value;
        _cfg.UseTls = _chkTls.Checked;
        _cfg.RatesApiUrl = _txtRatesUrl.Text.Trim();
        _cfg.ControllerReloadUrl = _txtReloadUrl.Text.Trim();
        if (_cfg.BoardIndex == 0) _cfg.Urls = _txtApiPort.Text.Trim();

        // Advanced
        _cfg.OnbonEnabled = _chkOnbonEnabled.Checked;
        _cfg.OnbonUserName = _txtOnbonUser.Text;
        _cfg.OnbonPassword = _txtOnbonPass.Text;
        _cfg.RetryCount = (int)_numRetry.Value;
        _cfg.RetryDelayMs = (int)_numRetryMs.Value;
        _cfg.ConnectionTimeoutMs = (int)_numConnTimeout.Value;
        _cfg.OnbonPollSeconds = (int)_numOnbonPoll.Value;
        _cfg.UseIsolatedSender = _chkIsolated.Checked;
        _cfg.SkipDuplicateUploads = _chkSkipDup.Checked;
        _cfg.RejectSizeMismatchBeforePublish = _chkRejectSize.Checked;
        _cfg.EnforceWifiOnly = _chkWifiOnly.Checked;
        _cfg.RequirePrivateAddress = _chkPrivate.Checked;

        return true;
    }

    private static string FormatCurrency(string code)
    {
        var name = AppSettingsManager.KnownCurrencies.TryGetValue(code, out var n) ? n : code;
        return $"{code}  {name}";
    }

    private static NumericStepper MakeNumeric(int min, int max)
        => new() { Minimum = min, Maximum = max, Width = 118, Height = 32 };

    // A clean section divider: uppercased caption followed by a hairline rule.
    private static Label MakeDivider(string text)
    {
        var lbl = new Label
        {
            Text = text,
            ForeColor = UITheme.TextDim,
            Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold),
            AutoSize = false,
            Height = 26,
            Width = 400,
            TextAlign = ContentAlignment.MiddleLeft,
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            Margin = new Padding(0, 14, 0, 6),
        };
        UITheme.PaintDivider(lbl);
        return lbl;
    }

    private static Button MakeButton(string text, Color bg, Color fg)
    {
        return new RoundedButton
        {
            Text = text,
            Height = 32,
            BackColor = bg,
            ForeColor = fg,
            Font = new Font("Segoe UI Semibold", 9f),
            CornerRadius = 9,
        };
    }

    private static Button MakeArrow(string text, Color bg, Color fg)
    {
        var btn = MakeButton(text, bg, fg);
        btn.Width = 60;
        btn.Height = 26;
        return btn;
    }

    private static Label? AddRow(TableLayoutPanel tbl, string label, Control ctrl)
    {
        int row = tbl.RowCount;
        tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tbl.RowCount = row + 1;

        Label? lbl = null;
        if (!string.IsNullOrEmpty(label))
        {
            lbl = new Label
            {
                Text = label,
                AutoSize = true,
                // Anchored left only → centred vertically against the input in the same row.
                Anchor = AnchorStyles.Left,
                ForeColor = UITheme.Text,
            };
            tbl.Controls.Add(lbl, 0, row);
        }

        ctrl.Anchor = AnchorStyles.Left;
        // Room for the rounded frame the theme paints around inputs.
        ctrl.Margin = new Padding(6, 6, 3, 6);
        tbl.Controls.Add(ctrl, 1, row);
        return lbl;
    }

    // Adds a control that spans both columns on its own row. Manages RowCount/RowStyles
    // the same way AddRow does — mixing manual Controls.Add(…, 0, RowCount) with AddRow
    // collides cells and scrambles the whole table, so every full-width add goes through here.
    private static void AddFullRow(TableLayoutPanel tbl, Control ctrl)
    {
        int row = tbl.RowCount;
        tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tbl.RowCount = row + 1;
        ctrl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        tbl.Controls.Add(ctrl, 0, row);
        tbl.SetColumnSpan(ctrl, 2);
    }

    // Show the Onbon controller rows. (This application supports the Onbon family only.)
    private void ApplyFamilyVisibility()
    {
        void Show(Label? lbl, Control ctrl)
        {
            if (lbl != null) lbl.Visible = true;
            ctrl.Visible = true;
        }

        Show(_lblIp, _pnlIp);
        Show(_lblCtrlPort, _numCtrlPort);
        Show(_lblModel, _cmbModel);
        Show(_lblDevice, _numDevice);
        Show(_lblConnMode, _cmbConnMode);
    }
}
