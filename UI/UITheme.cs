using System.Drawing.Drawing2D;

namespace LedImageUpdaterService.UI;

/// <summary>
/// Modern dark theme: deep, desaturated slate surfaces layered by elevation,
/// a single confident teal accent with an electric-blue for primary actions,
/// generous rounding and hairline borders. Applied recursively so the individual
/// control builders stay simple.
/// </summary>
internal static class UITheme
{
    // Surfaces, from lowest to highest elevation.
    public static readonly Color Bg = Color.FromArgb(15, 18, 24);          // window background
    public static readonly Color Panel = Color.FromArgb(20, 25, 33);       // header / footer / bars
    public static readonly Color Card = Color.FromArgb(26, 32, 42);        // cards / group surfaces
    public static readonly Color Input = Color.FromArgb(32, 39, 51);       // text / list / stepper fields
    public static readonly Color InputDisabled = Color.FromArgb(24, 29, 38);
    public static readonly Color Hover = Color.FromArgb(44, 54, 70);       // subtle hover fill
    public static readonly Color Border = Color.FromArgb(46, 56, 72);      // hairline borders

    // Accents.
    public static readonly Color Accent = Color.FromArgb(45, 212, 191);    // teal (brand highlight)
    public static readonly Color Accent2 = Color.FromArgb(59, 130, 246);   // electric blue (primary)
    public static readonly Color Success = Color.FromArgb(52, 211, 153);
    public static readonly Color Danger = Color.FromArgb(240, 97, 109);

    // Text.
    public static readonly Color Text = Color.FromArgb(228, 232, 240);
    public static readonly Color TextDim = Color.FromArgb(146, 156, 173);

    /// <summary>Recursively applies the dark theme to a control tree.</summary>
    public static void Apply(Control root)
    {
        foreach (Control c in root.Controls)
        {
            // Custom controls that style themselves fully — don't touch their internals.
            if (c is NumericStepper || c is RoundedButton)
                continue;

            switch (c)
            {
                case TextBox tb:
                    tb.BackColor = Input; tb.ForeColor = Text; tb.BorderStyle = BorderStyle.None;
                    SetTextMargins(tb, 6, 6);
                    RegisterInput(tb);
                    break;
                case ListBox lb:
                    lb.BackColor = Input; lb.ForeColor = Text; lb.BorderStyle = BorderStyle.None;
                    RegisterInput(lb);
                    break;
                case ComboBox cb:
                    StyleCombo(cb);
                    RegisterInput(cb);
                    break;
                case NumericUpDown nud:
                    nud.BackColor = Input; nud.ForeColor = Text; nud.BorderStyle = BorderStyle.FixedSingle;
                    // WinForms NumericUpDown has an internal TextBox that ignores the parent BackColor on value change
                    foreach (Control child in nud.Controls)
                    { child.BackColor = Input; child.ForeColor = Text; }
                    break;
                case CheckBox chk:
                    StyleToggle(chk, round: false);
                    break;
                case RadioButton rb:
                    StyleToggle(rb, round: true);
                    break;
                case GroupBox gb:
                    gb.ForeColor = Accent; gb.BackColor = Color.Transparent;
                    break;
                case Button:
                    break; // styled by MakeButton
                case Label lbl:
                    lbl.BackColor = Color.Transparent;
                    if (lbl.ForeColor.ToArgb() == Color.Black.ToArgb() ||
                        lbl.ForeColor == SystemColors.ControlText)
                        lbl.ForeColor = Text;
                    break;
                case TabControl tc:
                    tc.BackColor = Bg;
                    break;
                case TabPage tp:
                    tp.BackColor = Bg; tp.ForeColor = Text;
                    break;
                case Panel p:
                    if (p.BackColor == SystemColors.Control) p.BackColor = Bg;
                    break;
            }

            if (c.HasChildren) Apply(c);
        }
    }

    // ─── Inputs: borderless, with a rounded frame painted by the parent ──────

