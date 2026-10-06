using System.Drawing.Drawing2D;

namespace LedImageUpdaterService.UI;

/// <summary>
/// TabControl without its header strip. Pages are switched from <see cref="SideNav"/>,
/// so the old two-row tab strip (whose rows swapped places on click) is gone.
/// </summary>
internal sealed class HeaderlessTabControl : TabControl
{
    private const int TCM_ADJUSTRECT = 0x1328;

    protected override void WndProc(ref Message m)
    {
        // Tell Windows the display area is the whole control — no room for tabs.
        if (m.Msg == TCM_ADJUSTRECT && !DesignMode)
        {
            m.Result = 1;
            return;
        }
        base.WndProc(ref m);
    }
}

/// <summary>
/// Vertical navigation for the settings window: grouped items with an icon, a hover
/// state and an accent bar on the active page. Keeps itself in sync with the tab
/// control, so code that selects a tab programmatically updates the menu too.
/// </summary>
internal sealed class SideNav : Panel
{
    private readonly TabControl _tabs;
    private readonly List<NavItem> _items = [];
    private int _nextY = 12;

    // Segoe MDL2 Assets ships with Windows 10+; older systems fall back to a dot.
    private static readonly Font? IconFont = TryIconFont();

    public SideNav(TabControl tabs)
    {
        _tabs = tabs;
        Dock = DockStyle.Left;
        Width = 196;
        BackColor = UITheme.Panel;
        AutoScroll = true;
        _tabs.SelectedIndexChanged += (_, _) => SyncSelection();

        Paint += (_, e) =>
        {
            using var pen = new Pen(UITheme.Border, 1f);
            e.Graphics.DrawLine(pen, Width - 1, 0, Width - 1, Height);
        };
    }

    public void AddGroup(string caption)
    {
        var lbl = new Label
        {
            Text = caption,
            AutoSize = false,
            Location = new Point(18, _nextY + (_items.Count > 0 ? 10 : 0)),
            Size = new Size(Width - 30, 20),
            ForeColor = UITheme.TextDim,
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI Semibold", 8f, FontStyle.Bold),
        };
        Controls.Add(lbl);
        _nextY = lbl.Bottom + 2;
    }

    /// <param name="icon">Segoe MDL2 Assets glyph.</param>
    /// <param name="hotkey">Shown dimmed on the right, e.g. "Ctrl+1".</param>
    public void AddItem(string icon, string text, TabPage page, string? hotkey = null)
    {
        var item = new NavItem(icon, text, hotkey, IconFont)
        {
            Location = new Point(8, _nextY),
            Size = new Size(Width - 18, 36),
            Page = page,
        };
        item.Click += (_, _) => _tabs.SelectedTab = page;
        Controls.Add(item);
        _items.Add(item);
        _nextY = item.Bottom + 2;
        SyncSelection();
    }

    private void SyncSelection()
    {
        // Before the handle exists SelectedIndex can still be -1; the first page is shown then.
        int sel = Math.Max(0, _tabs.SelectedIndex);
        var page = sel < _tabs.TabPages.Count ? _tabs.TabPages[sel] : null;
        foreach (var i in _items)
            i.Selected = i.Page == page;
    }

    private static Font? TryIconFont()
    {
        try
        {
            var f = new Font("Segoe MDL2 Assets", 11f);
            return f.Name == "Segoe MDL2 Assets" ? f : null;
        }
        catch { return null; }
    }

    private sealed class NavItem : Control
    {
        private readonly string _icon, _text;
        private readonly string? _hotkey;
        private readonly Font? _iconFont;
        private bool _hover, _selected;

        public TabPage? Page { get; init; }

        public bool Selected
        {
            get => _selected;
            set { if (_selected != value) { _selected = value; Invalidate(); } }
        }

        public NavItem(string icon, string text, string? hotkey, Font? iconFont)
        {
            _icon = icon; _text = text; _hotkey = hotkey; _iconFont = iconFont;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI", 9.5f);
            AccessibleName = text;
            AccessibleRole = AccessibleRole.PageTab;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(UITheme.Panel);

            var r = new Rectangle(0, 0, Width - 1, Height - 1);
            if (_selected || _hover)
            {
                using var path = RoundedButton.RoundedRect(r, 8);
                using var fill = new SolidBrush(_selected ? Color.FromArgb(40, UITheme.Accent2) : UITheme.Hover);
                g.FillPath(fill, path);
            }
            if (_selected)
            {
                using var bar = new SolidBrush(UITheme.Accent);
                using var bp = RoundedButton.RoundedRect(new Rectangle(0, 8, 3, Height - 16), 1);
                g.FillPath(bar, bp);
            }

            var fore = _selected ? Color.White : _hover ? UITheme.Text : UITheme.TextDim;
            var iconRect = new Rectangle(10, 0, 24, Height);
            if (_iconFont != null)
                TextRenderer.DrawText(g, _icon, _iconFont, iconRect, _selected ? UITheme.Accent : fore,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            else
                TextRenderer.DrawText(g, "•", Font, iconRect, fore,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            var textRect = new Rectangle(40, 0, Width - 44, Height);
            TextRenderer.DrawText(g, _text, Font, textRect, fore,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            if (!string.IsNullOrEmpty(_hotkey) && (_hover || _selected))
            {
                using var small = new Font("Segoe UI", 7.5f);
                TextRenderer.DrawText(g, _hotkey, small, new Rectangle(0, 0, Width - 8, Height),
                    UITheme.TextDim, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
            }
        }
    }
}
