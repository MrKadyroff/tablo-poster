using System.Drawing.Drawing2D;
using System.Globalization;

namespace LedImageUpdaterService.UI;

/// <summary>
/// A modern numeric input: a rounded field with a value in the middle and clear
/// large "−" / "+" stepper buttons on the sides. Press-and-hold repeats.
///
/// Deliberately exposes the same surface the form used on <see cref="NumericUpDown"/>
/// (<c>Value</c>, <c>Minimum</c>, <c>Maximum</c>, <c>Increment</c>, <c>ValueChanged</c>)
/// so it is a drop-in replacement — the behaviour is identical, only the look changed.
/// </summary>
internal sealed class NumericStepper : Control
{
    private readonly TextBox _text;
    private readonly StepButton _minus;
    private readonly StepButton _plus;

    private decimal _value;
    private decimal _min;
    private decimal _max = 100;
    private decimal _increment = 1;
    private bool _syncing;

    public event EventHandler? ValueChanged;

    public NumericStepper()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer
               | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        BackColor = Color.Transparent;
        ForeColor = UITheme.Text;
        Width = 110;
        Height = 32;

        _minus = new StepButton("−") { Cursor = Cursors.Hand };
        _plus = new StepButton("+") { Cursor = Cursors.Hand };
        _minus.Step += (_, _) => Nudge(-1);
        _plus.Step += (_, _) => Nudge(+1);

        _text = new TextBox
        {
            BorderStyle = BorderStyle.None,
            TextAlign = HorizontalAlignment.Center,
            BackColor = UITheme.Input,
            ForeColor = UITheme.Text,
            Font = new Font("Segoe UI Semibold", 10.5f),
        };
        _text.KeyPress += OnKeyPress;
        _text.TextChanged += OnTextChanged;
        _text.Leave += (_, _) => NormalizeText();
        _text.Enter += (_, _) => Invalidate();

        Controls.Add(_text);
        Controls.Add(_minus);
        Controls.Add(_plus);

        UpdateText();
    }

    // ─── Public API (mirrors NumericUpDown) ─────────────────────────────────

    public decimal Minimum
    {
        get => _min;
        set { _min = value; if (_value < _min) Value = _min; }
    }

    public decimal Maximum
    {
        get => _max;
        set { _max = value; if (_value > _max) Value = _max; }
    }

    public decimal Increment
    {
        get => _increment;
        set => _increment = value <= 0 ? 1 : value;
    }

    public decimal Value
    {
        get => _value;
        set
        {
            var clamped = Clamp(value);
            if (clamped == _value) { UpdateText(); return; }
            _value = clamped;
            UpdateText();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private decimal Clamp(decimal v) => v < _min ? _min : v > _max ? _max : v;

    // ─── Interaction ────────────────────────────────────────────────────────

    private void Nudge(int sign)
    {
        Value = _value + sign * _increment;
        if (!_text.Focused) Focus();
    }

    private void OnKeyPress(object? sender, KeyPressEventArgs e)
    {
        // Digits, control chars, one leading sign and one decimal separator.
        if (char.IsControl(e.KeyChar)) return;
        var sep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        bool allowed = char.IsDigit(e.KeyChar)
            || (e.KeyChar == '-' && _min < 0 && _text.SelectionStart == 0 && !_text.Text.Contains('-'))
            || (sep.Length == 1 && e.KeyChar == sep[0] && !_text.Text.Contains(sep));
        if (!allowed) e.Handled = true;
    }

    private void OnTextChanged(object? sender, EventArgs e)
    {
        if (_syncing) return;
        // Commit live so consumers (e.g. the design live-preview) update as you type,
        // but do not rewrite the box mid-edit (that would fight the caret).
        if (decimal.TryParse(_text.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var parsed))
        {
            var clamped = Clamp(parsed);
            if (clamped != _value)
            {
                _value = clamped;
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void NormalizeText()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        _syncing = true;
        var s = _value.ToString(_value == Math.Truncate(_value) ? "0" : "0.###", CultureInfo.CurrentCulture);
        if (_text.Text != s) _text.Text = s;
        _syncing = false;
    }

    // ─── Layout & paint ─────────────────────────────────────────────────────

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        if (_text is null) return;
        _text.Enabled = Enabled;
        _minus.Enabled = Enabled;
        _plus.Enabled = Enabled;
        _text.BackColor = Enabled ? UITheme.Input : UITheme.InputDisabled;
        _text.ForeColor = Enabled ? UITheme.Text : UITheme.TextDim;
        Invalidate();
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        if (_text is null) return;   // children not built yet (during ctor sizing)
        int btn = Math.Min(30, Height);
        int h = Height;
        _minus.SetBounds(0, 0, btn, h);
        _plus.SetBounds(Width - btn, 0, btn, h);

        int textX = btn + 4;
        int textW = Width - 2 * btn - 8;
        // Vertically centre the (fixed-height) textbox.
        int ty = Math.Max(0, (h - _text.PreferredHeight) / 2);
        _text.SetBounds(textX, ty, Math.Max(4, textW), _text.PreferredHeight);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.Clear(Parent?.BackColor ?? UITheme.Panel);

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = RoundedButton.RoundedRect(rect, 8);

        using (var fill = new SolidBrush(Enabled ? UITheme.Input : UITheme.InputDisabled))
            g.FillPath(fill, path);

        var borderColor = _text.Focused ? UITheme.Accent : UITheme.Border;
        using (var pen = new Pen(borderColor, _text.Focused ? 1.5f : 1f))
            g.DrawPath(pen, path);
    }

    /// <summary>Flat glyph button with press-and-hold auto-repeat.</summary>
    private sealed class StepButton : Control
    {
        private readonly string _glyph;
        private readonly System.Windows.Forms.Timer _repeat = new() { Interval = 380 };
        private bool _hover, _down;
        private int _ticks;

        public event EventHandler? Step;

        public StepButton(string glyph)
        {
            _glyph = glyph;
            SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Color.Transparent;
            _repeat.Tick += (_, _) =>
            {
                _ticks++;
                if (_ticks == 1) _repeat.Interval = 60; // accelerate after the first hold-repeat
                Step?.Invoke(this, EventArgs.Empty);
            };
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; StopRepeat(); Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _down = true; Invalidate();
                Step?.Invoke(this, EventArgs.Empty);   // fire immediately on press
                _ticks = 0; _repeat.Interval = 380; _repeat.Start();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) { _down = false; StopRepeat(); Invalidate(); base.OnMouseUp(e); }

        private void StopRepeat() { _repeat.Stop(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? UITheme.Input);

            Color glyphColor = !Enabled ? UITheme.TextDim
                : _down ? UITheme.Accent
                : _hover ? UITheme.Text
                : UITheme.TextDim;

            if (_hover && Enabled)
            {
                using var hl = new SolidBrush(UITheme.Hover);
                using var hp = RoundedButton.RoundedRect(new Rectangle(2, 3, Width - 4, Height - 7), 6);
                g.FillPath(hl, hp);
            }

            using var f = new Font("Segoe UI", 13f, FontStyle.Bold);
            TextRenderer.DrawText(g, _glyph, f, ClientRectangle, glyphColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _repeat.Dispose();
            base.Dispose(disposing);
        }
    }
}
