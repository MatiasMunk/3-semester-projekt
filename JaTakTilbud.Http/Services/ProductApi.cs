using JaTakTilbud.Contracts;
using JaTakTilbud.Http.Interfaces;

namespace JaTakTilbud.Http.Services;

/// <summary>
/// Handles API calls related to products.
/// Uses ApiClient abstraction instead of raw HttpClient.
/// </summary>
public class ProductApi : IProductApi
{
    private readonly ApiClient _client;

    public ProductApi(ApiClient client)
    {
        _client = client;
    }

    public async Task<List<ProductDto>> GetAllAsync()
    {
        return await _client.GetAsync<List<ProductDto>>("api/products")
               ?? new List<ProductDto>();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        return await _client.GetAsync<ProductDto>($"api/products/{id}");
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request)
    {
        var result = await _client.PostAsync<CreateProductRequest, ProductDto>(
            "api/products",
            request
        );

        return result ?? throw new Exception("Failed to create product");
    }
}