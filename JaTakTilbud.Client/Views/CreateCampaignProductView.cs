using JaTakTilbud.Client.UI;
using JaTakTilbud.Client.UI.Controls;
using JaTakTilbud.Contracts;
using JaTakTilbud.Core.Models;
using JaTakTilbud.Http.Interfaces;

namespace JaTakTilbud.Client.Views;

public partial class CreateCampaignProductView : UserControl
{
    // =========================================================
    // DEPENDENCIES
    // =========================================================
    private readonly IProductApi _productApi;
    private readonly ICampaignApi _campaignApi;

    // =========================================================
    // STATE
    // =========================================================
    private Product? selectedProduct;

    private PictureBox? previewImage;
    private Label? lblDiscountBadge;
    private Label? errOffer;

    private ComboBox cmbProduct = null!;

    // NEW: override image (campaign-specific)
    private Image? overrideImage;

    public CreateCampaignProductView(IProductApi productApi, ICampaignApi campaignApi)
    {
        InitializeComponent();
        _productApi = productApi;
        _campaignApi = campaignApi;

        BuildUI();
        LoadProducts();
        LoadCampaigns();

        txtOfferPrice.TextChanged += OnInputChanged;
        txtQuantity.TextChanged += OnInputChanged;
        txtMaxPerCustomer.TextChanged += OnInputChanged;
    }

    private void OnInputChanged(object? sender, EventArgs e)
    {
        UpdatePreview(sender, e);
        ValidateForm();
    }

    // =========================================================
    // LOAD PRODUCTS
    // =========================================================
    private async void LoadProducts()
    {
        var products = await _productApi.GetAllAsync();

        if (products == null || products.Count == 0)
        {
            MessageBox.Show("No products found. Please create one first.");
            return;
        }

        cmbProduct.DataSource = products;
        cmbProduct.DisplayMember = nameof(ProductDto.Name);
        cmbProduct.ValueMember = nameof(ProductDto.Id);

        if (cmbProduct.Items.Count > 0)
        {
            cmbProduct.SelectedIndex = 0;

            if (cmbProduct.SelectedItem is ProductDto dto)
                selectedProduct = MapToDomain(dto);

            UpdatePreview(null, EventArgs.Empty);
        }

        cmbProduct.SelectedIndexChanged += (_, _) =>
        {
            if (cmbProduct.SelectedItem is not ProductDto dto)
                return;

            selectedProduct = MapToDomain(dto);
            UpdatePreview(null, EventArgs.Empty);
        };
    }

    // =========================================================
    // LOAD CAMPAIGNS (FIXED: no hardcoded items anymore)
    // =========================================================
    private async void LoadCampaigns()
    {
        var campaigns = await _campaignApi.GetCampaigns();

        if (campaigns == null || campaigns.Count == 0)
        {
            MessageBox.Show("No campaigns found. Create one first.");
            return;
        }

        cmbCampaign.DataSource = campaigns;
        cmbCampaign.DisplayMember = nameof(CampaignDto.Title);
        cmbCampaign.ValueMember = nameof(CampaignDto.Id);
    }

