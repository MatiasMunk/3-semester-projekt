namespace JaTakTilbud.Client.Views
{
    partial class DashboardView
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblTitle;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitle = new Label();

            SuspendLayout();

            // lblTitle
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblTitle.Location = new Point(20, 20);
            lblTitle.Text = "Dashboard";

            // DashboardView
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = JaTakTilbud.Client.UI.Theme.Background; // IMPORTANT
            Controls.Add(lblTitle);
            Name = "DashboardView";
            Size = new Size(800, 600);

            ResumeLayout(false);
            PerformLayout();
        }
    }
}