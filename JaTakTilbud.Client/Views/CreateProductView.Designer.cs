using JaTakTilbud.Client.UI.Controls;

namespace JaTakTilbud.Client.Views
{
    partial class CreateProductView
    {
        private System.ComponentModel.IContainer components = null;

        // =========================================================
        // INPUT FIELDS
        // =========================================================
        private UnderlineTextBox txtName;
        private UnderlineTextBox txtDescription;
        private UnderlineTextBox txtPrice;

        // =========================================================
        // PREVIEW
        // =========================================================
        private Label lblPreviewTitle;
        private Label lblPreviewDesc;
        private Label lblPreviewPrice;

        // =========================================================
        // BUTTON
        // =========================================================
        private ModernButton btnCreate;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            // =========================
            // TEXT INPUTS
            // =========================
            txtName = new UnderlineTextBox();
            txtDescription = new UnderlineTextBox();
            txtPrice = new UnderlineTextBox();

            txtPrice.IsNumeric = true;
            txtPrice.AutoFormatCurrency = true;

            // =========================
            // PREVIEW LABELS
            // =========================
            lblPreviewTitle = new Label();
            lblPreviewDesc = new Label();
            lblPreviewPrice = new Label();

            // =========================
            // BUTTON
            // =========================
            btnCreate = new ModernButton();

            // =========================
            // ROOT CONTROL
            // =========================
            SuspendLayout();

            Name = "CreateProductView";
            Size = new System.Drawing.Size(1000, 650);

            ResumeLayout(false);
        }
    }
}