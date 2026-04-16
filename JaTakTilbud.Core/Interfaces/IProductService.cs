using JaTakTilbud.Core.Common;
using JaTakTilbud.Core.Models;
using JaTakTilbud.Contracts;

namespace JaTakTilbud.Core.Interfaces;

/// <summary>
/// Business logic layer for managing products.
/// </summary>
public interface IProductService
{
    // --------------------------------------------------
    // GET ALL PRODUCTS
    // --------------------------------------------------
    Task<Result<IEnumerable<Product>>> GetAllAsync();

    // --------------------------------------------------
    // GET PRODUCT BY ID
    // --------------------------------------------------
    Task<Result<Product>> GetByIdAsync(int id);

    // --------------------------------------------------
    // CREATE PRODUCT
    // --------------------------------------------------
    Task<Result<Product>> CreateAsync(CreateProductRequest request);

    // --------------------------------------------------
    // UPDATE PRODUCT
    // --------------------------------------------------
    Task<Result> UpdateAsync(Product product);

    // --------------------------------------------------
    // DELETE PRODUCT
    // --------------------------------------------------
    Task<Result> DeleteAsync(int id);
}