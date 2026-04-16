using JaTakTilbud.Contracts;

namespace JaTakTilbud.Http.Interfaces;

public interface IProductApi
{
    Task<List<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(int id);

    Task<ProductDto> CreateAsync(CreateProductRequest request);
}