using JaTakTilbud.Client.UI.Controls;

namespace JaTakTilbud.Client.Views;

partial class CreateProductView
{
    private UnderlineTextBox txtName;
    private UnderlineTextBox txtDescription;
    private UnderlineTextBox txtPrice;
    private UnderlineTextBox txtImageUrl;
    private ModernButton btnCreate;
    private CardPanel container;

    private void InitializeComponent()
    {
        container = new CardPanel();
        txtName = new UnderlineTextBox();
        txtDescription = new UnderlineTextBox();
        txtPrice = new UnderlineTextBox();
        txtImageUrl = new UnderlineTextBox();
        btnCreate = new ModernButton();

        SuspendLayout();

        container.Dock = DockStyle.Fill;

        txtName.PlaceholderText = "Product Name";
        txtName.Top = 30;
        txtName.Width = 300;

        txtDescription.PlaceholderText = "Description";
        txtDescription.Top = 80;
        txtDescription.Width = 300;

        txtPrice.PlaceholderText = "Price";
        txtPrice.Top = 130;
        txtPrice.Width = 300;

        txtImageUrl.PlaceholderText = "Image URL";
        txtImageUrl.Top = 180;
        txtImageUrl.Width = 300;

        btnCreate.Text = "Create Product";
        btnCreate.Top = 240;
        btnCreate.Width = 200;
        btnCreate.Click += btnCreate_Click;

        container.Controls.Add(txtName);
        container.Controls.Add(txtDescription);
        container.Controls.Add(txtPrice);
        container.Controls.Add(txtImageUrl);
        container.Controls.Add(btnCreate);

        Controls.Add(container);

        ResumeLayout(false);
    }
}