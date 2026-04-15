using JaTakTilbud.Client.Controls;
using JaTakTilbud.Client.Views;
using JaTakTilbud.Client.UI;

namespace JaTakTilbud.Client;

public partial class MainForm : Form
{
    private Panel mainArea = new Panel();
    private Panel contentPanel = new Panel();

    public MainForm()
    {
        InitializeComponent();
        InitializeLayout();

        BackColor = Theme.Background;
        contentPanel.BackColor = Theme.Background;
        mainArea.BackColor = Theme.Background;

        LoadView("Dashboard");
    }

    private void InitializeLayout()
    {
        var sidebar = new SidebarControl();
        sidebar.Navigate += LoadView;

        mainArea.Dock = DockStyle.Fill;

        // =========================
        // HEADER (MODERN LAYOUT)
        // =========================
        var header = new TableLayoutPanel();
        header.Dock = DockStyle.Top;
        header.Height = 60;
        header.BackColor = Theme.Surface;
        header.Padding = new Padding(10, 0, 10, 0);
        header.ColumnCount = 2;

        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

        var lblTitle = new Label
        {
            Text = "MINE 'JA TAK'-TILBUD",
            Font = Theme.TitleFont,
            ForeColor = Theme.TextPrimary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var lblUser = new Label
        {
            Text = "Velkommen, Hans",
            Font = Theme.BodyFont,
            ForeColor = Theme.TextSecondary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight
        };

        header.Controls.Add(lblTitle, 0, 0);
        header.Controls.Add(lblUser, 1, 0);

        // =========================
        // CONTENT
        // =========================
        contentPanel.Dock = DockStyle.Fill;

        mainArea.Controls.Add(contentPanel);
        mainArea.Controls.Add(header);

        Controls.Add(mainArea);
        Controls.Add(sidebar);
    }

    private void LoadView(string viewName)
    {
        contentPanel.Controls.Clear();

        UserControl view = viewName switch
        {
            "Dashboard" => new DashboardView(),
            "CreateCampaign" => new CreateCampaignView(),
            _ => new DashboardView()
        };

        view.Dock = DockStyle.Fill;
        contentPanel.Controls.Add(view);
    }
}