using JaTakTilbud.Client.UI;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace JaTakTilbud.Client.Controls;

public class SidebarControl : UserControl
{
    public event Action<string>? Navigate;

    private Button? _activeButton;

    public SidebarControl()
    {
        InitializeSidebar();
    }

    private void InitializeSidebar()
    {
        Dock = DockStyle.Left;
        Width = 200;
        BackColor = Theme.SidebarBg;

        Padding = new Padding(0, 20, 0, 0);

        // Add buttons (top-down order)
        AddButton("Dashboard");

        AddButton("CreateProduct");
        AddButton("Products");

        AddButton("CreateCampaign");
        AddButton("ActiveCampaigns");

        AddButton("CreateCampaignProduct");

        AddButton("Orders");
        AddButton("Customers");
        AddButton("Settings");

        var dashboardButton = Controls
        .OfType<Button>()
        .FirstOrDefault(b => (string?)b.Tag == "Dashboard");

        if (dashboardButton != null)
        {
            dashboardButton.PerformClick();
        }
    }

    private void AddButton(string name)
    {
        var btn = new Button
        {
            Text = GetDisplayName(name),
            Tag = name,
            Dock = DockStyle.Top,
            Height = 45,

            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 0 },

            BackColor = Theme.SidebarBg,
            ForeColor = Color.White,
            Font = Theme.BodyFont,

            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(20, 0, 0, 0),
            Cursor = Cursors.Hand
        };

        btn.Click += OnButtonClick;

        Controls.Add(btn);
        Controls.SetChildIndex(btn, 0);
    }

    private void OnButtonClick(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.Tag is string route)
        {
            if (_activeButton != null)
            {
                _activeButton.BackColor = Theme.SidebarBg;
            }

            btn.BackColor = Theme.SidebarActive;
            _activeButton = btn;

            Navigate?.Invoke(route);
        }
    }

    private static string GetDisplayName(string route)
    {
        return route switch
        {
            "Dashboard" => "Forside",

            "CreateProduct" => "Opret Produkt",
            "Products" => "Produkter",

            "CreateCampaign" => "Opret Kampagne",
            "ActiveCampaigns" => "Aktive Kampagner",

            "CreateCampaignProduct" => "Opret kampagneprodukt",

            "Orders" => "Aktive reservationer",
            "Customers" => "Kunder",
            "Settings" => "Indstillinger",
            _ => route
        };
    }
}