using StudyGroups.Client.UI;

namespace StudyGroups.Client.Controls;

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
        Width = 240;
        BackColor = Theme.SidebarBg;
        Padding = new Padding(0, 20, 0, 0);

        // Sprint 4 cleanup: StudyGroups client navigation should use
        // sessions/users/categories language.
        AddButton("Dashboard");
        AddButton("CreateStudySession");
        AddButton("ActiveSessions");
        AddButton("Categories");
        AddButton("Users");
        AddButton("Settings");

        var dashboardButton = Controls
            .OfType<Button>()
            .FirstOrDefault(b => (string?)b.Tag == "Dashboard");

        dashboardButton?.PerformClick();
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
            Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(20, 0, 0, 0),
            AutoEllipsis = true,
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
                _activeButton.BackColor = Theme.SidebarBg;

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
            "CreateStudySession" => "Opret session",
            "ActiveSessions" => "Aktive sessions",
            "Categories" => "Kategorier",
            "Users" => "Brugere",
            "Settings" => "Indstillinger",
            _ => route
        };
    }
}
