using StudyGroups.Client.UI;
using StudyGroups.Client.UI.Controls;
using StudyGroups.Contracts;
using StudyGroups.Http.Interfaces;

namespace StudyGroups.Client.Views;

public class CategoriesView : UserControl
{
    private readonly ICategoryApi _categoryApi;
    private readonly DataGridView _grid = new();
    private readonly UnderlineTextBox _name = new();
    private readonly UnderlineTextBox _icon = new();
    private readonly UnderlineTextBox _color = new();
    private readonly Label _status = new();
    private int? _selectedId;

    public CategoriesView(ICategoryApi categoryApi)
    {
        _categoryApi = categoryApi;
        BuildUI();
        _ = LoadCategoriesAsync();
    }

    private void BuildUI()
    {
        BackColor = Theme.Background;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(20)
        };

        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));

        _grid.Dock = DockStyle.Fill;
        _grid.AutoGenerateColumns = false;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.ReadOnly = true;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.BackgroundColor = Theme.Background;
        _grid.DefaultCellStyle.Font = Theme.BodyFont;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Id", DataPropertyName = "Id", Width = 50 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", DataPropertyName = "Name", Width = 170 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Slug", DataPropertyName = "Slug", Width = 170 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Icon",
            DataPropertyName = "Icon",
            Width = 70,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Segoe UI Emoji", 10F, FontStyle.Regular)
            }
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Color", DataPropertyName = "Color", Width = 90 });
        _grid.SelectionChanged += (_, _) => SelectCurrentCategory();

        var form = new CardPanel
        {
            Dock = DockStyle.Top,
            Height = 330,
            Padding = new Padding(16)
        };

        var formLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };

        formLayout.Controls.Add(new Label
        {
            Text = "Maintain categories",
            Font = Theme.TitleFont,
            ForeColor = Theme.TextPrimary,
            Width = 320,
            Height = 36
        });

        _name.PlaceholderText = "Category name";
        _name.Width = 280;
        _icon.PlaceholderText = "Icon, e.g. 📚";
        _icon.Width = 280;
        _color.PlaceholderText = "Color, e.g. #4f46e5";
        _color.Width = 280;

        formLayout.Controls.Add(_name);
        formLayout.Controls.Add(_icon);
        formLayout.Controls.Add(_color);

        var buttons = new FlowLayoutPanel
        {
            Width = 320,
            Height = 48,
            FlowDirection = FlowDirection.LeftToRight
        };

        var add = new ModernButton { Text = "Add", Width = 90 };
        var update = new ModernButton { Text = "Update", Width = 90 };
        var delete = new ModernButton { Text = "Delete", Width = 90 };

        add.Click += async (_, _) => await AddCategoryAsync();
        update.Click += async (_, _) => await UpdateCategoryAsync();
        delete.Click += async (_, _) => await DeleteCategoryAsync();

        buttons.Controls.Add(add);
        buttons.Controls.Add(update);
        buttons.Controls.Add(delete);

        _status.Width = 320;
        _status.Height = 70;
        _status.ForeColor = Theme.TextSecondary;
        _status.Text = "Categories define what users can create study sessions within.";

        formLayout.Controls.Add(buttons);
        formLayout.Controls.Add(_status);
        form.Controls.Add(formLayout);

        root.Controls.Add(_grid, 0, 0);
        root.Controls.Add(form, 1, 0);

        Controls.Add(root);
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = (await _categoryApi.GetAll()).ToList();
            _grid.DataSource = categories;
            _status.Text = $"Loaded {categories.Count} categories.";
        }
        catch (Exception ex)
        {
            _status.Text = "Could not load categories.";
            MessageBox.Show(ex.Message, "Categories failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void SelectCurrentCategory()
    {
        if (_grid.CurrentRow?.DataBoundItem is not CategoryDto category)
            return;

        _selectedId = category.Id;
        _name.TextValue = category.Name;
        _icon.TextValue = category.Icon;
        _color.TextValue = category.Color;
    }

    private async Task AddCategoryAsync()
    {
        try
        {
            await _categoryApi.Create(new CreateCategoryRequest
            {
                Name = _name.TextValue,
                Icon = _icon.TextValue,
                Color = _color.TextValue
            });

            ClearForm();
            await LoadCategoriesAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Add category failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task UpdateCategoryAsync()
    {
        if (_selectedId == null)
        {
            MessageBox.Show("Select a category first.");
            return;
        }

        try
        {
            await _categoryApi.Update(_selectedId.Value, new UpdateCategoryRequest
            {
                Name = _name.TextValue,
                Icon = _icon.TextValue,
                Color = _color.TextValue
            });

            await LoadCategoriesAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Update category failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task DeleteCategoryAsync()
    {
        if (_selectedId == null)
        {
            MessageBox.Show("Select a category first.");
            return;
        }

        if (MessageBox.Show("Delete selected category? Categories used by sessions cannot be deleted.", "Delete category", MessageBoxButtons.YesNo) != DialogResult.Yes)
            return;

        try
        {
            await _categoryApi.Delete(_selectedId.Value);
            ClearForm();
            await LoadCategoriesAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Delete category failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ClearForm()
    {
        _selectedId = null;
        _name.TextValue = "";
        _icon.TextValue = "";
        _color.TextValue = "";
    }
}
