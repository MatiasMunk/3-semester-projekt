using StudyGroups.Client.UI;
using StudyGroups.Client.UI.Controls;
using StudyGroups.Contracts;
using StudyGroups.Http.Interfaces;

namespace StudyGroups.Client.Views;

public partial class CreateStudySessionView : UserControl
{
    private readonly IStudySessionApi _sessionApi;
    private readonly ICategoryApi _categoryApi;

    private ComboBox cmbTopic = new();

    public CreateStudySessionView(IStudySessionApi sessionApi, ICategoryApi categoryApi)
    {
        InitializeComponent();
        _sessionApi = sessionApi;
        _categoryApi = categoryApi;

        BuildUI();

        txtTitle.TextChanged += OnInputChanged;
        txtLocation.TextChanged += OnInputChanged;
        txtMaxParticipants.TextChanged += OnInputChanged;
        cmbTopic.SelectedIndexChanged += OnInputChanged;

        _ = LoadCategoriesAsync();
    }

    private void OnInputChanged(object? sender, EventArgs e)
    {
        UpdatePreview();
        ValidateForm();
    }

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

        left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        left.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        left.Controls.Add(CreateInfoCard(), 0, 0);
        left.Controls.Add(CreateDetailsCard(), 0, 1);

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

    private CardPanel CreateInfoCard()
    {
        var card = new CardPanel { Width = 520, Height = 200 };

        var title = new Label
        {
            Text = "SESSION INFO",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Dock = DockStyle.Top
        };

        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(10)
        };

        txtTitle = new UnderlineTextBox { PlaceholderText = "Title", Width = 300 };
        layout.Controls.Add(txtTitle);

        card.Controls.Add(layout);
        card.Controls.Add(title);

        return card;
    }

    private CardPanel CreateDetailsCard()
    {
        var card = new CardPanel { Width = 520, Height = 260 };

        var title = new Label
        {
            Text = "DETAILS",
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

        cmbTopic = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 250,
            Font = new Font("Segoe UI Emoji", 10F, FontStyle.Regular),
            FormattingEnabled = true,
            DisplayMember = nameof(CategoryDto.Name),
            ValueMember = nameof(CategoryDto.Id)
        };
        cmbTopic.Format += (_, e) =>
        {
            if (e.ListItem is CategoryDto category)
                e.Value = $"{category.Icon} {category.Name}";
        };

        txtLocation = new UnderlineTextBox { PlaceholderText = "Location", Width = 250 };

        txtMaxParticipants = new UnderlineTextBox
        {
            PlaceholderText = "Max participants",
            IsNumeric = true,
            Width = 250
        };

        dtStart = new DateTimePicker { Dock = DockStyle.Fill };

        layout.Controls.Add(new Label { Text = "Category:" }, 0, 0);
        layout.Controls.Add(cmbTopic, 1, 0);

        layout.Controls.Add(new Label { Text = "Location:" }, 0, 1);
        layout.Controls.Add(txtLocation, 1, 1);

        layout.Controls.Add(new Label { Text = "Start time:" }, 0, 2);
        layout.Controls.Add(dtStart, 1, 2);

        layout.Controls.Add(new Label { Text = "Capacity:" }, 0, 3);
        layout.Controls.Add(txtMaxParticipants, 1, 3);

        card.Controls.Add(layout);
        card.Controls.Add(title);

        return card;
    }

    private CardPanel CreatePreviewCard()
    {
        var card = new CardPanel { Dock = DockStyle.Fill };

        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(20)
        };

        lblPreviewTitle = new Label { Font = new Font("Segoe UI", 14, FontStyle.Bold), Width = 320 };
        lblPreviewSubject = new Label { Width = 320 };
        lblPreviewDetails = new Label { Width = 320 };

        layout.Controls.Add(lblPreviewTitle);
        layout.Controls.Add(lblPreviewSubject);
        layout.Controls.Add(lblPreviewDetails);

        card.Controls.Add(layout);

        return card;
    }

    private Panel CreateButtonBar()
    {
        var panel = new Panel { Dock = DockStyle.Fill };

        btnCreate = new ModernButton { Text = "Create Session", Width = 180 };
        btnCreate.Click += async (_, _) => await CreateSession();

        panel.Controls.Add(btnCreate);

        return panel;
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = (await _categoryApi.GetAll()).ToList();
            cmbTopic.DataSource = categories;

            if (categories.Count > 0)
                cmbTopic.SelectedIndex = 0;

            ValidateForm();
            UpdatePreview();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not load categories. Add categories in the Categories page first.\n\n{ex.Message}");
        }
    }

    private async Task CreateSession()
    {
        if (!ValidateForm())
        {
            MessageBox.Show("Fix errors first. Make sure title, category and capacity are set.");
            return;
        }

        var request = new CreateStudySessionRequest
        {
            Title = txtTitle.TextValue,
            Description = "",
            TopicId = (int)cmbTopic.SelectedValue!,
            Location = txtLocation.TextValue,
            StartTime = dtStart.Value,
            MaxParticipants = (int)txtMaxParticipants.NumericValue!.Value,
            UserId = 1
        };

        try
        {
            await _sessionApi.Create(request);
            MessageBox.Show("Session created");
            ClearForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void ClearForm()
    {
        txtTitle.TextValue = "";
        txtLocation.TextValue = "";
        txtMaxParticipants.TextValue = "";
        if (cmbTopic.Items.Count > 0)
            cmbTopic.SelectedIndex = 0;
    }

    private bool ValidateForm()
    {
        bool valid = true;

        if (string.IsNullOrWhiteSpace(txtTitle.TextValue))
            valid = false;

        if (cmbTopic.SelectedValue == null)
            valid = false;

        if (!txtMaxParticipants.NumericValue.HasValue || txtMaxParticipants.NumericValue <= 0)
            valid = false;

        btnCreate.Enabled = valid;

        return valid;
    }

    private void UpdatePreview()
    {
        lblPreviewTitle.Text = string.IsNullOrWhiteSpace(txtTitle.TextValue) ? "Session title" : txtTitle.TextValue;
        lblPreviewSubject.Text = cmbTopic.SelectedItem is CategoryDto category
            ? $"Category: {category.Icon} {category.Name}"
            : "Category: none selected";
        lblPreviewDetails.Text = $"{txtLocation.TextValue} • {dtStart.Value:g}";
    }
}
