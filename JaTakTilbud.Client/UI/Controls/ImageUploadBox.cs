using System;
using System.Drawing;
using System.Windows.Forms;

namespace JaTakTilbud.Client.UI.Controls;

public class ImageUploadBox : Panel
{
    private Image? image;

    public event Action<Image?>? ImageChanged;

    public ImageUploadBox()
    {
        Width = 100;
        Height = 100;
        BackColor = Color.FromArgb(240, 240, 240);
        Cursor = Cursors.Hand;

        Click += OnClick;
    }

    private void OnClick(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            image = Image.FromFile(dialog.FileName);

            // Notify listeners (preview)
            ImageChanged?.Invoke(image);

            Invalidate();
        }
    }

    public Image? GetImage()
    {
        return image;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (image != null)
        {
            e.Graphics.DrawImage(image, new Rectangle(0, 0, Width, Height));
        }
        else
        {
            // Placeholder
            var text = "Tilføj foto";
            var size = e.Graphics.MeasureString(text, Font);

            e.Graphics.DrawString(
                text,
                Font,
                Brushes.Gray,
                (Width - size.Width) / 2,
                (Height - size.Height) / 2
            );
        }

        // Border
        using var pen = new Pen(Color.LightGray);
        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }
}