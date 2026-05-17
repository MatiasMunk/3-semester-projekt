using StudyGroups.Client.UI;
using StudyGroups.Client.UI.Controls;
using StudyGroups.Contracts;
using StudyGroups.Http.Interfaces;

namespace StudyGroups.Client.Views;

public partial class DashboardView : UserControl
{
    private readonly IStudySessionApi _sessionApi;
    private readonly FlowLayoutPanel _cards = new();
    private readonly ListView _activeSessions = new();
    private readonly Label _status = new();
    private readonly System.Windows.Forms.Timer _refreshTimer = new();

    public DashboardView(IStudySessionApi sessionApi)
    {
        InitializeComponent();
        _sessionApi = sessionApi;
        BuildUI();

        _refreshTimer.Interval = 30_000;
        _refreshTimer.Tick += async (_, _) => await LoadStatsAsync();
        _refreshTimer.Start();

        _ = LoadStatsAsync();
    }

    private void BuildUI()
    {
        BackColor = Theme.Background;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            Padding = new Padding(20)
        };

        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var title = new Label
        {
            Text = "Live session statistics",
            Font = Theme.TitleFont,
            ForeColor = Theme.TextPrimary,
            Dock = DockStyle.Top,
            Height = 45
        };

        _cards.Dock = DockStyle.Top;
        _cards.Height = 165;
        _cards.WrapContents = true;
        _cards.AutoScroll = false;

        _activeSessions.Dock = DockStyle.Fill;
        _activeSessions.View = View.Details;
        _activeSessions.FullRowSelect = true;
        _activeSessions.GridLines = true;
        _activeSessions.Columns.Add("Session", 260);
        _activeSessions.Columns.Add("Category", 140);
        _activeSessions.Columns.Add("Start", 160);
        _activeSessions.Columns.Add("Participants", 110);
        _activeSessions.Columns.Add("Location", 180);

        _status.Dock = DockStyle.Bottom;
        _status.Height = 28;
        _status.ForeColor = Theme.TextSecondary;
        _status.Text = "Loading...";

        var top = new Panel { Dock = DockStyle.Top, Height = 220 };
        top.Controls.Add(_cards);
        top.Controls.Add(title);

        root.Controls.Add(top, 0, 0);
        root.Controls.Add(_activeSessions, 0, 1);
        root.Controls.Add(_status, 0, 2);

        Controls.Add(root);
    }

    private async Task LoadStatsAsync()
    {
        try
        {
            _status.Text = "Refreshing session stats...";

            var sessions = (await _sessionApi.GetAll(null))?.ToList() ?? new List<StudySessionDto>();
            var now = DateTime.Now;
            var active = sessions
                .Where(s => s.StartTime >= now)
                .OrderBy(s => s.StartTime)
                .ToList();

            var totalParticipants = sessions.Sum(s => s.CurrentParticipants);
            var totalCapacity = sessions.Sum(s => s.MaxParticipants);
            var fullSessions = sessions.Count(s => s.MaxParticipants > 0 && s.CurrentParticipants >= s.MaxParticipants);

            _cards.Controls.Clear();
            _cards.Controls.Add(CreateCard("Total sessions", sessions.Count.ToString()));
            _cards.Controls.Add(CreateCard("Active sessions", active.Count.ToString()));
            _cards.Controls.Add(CreateCard("Participants", $"{totalParticipants}/{totalCapacity}"));
            _cards.Controls.Add(CreateCard("Full sessions", fullSessions.ToString()));

            _activeSessions.Items.Clear();
            foreach (var session in active.Take(25))
            {
                var item = new ListViewItem(session.Title);
                item.SubItems.Add($"{session.TopicIcon} {session.TopicName}".Trim());
                item.SubItems.Add(session.StartTime.ToString("g"));
                item.SubItems.Add($"{session.CurrentParticipants}/{session.MaxParticipants}");
                item.SubItems.Add(session.Location ?? "");
                _activeSessions.Items.Add(item);
            }

            _status.Text = $"Last refreshed {DateTime.Now:T}. Showing {Math.Min(active.Count, 25)} upcoming/active sessions.";
        }
        catch (Exception ex)
        {
            _status.Text = "Could not load live session stats.";
            MessageBox.Show(ex.Message, "Dashboard stats failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private CardPanel CreateCard(string title, string value)
    {
        var card = new CardPanel
        {
            Width = 220,
            Height = 125,
            Margin = new Padding(0, 0, 16, 16)
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
