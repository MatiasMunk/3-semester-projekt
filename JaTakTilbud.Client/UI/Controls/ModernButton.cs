using System.Drawing;
using System.Windows.Forms;
using JaTakTilbud.Client.UI;

namespace JaTakTilbud.Client.UI.Controls;

public class ModernButton : Button
{
    public ModernButton()
    {
        Height = 40;
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;

        BackColor = Theme.Primary;
        ForeColor = Color.White;
        Font = Theme.BodyFont;

        Cursor = Cursors.Hand;
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        BackColor = Theme.PrimaryHover;
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        BackColor = Theme.Primary;
        base.OnMouseLeave(e);
    }
}