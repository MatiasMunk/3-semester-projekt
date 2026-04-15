using JaTakTilbud.Core.Models;
using JaTakTilbud.Client.UI;
using JaTakTilbud.Client.UI.Controls;

namespace JaTakTilbud.Client.Views;

public partial class CreateCampaignView : UserControl
{
    // =========================================================
    // FIELDS
    // =========================================================
    private Product? selectedProduct;
    private PictureBox? previewImage;
    private Label? lblDiscountBadge;
    private Label? errOffer;

    public CreateCampaignView()
    {
        InitializeComponent();
        BuildUI();

        txtTitle.TextChanged += OnInputChanged;
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
    // VALIDATION
    // =========================================================
    private bool ValidateForm()
    {
        bool valid = true;

        if (selectedProduct == null)
            valid = false;

        if (string.IsNullOrWhiteSpace(txtTitle.TextValue))
        {
            txtTitle.ErrorMessage = "Påkrævet";
            valid = false;
        }
        else txtTitle.ErrorMessage = null;

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

        cmbCampaign.Items.Clear();
        cmbCampaign.Items.AddRange(new object[]
        {
            "Uge 15 Tilbud",
            "Weekend Kampagne",
            "Sommer Deals"
        });
        cmbCampaign.SelectedIndex = 0;

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
            if (previewImage != null)
                previewImage.Image = img;
        };

        layout.Controls.Add(imageBox, 0, 0);

        var stack = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            Dock = DockStyle.Fill,
            WrapContents = false
        };

        var cmbProduct = new ComboBox
        {
            Width = 250,
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        var products = new[]
        {
            new Product { Id = 1, Name = "Oksekød 1kg", Price = 120 },
            new Product { Id = 2, Name = "Kylling 2kg", Price = 90 },
            new Product { Id = 3, Name = "Laks 500g", Price = 150 }
        };

        cmbProduct.DataSource = products;
        cmbProduct.DisplayMember = "Name";

        cmbProduct.SelectedIndexChanged += (_, _) =>
        {
            selectedProduct = cmbProduct.SelectedItem as Product;
            UpdatePreview(null, EventArgs.Empty);
        };

        txtTitle = new UnderlineTextBox { PlaceholderText = "Produktnavn", Width = 250 };
        txtDesc = new UnderlineTextBox { PlaceholderText = "Beskrivelse", Width = 250 };

        stack.Controls.Add(new Label { Text = "Produkt:" });
        stack.Controls.Add(cmbProduct);
        stack.Controls.Add(txtTitle);
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
    // PICKUP CARD (THIS WAS MISSING BEFORE)
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
    // PREVIEW
    // =========================================================
    private CardPanel CreatePreviewCard()
    {
        var card = new CardPanel { Dock = DockStyle.Fill, BackColor = Color.White };

        var title = new Label
        {
            Text = "LIVE PREVIEW",
            Dock = DockStyle.Top
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(20)
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var wrapper = new Panel { Width = 120, Height = 120 };

        previewImage = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.LightGray
        };

        var badge = new Label
        {
            BackColor = Color.Red,
            ForeColor = Color.White,
            Padding = new Padding(6, 2, 6, 2),
            Location = new Point(70, 5),
            Visible = false
        };

        lblDiscountBadge = badge;

        wrapper.Controls.Add(previewImage);
        wrapper.Controls.Add(badge);

        layout.Controls.Add(wrapper, 0, 0);

        var stack = new FlowLayoutPanel { Dock = DockStyle.Fill };

        stack.Controls.Add(lblPreviewTitle);
        stack.Controls.Add(lblPreviewPrice);
        stack.Controls.Add(lblPreviewSavings);

        layout.Controls.Add(stack, 1, 0);

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

        btnSave = new ModernButton { Text = "Gem kladde", Width = 140 };
        btnPublish = new ModernButton { Text = "Publicer", Width = 160, Left = 160 };

        panel.Controls.Add(btnSave);
        panel.Controls.Add(btnPublish);

        return panel;
    }

    // =========================================================
    // PREVIEW LOGIC
    // =========================================================
    private void UpdatePreview(object? sender, EventArgs e)
    {
        lblPreviewTitle.Text = txtTitle.TextValue;

        var offer = txtOfferPrice.NumericValue;

        if (selectedProduct != null && offer.HasValue)
        {
            var normal = selectedProduct.Price;
            var savings = normal - offer.Value;

            lblPreviewPrice.Text = $"Kun {FormatKr(offer.Value)}";

            if (savings > 0)
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