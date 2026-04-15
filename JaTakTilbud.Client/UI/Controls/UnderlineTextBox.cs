using System.Globalization;
using JaTakTilbud.Client.UI;

namespace JaTakTilbud.Client.UI.Controls;

public class UnderlineTextBox : Panel
{
    private readonly TextBox inner;
    private bool isFocused = false;

    public bool IsNumeric { get; set; } = false;
    public bool AutoFormatCurrency { get; set; } = false;

    private string? errorMessage;

    public string? ErrorMessage
    {
        get => errorMessage;
        set { errorMessage = value; Invalidate(); }
    }

    public decimal? NumericValue
    {
        get
        {
            if (string.IsNullOrWhiteSpace(inner.Text))
                return null;

            var clean = inner.Text.Replace(".", "").Trim();

            if (decimal.TryParse(clean, NumberStyles.Number, CultureInfo.InvariantCulture, out var val))
                return val;

            return null;
        }
    }

    public string TextValue
    {
        get => inner.Text;
        set => inner.Text = value ?? string.Empty;
    }

    public new event EventHandler? TextChanged;

    public string PlaceholderText
    {
        get => inner.PlaceholderText;
        set => inner.PlaceholderText = value;
    }

    public UnderlineTextBox()
    {
        Height = 26;
        Width = 120;
        BackColor = Theme.Surface;

        inner = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Font = Theme.BodyFont,
            Location = new Point(0, 5), // FIXED ALIGNMENT
            Width = Width
        };

        inner.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

        inner.TextChanged += (s, e) => TextChanged?.Invoke(this, e);
        inner.KeyPress += OnKeyPress;
        inner.GotFocus += (_, _) => { isFocused = true; Invalidate(); };
        inner.LostFocus += OnLostFocus;

        Controls.Add(inner);

        Resize += (_, _) => inner.Width = Width;
    }

    private void OnKeyPress(object? sender, KeyPressEventArgs e)
    {
        if (!IsNumeric) return;

        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            e.Handled = true;
    }

    private void OnLostFocus(object? sender, EventArgs e)
    {
        isFocused = false;

        if (IsNumeric && AutoFormatCurrency)
        {
            var clean = inner.Text.Replace(".", "");

            if (decimal.TryParse(clean, out var val))
                inner.Text = val.ToString("N0");
        }

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var color = !string.IsNullOrEmpty(errorMessage)
            ? Color.Red
            : isFocused ? Theme.Primary : Theme.Border;

        using var pen = new Pen(color, 2);
        e.Graphics.DrawLine(pen, 0, Height - 1, Width, Height - 1);
    }
}