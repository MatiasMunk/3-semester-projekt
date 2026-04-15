using System.Drawing;
using System.Windows.Forms;
using JaTakTilbud.Client.UI;

namespace JaTakTilbud.Client.UI.Controls;

public class CardPanel : Panel
{
    public CardPanel()
    {
        BackColor = Theme.Surface;
        Padding = new Padding(16);
        Margin = new Padding(10);

        BorderStyle = BorderStyle.None;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        using var pen = new Pen(Theme.Border);
        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }
}