    // Inputs whose rounded frame is painted by their parent.
    private static readonly HashSet<Control> Inputs = [];
    // Parents that already carry the frame-painting handler.
    private static readonly HashSet<Control> FramedParents = [];

    private const int EM_SETMARGINS = 0x00D3;
    private const int EC_LEFTMARGIN = 0x0001;
    private const int EC_RIGHTMARGIN = 0x0002;

    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    /// <summary>Inner horizontal padding of a borderless text box, so text doesn't touch the frame.</summary>
    private static void SetTextMargins(TextBox tb, int left, int right)
    {
        void ApplyMargins()
        {
            if (!tb.IsHandleCreated) return;
            try { SendMessage(tb.Handle, EM_SETMARGINS, EC_LEFTMARGIN | EC_RIGHTMARGIN, (right << 16) | left); }
            catch { /* cosmetic only */ }
        }
        if (tb.IsHandleCreated) ApplyMargins(); else tb.HandleCreated += (_, _) => ApplyMargins();
    }

    /// <summary>
    /// Marks a control as an input: its parent paints a rounded frame around it, tinted
    /// with the accent while the input has focus — so it is obvious where typing goes.
    /// </summary>
    private static void RegisterInput(Control input)
    {
        if (!Inputs.Add(input)) return;

        void Refresh() => input.Parent?.Invalidate(Rectangle.Inflate(input.Bounds, 6, 6), false);

        input.GotFocus += (_, _) => Refresh();
        input.LostFocus += (_, _) => Refresh();
        input.Enter += (_, _) => Refresh();
        input.Leave += (_, _) => Refresh();
        input.EnabledChanged += (_, _) => Refresh();
        input.VisibleChanged += (_, _) => input.Parent?.Invalidate();
        input.LocationChanged += (_, _) => input.Parent?.Invalidate();
        input.SizeChanged += (_, _) => input.Parent?.Invalidate();
        input.Disposed += (_, _) => Inputs.Remove(input);

        AttachFramePainter(input.Parent);
        input.ParentChanged += (_, _) => AttachFramePainter(input.Parent);
    }

    private static void AttachFramePainter(Control? parent)
    {
        if (parent is null || !FramedParents.Add(parent)) return;
        EnableDoubleBuffering(parent);

        parent.Paint += (s, e) =>
        {
            var host = (Control)s!;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            foreach (Control child in host.Controls)
            {
                if (!Inputs.Contains(child) || !child.Visible) continue;

                bool focused = child.Focused || child.ContainsFocus;
                var r = Rectangle.Inflate(child.Bounds, 4, 4);
                r.Width -= 1; r.Height -= 1;
                if (r.Width <= 0 || r.Height <= 0) continue;

                using var path = RoundedButton.RoundedRect(r, 7);
                using (var fill = new SolidBrush(child.Enabled ? Input : InputDisabled))
                    g.FillPath(fill, path);
                using (var pen = new Pen(focused ? Accent : Border, focused ? 1.6f : 1f))
                    g.DrawPath(pen, path);

                if (!focused) continue;

                // Soft outer glow — the WinForms stand-in for a CSS focus box-shadow.
                using var gp = RoundedButton.RoundedRect(Rectangle.Inflate(r, 2, 2), 9);
                using var gpen = new Pen(Color.FromArgb(70, Accent), 2f);
                g.DrawPath(gpen, gp);
            }
        };

        parent.Disposed += (_, _) => FramedParents.Remove(parent);
    }

