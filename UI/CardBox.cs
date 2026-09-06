using System.Drawing.Drawing2D;

namespace LedImageUpdaterService.UI;

/// <summary>
/// A GroupBox that renders as a modern flat "card": a softly rounded surface with
/// a hairline border and a small accent title — instead of the dated etched frame.
/// Layout behaviour (docking, AutoSize, child positioning) is unchanged, so it is a
/// drop-in replacement for <see cref="GroupBox"/>.
/// </summary>
internal sealed class CardBox : GroupBox
{
    public CardBox()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint
               | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        ForeColor = UITheme.Accent;
        Padding = new Padding(12, 30, 12, 12);
        Font = new Font("Segoe UI", 9f);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.Clear(Parent?.BackColor ?? UITheme.Bg);

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = RoundedButton.RoundedRect(rect, 12);

        using (var fill = new SolidBrush(UITheme.Card))
            g.FillPath(fill, path);
        using (var pen = new Pen(UITheme.Border, 1f))
            g.DrawPath(pen, path);

        // Title: a short accent bar + label, top-left.
        if (!string.IsNullOrEmpty(Text))
        {
            using var titleFont = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold);
            const int pad = 14;
            var barRect = new Rectangle(pad, 11, 3, 13);
            using (var bar = new SolidBrush(UITheme.Accent))
            using (var barPath = RoundedButton.RoundedRect(barRect, 2))
                g.FillPath(bar, barPath);

            TextRenderer.DrawText(g, Text, titleFont,
                new Point(pad + 9, 8), UITheme.Text, TextFormatFlags.NoPadding);
        }
    }
}
