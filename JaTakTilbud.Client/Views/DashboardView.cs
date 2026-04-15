using System;
using System.Drawing;
using System.Windows.Forms;
using JaTakTilbud.Client.UI;
using JaTakTilbud.Client.UI.Controls;

namespace JaTakTilbud.Client.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        BuildUI();
    }

    private void BuildUI()
    {
        BackColor = Theme.Background;

        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(20),
            WrapContents = true
        };

        // CARD 1
        var ordersCard = CreateCard("Total Orders", "124");

        // CARD 2
        var customersCard = CreateCard("Customers", "58");

        // CARD 3
        var campaignsCard = CreateCard("Active Campaigns", "12");

        layout.Controls.Add(ordersCard);
        layout.Controls.Add(customersCard);
        layout.Controls.Add(campaignsCard);

        Controls.Add(layout);
    }

    private CardPanel CreateCard(string title, string value)
    {
        var card = new CardPanel
        {
            Width = 260,
            Height = 140
        };

        var lblTitle = new Label
        {
            Text = title,
            Font = Theme.BodyFont,
            ForeColor = Theme.TextSecondary,
            Dock = DockStyle.Top,
            Height = 30
        };

        var lblValue = new Label
        {
            Text = value,
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };

        card.Controls.Add(lblValue);
        card.Controls.Add(lblTitle);

        return card;
    }
}