    // =========================================================
    // ROOT LAYOUT
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
            RowCount = 4,
            AutoScroll = true,
            Padding = new Padding(20)
        };

        for (int i = 0; i < 4; i++)
            left.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        left.Controls.Add(CreateCampaignCard(), 0, 0);
        left.Controls.Add(CreateProductCard(), 0, 1);
        left.Controls.Add(CreatePriceCard(), 0, 2);
        left.Controls.Add(CreatePickupCard(), 0, 3);

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
    // DOMAIN MAPPING
    // =========================================================
    private static Product MapToDomain(ProductDto dto)
    {
        return new Product
        {
            Id = dto.Id,
            Name = dto.Name,
            Price = dto.Price
        };
    }

    // =========================================================
    // VALIDATION
    // =========================================================
    private bool ValidateForm()
    {
        bool valid = true;

        if (selectedProduct == null)
            valid = false;

        if (!txtOfferPrice.NumericValue.HasValue)
        {
            txtOfferPrice.ErrorMessage = "Ugyldig pris";
            if (errOffer != null) errOffer.Text = "Indtast en gyldig pris";
            valid = false;
        }
        else
        {
            txtOfferPrice.ErrorMessage = null;
            if (errOffer != null) errOffer.Text = "";
        }

        if (!txtQuantity.NumericValue.HasValue || txtQuantity.NumericValue <= 0)
        {
            txtQuantity.ErrorMessage = "Skal være > 0";
            valid = false;
        }
        else txtQuantity.ErrorMessage = null;

        if (!txtMaxPerCustomer.NumericValue.HasValue || txtMaxPerCustomer.NumericValue <= 0)
        {
            txtMaxPerCustomer.ErrorMessage = "Skal være > 0";
            valid = false;
        }
        else txtMaxPerCustomer.ErrorMessage = null;

        return valid;
    }

    // =========================================================
    // CAMPAIGN CARD
    // =========================================================
    private CardPanel CreateCampaignCard()
    {
        var card = new CardPanel { Width = 520, Height = 100 };

        var title = new Label
        {
            Text = "KAMPAGNE",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Dock = DockStyle.Top
        };

        cmbCampaign.Width = 300;
        cmbCampaign.Left = 10;
        cmbCampaign.Top = 40;

        card.Controls.Add(title);
        card.Controls.Add(cmbCampaign);

        return card;
    }

    // =========================================================
    // PRODUCT CARD
    // =========================================================
    private CardPanel CreateProductCard()
    {
        var card = new CardPanel { Width = 520, Height = 220 };

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

        var imageBox = new ImageUploadBox { Width = 100, Height = 100 };

        imageBox.ImageChanged += (img) =>
        {
            overrideImage = img;
            UpdatePreview(null, EventArgs.Empty);
        };

        layout.Controls.Add(imageBox, 0, 0);

        var stack = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            Dock = DockStyle.Fill,
            WrapContents = false
        };

        cmbProduct = new ComboBox
        {
            Width = 250,
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        txtDesc = new UnderlineTextBox { PlaceholderText = "Beskrivelse", Width = 250 };

        stack.Controls.Add(new Label { Text = "Produkt:" });
        stack.Controls.Add(cmbProduct);
        stack.Controls.Add(txtDesc);

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
        var card = new CardPanel { Width = 520, Height = 180 };

        var title = new Label
        {
            Text = "2. PRIS & LAGER",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Dock = DockStyle.Top
        };

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = 3,
            Padding = new Padding(10)
        };

        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        errOffer = new Label { ForeColor = Color.Red, AutoSize = true };

        grid.Controls.Add(new Label { Text = "'JA TAK' PRIS:", Dock = DockStyle.Fill }, 0, 0);
        grid.Controls.Add(txtOfferPrice, 1, 0);
        grid.Controls.Add(new Label { Text = "kr", Dock = DockStyle.Fill }, 2, 0);
        grid.Controls.Add(errOffer, 1, 1);

        grid.Controls.Add(new Label { Text = "Antal:", Dock = DockStyle.Fill }, 0, 2);
        grid.Controls.Add(txtQuantity, 1, 2);

        grid.Controls.Add(new Label { Text = "Max pr kunde:", Dock = DockStyle.Fill }, 3, 2);
        grid.Controls.Add(txtMaxPerCustomer, 4, 2);

        card.Controls.Add(grid);
        card.Controls.Add(title);

        return card;
    }

    // =========================================================
    // PICKUP CARD
    // =========================================================
    private CardPanel CreatePickupCard()
    {
        var card = new CardPanel { Width = 520, Height = 140 };

        var title = new Label
        {
            Text = "3. AFHENTNING",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Dock = DockStyle.Top
        };

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(10)
        };

        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        dtStart.Dock = DockStyle.Fill;
        dtEnd.Dock = DockStyle.Fill;

        grid.Controls.Add(new Label { Text = "Start:", Dock = DockStyle.Fill }, 0, 0);
        grid.Controls.Add(dtStart, 1, 0);

        grid.Controls.Add(new Label { Text = "Slut:", Dock = DockStyle.Fill }, 0, 1);
        grid.Controls.Add(dtEnd, 1, 1);

        card.Controls.Add(grid);
        card.Controls.Add(title);

        return card;
    }

    // =========================================================
    // PREVIEW (FIXED LAYOUT)
    // =========================================================
    private CardPanel CreatePreviewCard()
    {
        var card = new CardPanel { Dock = DockStyle.Fill, BackColor = Color.White };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            Padding = new Padding(20)
        };

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var stack = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true
        };

        stack.Controls.Add(lblPreviewTitle);
        stack.Controls.Add(lblPreviewPrice);
        stack.Controls.Add(lblPreviewSavings);

        previewImage = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.LightGray
        };

        layout.Controls.Add(stack, 0, 0);
        layout.Controls.Add(previewImage, 0, 1);

        card.Controls.Add(layout);

        return card;
    }

    // =========================================================
    // BUTTONS
    // =========================================================
    private Panel CreateButtonBar()
    {
        var panel = new Panel { Dock = DockStyle.Fill };

        btnSave = new ModernButton { Text = "Gem kladde", Width = 140 };
        btnPublish = new ModernButton { Text = "Publicer", Width = 160, Left = 160 };

        btnPublish.Click += async (_, _) => await PublishAsync();

        panel.Controls.Add(btnSave);
        panel.Controls.Add(btnPublish);

        return panel;
    }

    private async Task PublishAsync()
    {
        if (!ValidateForm())
        {
            MessageBox.Show("Fix validation errors first.");
            return;
        }

        if (cmbCampaign.SelectedItem is not CampaignDto campaign)
        {
            MessageBox.Show("Select a campaign.");
            return;
        }

        if (selectedProduct == null)
        {
            MessageBox.Show("Select a product.");
            return;
        }

        var offer = txtOfferPrice.NumericValue!.Value;
        var quantity = (int)txtQuantity.NumericValue!.Value;

        try
        {
            byte[]? imageBytes = null;

            if (previewImage?.Image != null)
            {
                using var ms = new MemoryStream();
                previewImage.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                imageBytes = ms.ToArray();
            }

            var request = new AddCampaignProductRequest
            {
                ProductId = selectedProduct.Id,
                CampaignPrice = offer,
                Quantity = quantity,
                ImageBlob = imageBytes
            };

            await _campaignApi.AddProductAsync(campaign.Id, request);

            MessageBox.Show("Product added to campaign!");

            ClearForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}");
        }
    }

    private void ClearForm()
    {
        txtOfferPrice.TextValue = "";
        txtQuantity.TextValue = "";
        txtMaxPerCustomer.TextValue = "";
    }

    // =========================================================
    // PREVIEW LOGIC
    // =========================================================
    private void UpdatePreview(object? sender, EventArgs e)
    {
        if (selectedProduct != null)
            lblPreviewTitle.Text = selectedProduct.Name;

        if (overrideImage != null && previewImage != null)
            previewImage.Image = overrideImage;

        var offer = txtOfferPrice.NumericValue;

        if (selectedProduct != null && offer.HasValue)
        {
            var normal = selectedProduct.Price;
            var savings = normal - offer.Value;

            lblPreviewPrice.Text = $"Kun {FormatKr(offer.Value)}";

            if (savings > 0 && normal > 0)
            {
                lblPreviewSavings.Text = $"Før {FormatKr(normal)} • Du sparer {FormatKr(savings)}";

                var percent = (int)((savings / normal) * 100);

                if (lblDiscountBadge != null)
                {
                    lblDiscountBadge.Text = $"-{percent}%";
                    lblDiscountBadge.Visible = true;
                }
            }
        }
        else
        {
            lblPreviewPrice.Text = "Kun 0 kr";
            lblPreviewSavings.Text = "";
        }

        btnPublish.Enabled = ValidateForm();
    }

    private string FormatKr(decimal value)
    {
        return $"{value:N0} kr";
    }
}