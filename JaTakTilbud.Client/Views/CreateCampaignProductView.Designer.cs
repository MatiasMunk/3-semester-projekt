using JaTakTilbud.Client.UI.Controls;

namespace JaTakTilbud.Client.Views
{
    partial class CreateCampaignProductView
    {
        private System.ComponentModel.IContainer components = null;

        private UnderlineTextBox txtDesc;
        private UnderlineTextBox txtNormalPrice;
        private UnderlineTextBox txtOfferPrice;
        private UnderlineTextBox txtQuantity;
        private UnderlineTextBox txtMaxPerCustomer;

        private DateTimePicker dtStart;
        private DateTimePicker dtEnd;

        private Label lblPreviewTitle;
        private Label lblPreviewPrice;
        private Label lblPreviewSavings;

        private Button btnSave;
        private Button btnPublish;

        private ComboBox cmbCampaign;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            txtDesc = new UnderlineTextBox();

            txtNormalPrice = new UnderlineTextBox();
            txtNormalPrice.IsNumeric = true;
            txtNormalPrice.AutoFormatCurrency = true;

            txtOfferPrice = new UnderlineTextBox();
            txtOfferPrice.IsNumeric = true;
            txtOfferPrice.AutoFormatCurrency = true;

            txtQuantity = new UnderlineTextBox();
            txtQuantity.IsNumeric = true;

            txtMaxPerCustomer = new UnderlineTextBox();
            txtMaxPerCustomer.IsNumeric = true;

            dtStart = new DateTimePicker();
            dtEnd = new DateTimePicker();

            lblPreviewTitle = new Label();
            lblPreviewPrice = new Label();
            lblPreviewSavings = new Label();

            btnSave = new Button();
            btnPublish = new Button();

            cmbCampaign = new ComboBox();

            SuspendLayout();

            // Root
            Name = "CreateCampaignView";
            Size = new System.Drawing.Size(1000, 650);

            ResumeLayout(false);
        }
    }
}