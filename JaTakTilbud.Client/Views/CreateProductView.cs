using JaTakTilbud.Contracts;
using JaTakTilbud.Http.Interfaces;

namespace JaTakTilbud.Client.Views;

/// <summary>
/// View used by admin to create new base products.
/// These products are later used in campaigns.
/// </summary>
public partial class CreateProductView : UserControl
{
    private readonly IProductApi _productApi;

    public CreateProductView(IProductApi productApi)
    {
        InitializeComponent();
        _productApi = productApi;
    }

    /// <summary>
    /// Handles create button click
    /// </summary>
    private async void btnCreate_Click(object sender, EventArgs e)
    {
        try
        {
            var request = new CreateProductRequest
            {
                Name = txtName.TextValue.Trim(),
                Description = txtDescription.TextValue.Trim(),
                Price = decimal.TryParse(txtPrice.TextValue, out var price) ? price : 0,
                ImageUrl = txtImageUrl.TextValue.Trim()
            };

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                MessageBox.Show("Name is required.");
                return;
            }

            await _productApi.CreateAsync(request);

            MessageBox.Show("Product created successfully.");

            ClearForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error creating product: {ex.Message}");
        }
    }

    private void ClearForm()
    {
        txtName.TextValue = "";
        txtDescription.TextValue = "";
        txtPrice.TextValue = "";
        txtImageUrl.TextValue = "";
    }
}