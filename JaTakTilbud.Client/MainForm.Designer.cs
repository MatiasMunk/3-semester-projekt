namespace JaTakTilbud.Client;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(1200, 800);
        this.Text = "JaTakTilbud Admin";
    }
}