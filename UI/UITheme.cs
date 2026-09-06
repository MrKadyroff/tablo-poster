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
                    tb.BackColor = Input; tb.ForeColor = Text; tb.BorderStyle = BorderStyle.FixedSingle;
                    break;
                case ListBox lb:
                    lb.BackColor = Input; lb.ForeColor = Text; lb.BorderStyle = BorderStyle.FixedSingle;
                    break;
                case ComboBox cb:
                    cb.BackColor = Input; cb.ForeColor = Text; cb.FlatStyle = FlatStyle.Flat;
                    break;
                case NumericUpDown nud:
                    nud.BackColor = Input; nud.ForeColor = Text; nud.BorderStyle = BorderStyle.FixedSingle;
                    // WinForms NumericUpDown has an internal TextBox that ignores the parent BackColor on value change
                    foreach (Control child in nud.Controls)
                    { child.BackColor = Input; child.ForeColor = Text; }
                    break;
                case CheckBox chk:
                    chk.ForeColor = Text; chk.BackColor = Color.Transparent;
                    chk.FlatStyle = FlatStyle.Flat;
                    chk.FlatAppearance.BorderColor = Border;
                    break;
                case RadioButton rb:
                    rb.ForeColor = Text; rb.BackColor = Color.Transparent;
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
