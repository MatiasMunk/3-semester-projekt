using JaTakTilbud.Client.UI;
using JaTakTilbud.Client.UI.Controls;
using JaTakTilbud.Contracts;
using JaTakTilbud.Http.Interfaces;

namespace JaTakTilbud.Client.Views;

public partial class CreateProductView : UserControl
{
    private readonly IProductApi _productApi;

    private byte[]? _imageBytes;
    private PictureBox? previewImage;

    public CreateProductView(IProductApi productApi)
    {
        InitializeComponent();
        _productApi = productApi;

        BuildUI();

        txtName.TextChanged += UpdatePreview;
        txtDescription.TextChanged += UpdatePreview;
        txtPrice.TextChanged += UpdatePreview;
    }

    // =========================================================
    // ROOT UI
    // =========================================================
    private void BuildUI()
    {
        BackColor = Theme.Background;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2
        };

        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

        var left = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            Padding = new Padding(20)
        };

        left.Controls.Add(CreateProductCard(), 0, 0);
        left.Controls.Add(CreatePriceCard(), 0, 1);

        var right = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            Padding = new Padding(20)
        };

        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));

        right.Controls.Add(CreatePreviewCard(), 0, 0);
        right.Controls.Add(CreateButtonBar(), 0, 1);

        root.Controls.Add(left, 0, 0);
        root.Controls.Add(right, 1, 0);

        Controls.Add(root);
    }

    // =========================================================
    // PRODUCT CARD
    // =========================================================
    private CardPanel CreateProductCard()
    {
        var card = new CardPanel { Width = 520, Height = 260 };

        var title = new Label
        {
            Text = "1. PRODUKT INFO",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Dock = DockStyle.Top
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(10)
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var imageBox = new ImageUploadBox
        {
            Width = 100,
            Height = 100
        };

        imageBox.ImageChanged += (img) =>
        {
            if (img != null)
            {
                using var ms = new MemoryStream();
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                _imageBytes = ms.ToArray();
            }

            if (previewImage != null)
                previewImage.Image = img;
        };

        layout.Controls.Add(imageBox, 0, 0);

        var stack = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            Dock = DockStyle.Fill
        };

        txtName = new UnderlineTextBox { PlaceholderText = "Produkt navn", Width = 250 };
        txtDescription = new UnderlineTextBox { PlaceholderText = "Beskrivelse", Width = 250 };

        stack.Controls.Add(txtName);
        stack.Controls.Add(txtDescription);

        layout.Controls.Add(stack, 1, 0);

        card.Controls.Add(layout);
        card.Controls.Add(title);

        return card;
    }

    // =========================================================
    // PRICE CARD
    // =========================================================
    private CardPanel CreatePriceCard()
    {
        var card = new CardPanel { Width = 520, Height = 140 };

        var title = new Label
        {
            Text = "2. PRIS",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Dock = DockStyle.Top
        };

        txtPrice = new UnderlineTextBox
        {
            PlaceholderText = "Pris",
            Width = 200,
            IsNumeric = true,
            AutoFormatCurrency = true
        };

        var stack = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(10)
        };

        stack.Controls.Add(txtPrice);

        card.Controls.Add(stack);
        card.Controls.Add(title);

        return card;
    }

    // =========================================================
    // PREVIEW
    // =========================================================
    private CardPanel CreatePreviewCard()
    {
        var card = new CardPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };

        var title = new Label
        {
            Text = "LIVE PREVIEW",
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };

        // =========================
        // MAIN LAYOUT (VERTICAL)
        // =========================
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 4,
            Padding = new Padding(20)
        };

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // Name
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // Desc
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // Price
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Image

        // =========================
        // TEXT STACK
        // =========================
        var textStack = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false
        };

        lblPreviewTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        lblPreviewDesc.Font = new Font("Segoe UI", 9);
        lblPreviewPrice.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        lblPreviewPrice.ForeColor = Color.Green;

        textStack.Controls.Add(lblPreviewTitle);
        textStack.Controls.Add(lblPreviewDesc);
        textStack.Controls.Add(lblPreviewPrice);

        // =========================
        // IMAGE (FULL WIDTH)
        // =========================
        previewImage = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.LightGray
        };

        // =========================
        // ADD TO LAYOUT
        // =========================
        layout.Controls.Add(textStack, 0, 0);
        layout.Controls.Add(previewImage, 0, 3);

        card.Controls.Add(layout);
        card.Controls.Add(title);

        return card;
    }

    // =========================================================
    // BUTTONS
    // =========================================================
    private Panel CreateButtonBar()
    {
        var panel = new Panel { Dock = DockStyle.Fill };

        btnCreate = new ModernButton
        {
            Text = "Create Product",
            Width = 180
        };

        btnCreate.Click += btnCreate_Click;

        panel.Controls.Add(btnCreate);

        return panel;
    }

    // =========================================================
    // PREVIEW LOGIC
    // =========================================================
    private void UpdatePreview(object? sender, EventArgs e)
    {
        lblPreviewTitle.Text = txtName.TextValue;
        lblPreviewDesc.Text = txtDescription.TextValue;

        if (txtPrice.NumericValue.HasValue)
            lblPreviewPrice.Text = $"{txtPrice.NumericValue.Value:N0} kr";
        else
            lblPreviewPrice.Text = "";
    }

    // =========================================================
    // CREATE
    // =========================================================
    private async void btnCreate_Click(object sender, EventArgs e)
    {
        try
        {
            var request = new CreateProductRequest
            {
                Name = txtName.TextValue.Trim(),
                Description = txtDescription.TextValue.Trim(),
                Price = txtPrice.NumericValue ?? 0,
                ImageBytes = _imageBytes
            };

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                MessageBox.Show("Name is required.");
                return;
            }

            await _productApi.CreateAsync(request);

            MessageBox.Show("Product created.");
            ClearForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void ClearForm()
    {
        txtName.TextValue = "";
        txtDescription.TextValue = "";
        txtPrice.TextValue = "";

        _imageBytes = null;

        if (previewImage != null)
            previewImage.Image = null;
    }
}