    /// <summary>
    /// Dark drop-down: owner-drawn items (the closed box and the list) so it no longer shows
    /// the light system face, with an accent-highlighted selection.
    /// </summary>
    public static void StyleCombo(ComboBox cb)
    {
        cb.BackColor = Input;
        cb.ForeColor = Text;
        cb.FlatStyle = FlatStyle.Flat;
        cb.Cursor = Cursors.Hand;
        if (cb.DrawMode == DrawMode.OwnerDrawFixed) return;   // already styled

        cb.DrawMode = DrawMode.OwnerDrawFixed;
        cb.ItemHeight = Math.Max(cb.ItemHeight, cb.Font.Height + 6);
        cb.DrawItem += (s, e) =>
        {
            var box = (ComboBox)s!;
            bool edit = (e.State & DrawItemState.ComboBoxEdit) != 0;
            bool selected = !edit && (e.State & DrawItemState.Selected) != 0;

            using (var bg = new SolidBrush(selected ? Accent2 : box.Enabled ? Input : InputDisabled))
                e.Graphics.FillRectangle(bg, e.Bounds);

            if (e.Index < 0) return;
            var text = box.GetItemText(box.Items[e.Index]);
            var fore = !box.Enabled ? TextDim : selected ? Color.White : Text;
            var r = new Rectangle(e.Bounds.X + 6, e.Bounds.Y, e.Bounds.Width - 8, e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, text, box.Font, r, fore,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        };
    }

    // ─── Check boxes / radio buttons ─────────────────────────────────────────

    // Controls already carrying the toggle painter, so a second Apply() pass is a no-op.
    private static readonly HashSet<Control> Toggles = [];

    /// <summary>Box painted over the native glyph; the flat glyph is 13px wide.</summary>
    private const int GlyphBox = 15;

    /// <summary>
    /// Repaints the glyph of a CheckBox/RadioButton so its state reads at a glance on the
    /// dark theme: unchecked is an empty outlined box, checked is a filled accent box with a
    /// dark tick (or dot). The flat WinForms glyph only shifts a nearly-black tick on a
    /// nearly-black square, which is what made the state invisible.
    ///
    /// ButtonBase paints itself before raising Paint, so drawing here simply layers on top.
    /// </summary>
    public static void StyleToggle(ButtonBase btn, bool round)
    {
        btn.ForeColor = Text;
        btn.BackColor = Color.Transparent;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderColor = Border;
        btn.FlatAppearance.CheckedBackColor = Color.Transparent;
        btn.Cursor = Cursors.Hand;

        if (!Toggles.Add(btn)) return;
        btn.Disposed += (_, _) => Toggles.Remove(btn);
        EnableDoubleBuffering(btn);

        bool hover = false;
        btn.MouseEnter += (_, _) => { hover = true; btn.Invalidate(); };
        btn.MouseLeave += (_, _) => { hover = false; btn.Invalidate(); };
        // CheckedChanged fires before the repaint in some layouts — force one.
        if (btn is CheckBox c) c.CheckedChanged += (_, _) => btn.Invalidate();
        if (btn is RadioButton r) r.CheckedChanged += (_, _) => btn.Invalidate();

        btn.Paint += (s, e) =>
        {
            var ctl = (ButtonBase)s!;
            bool on = ctl is CheckBox cb ? cb.Checked : ((RadioButton)ctl).Checked;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Our box is drawn opaque over the 13px native glyph, so it hides it entirely —
            // no need to know the (possibly transparent) parent background colour.
            var box = new Rectangle(0, (ctl.Height - GlyphBox) / 2, GlyphBox - 1, GlyphBox - 1);
            Color line = !ctl.Enabled ? Border : on ? Accent : hover ? Mix(Border, Accent, 0.5) : Border;

            using var path = round ? EllipsePath(box) : RoundedButton.RoundedRect(box, 4);

            using (var fill = new SolidBrush(on && ctl.Enabled ? Accent : Input))
                g.FillPath(fill, path);
            using (var pen = new Pen(line, on ? 1.6f : 1.2f))
                g.DrawPath(pen, path);

            if (!on) return;

            if (round)
            {
                var dot = Rectangle.Inflate(box, -4, -4);
                using var inner = new SolidBrush(Bg);
                g.FillEllipse(inner, dot);
                return;
            }

            // Tick drawn dark on the accent fill for maximum contrast
            using var tick = new Pen(ctl.Enabled ? Bg : TextDim, 2f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
            };
            float x = box.X, y = box.Y, w = box.Width, h = box.Height;
            g.DrawLines(tick,
            [
                new PointF(x + w * 0.24f, y + h * 0.52f),
                new PointF(x + w * 0.44f, y + h * 0.72f),
                new PointF(x + w * 0.78f, y + h * 0.28f),
            ]);
        };
    }

    private static GraphicsPath EllipsePath(Rectangle r)
    {
        var p = new GraphicsPath();
        p.AddEllipse(r);
        return p;
    }

    private static Color Mix(Color a, Color b, double t) => Color.FromArgb(
        (int)(a.R + (b.R - a.R) * t),
        (int)(a.G + (b.G - a.G) * t),
        (int)(a.B + (b.B - a.B) * t));

    private static void EnableDoubleBuffering(Control c) =>
        typeof(Control)
            .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(c, true);

    /// <summary>Owner-draws the tab strip as modern segmented pills.</summary>
    public static void StyleTabs(TabControl tabs)
    {
        tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
        tabs.SizeMode = TabSizeMode.Fixed;
        tabs.Multiline = true;               // show every tab; no scroll arrows
        tabs.ItemSize = new Size(122, 36);
        tabs.Padding = new Point(18, 4);
        tabs.BackColor = Bg;

        tabs.DrawItem += (s, e) =>
        {
            var tc = (TabControl)s!;
            if (e.Index < 0 || e.Index >= tc.TabPages.Count) return;
            var page = tc.TabPages[e.Index];
            bool selected = e.Index == tc.SelectedIndex;

            var full = tc.GetTabRect(e.Index);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Clear the tab cell to the window background so pills float on it.
            using (var bg = new SolidBrush(Bg))
                e.Graphics.FillRectangle(bg, full);

            var pill = new Rectangle(full.X + 4, full.Y + 5, full.Width - 8, full.Height - 9);
            using var pillPath = RoundedButton.RoundedRect(pill, 9);

            if (selected)
            {
                using var fill = new SolidBrush(Accent2);
                e.Graphics.FillPath(fill, pillPath);
            }
            else if ((e.State & DrawItemState.HotLight) != 0)
            {
                using var fill = new SolidBrush(Hover);
                e.Graphics.FillPath(fill, pillPath);
            }

            var textColor = selected ? Color.White : TextDim;
            TextRenderer.DrawText(e.Graphics, page.Text, tc.Font, pill, textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        };
    }

    /// <summary>Paints a subtle dark gradient header with a hairline accent underline.</summary>
    public static void PaintHeader(Panel header)
    {
        header.Paint += (s, e) =>
        {
            var rect = header.ClientRectangle;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var brush = new LinearGradientBrush(rect,
                Color.FromArgb(18, 23, 32), Color.FromArgb(24, 33, 48), LinearGradientMode.Horizontal))
            {
                e.Graphics.FillRectangle(brush, rect);
            }

            // Soft accent glow along the bottom edge, then a crisp hairline.
            using (var glow = new LinearGradientBrush(
                new Rectangle(rect.Left, rect.Bottom - 14, rect.Width, 14),
                Color.FromArgb(0, Accent), Color.FromArgb(60, Accent), LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(glow, rect.Left, rect.Bottom - 14, rect.Width, 14);
            }
            using var pen = new Pen(Accent, 2);
            e.Graphics.DrawLine(pen, rect.Left, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
        };
    }

    /// <summary>
    /// Adds a hairline rule to the right of a section-caption label (replaces the old
    /// "───── X ─────" dashes). The label draws its own caption text; we only add the line.
    /// </summary>
    public static void PaintDivider(Label lbl)
    {
        lbl.Paint += (s, e) =>
        {
            var r = lbl.ClientRectangle;
            var size = TextRenderer.MeasureText(e.Graphics, lbl.Text, lbl.Font);
            using var pen = new Pen(Border, 1f);
            int lineY = r.Height / 2;
            int startX = size.Width + 10;
            if (startX < r.Right - 6)
                e.Graphics.DrawLine(pen, startX, lineY, r.Right - 6, lineY);
        };
    }
}
