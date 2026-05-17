using StudyGroups.Client.UI;
using StudyGroups.Contracts;
using StudyGroups.Http.Interfaces;

namespace StudyGroups.Client.Views;

public class UsersView : UserControl
{
    private readonly IUserApi _userApi;
    private readonly DataGridView _grid = new();
    private readonly Label _status = new();

    public UsersView(IUserApi userApi)
    {
        _userApi = userApi;
        BuildUI();
        _ = LoadUsersAsync();
    }

    private void BuildUI()
    {
        BackColor = Theme.Background;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            Padding = new Padding(20)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

        var title = new Label
        {
            Text = "Brugere",
            Dock = DockStyle.Fill,
            Font = Theme.TitleFont,
            ForeColor = Theme.TextPrimary,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var refresh = new Button
        {
            Text = "Refresh",
            Dock = DockStyle.Right,
            Width = 110
        };
        refresh.Click += async (_, _) => await LoadUsersAsync();

        header.Controls.Add(title, 0, 0);
        header.Controls.Add(refresh, 1, 0);

        _grid.Dock = DockStyle.Fill;
        _grid.AutoGenerateColumns = false;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.ReadOnly = true;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.BackgroundColor = Theme.Background;
        _grid.DefaultCellStyle.Font = Theme.BodyFont;
        _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Id", DataPropertyName = nameof(UserDto.Id), Width = 70 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Username", DataPropertyName = nameof(UserDto.Username), Width = 220 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Email", DataPropertyName = nameof(UserDto.Email), Width = 260 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Created", DataPropertyName = nameof(UserDto.CreatedAt), Width = 180 });

        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2
        };
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        _status.Dock = DockStyle.Fill;
        _status.ForeColor = Theme.TextSecondary;
        _status.TextAlign = ContentAlignment.MiddleLeft;
        _status.Text = "Loading users...";

        body.Controls.Add(_grid, 0, 0);
        body.Controls.Add(_status, 0, 1);

        root.Controls.Add(header, 0, 0);
        root.Controls.Add(body, 0, 1);

        Controls.Add(root);
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            _status.Text = "Loading users...";
            var users = (await _userApi.GetAll()).ToList();
            _grid.DataSource = users;
            _status.Text = $"Loaded {users.Count} users.";
        }
        catch (Exception ex)
        {
            _status.Text = "Could not load users.";
            MessageBox.Show(ex.Message, "Users failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
