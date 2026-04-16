using Dapper;
using JaTakTilbud.Core.Common;
using JaTakTilbud.Core.Interfaces;
using JaTakTilbud.Core.Models;
using JaTakTilbud.Contracts;
using JaTakTilbud.Infrastructure.Data;

namespace JaTakTilbud.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly DbConnectionFactory _factory;

    public ProductService(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<Result<IEnumerable<Product>>> GetAllAsync()
    {
        using var conn = _factory.Create();

        var products = await conn.QueryAsync<Product>(@"
            SELECT 
                p.Id,
                p.name        AS Name,
                p.description AS Description,
                ISNULL(pp.amount, 0) AS Price
            FROM Products p
            LEFT JOIN ProductPrices pp 
                ON pp.productId_FK = p.Id
                AND pp.validTo IS NULL
        ");

        return Result<IEnumerable<Product>>.Success(products);
    }

    public async Task<Result<Product>> GetByIdAsync(int id)
    {
        using var conn = _factory.Create();

        var product = await conn.QueryFirstOrDefaultAsync<Product>(@"
            SELECT 
                p.Id,
                p.name        AS Name,
                p.description AS Description,
                ISNULL(pp.amount, 0) AS Price
            FROM Products p
            LEFT JOIN ProductPrices pp 
                ON pp.productId_FK = p.Id
                AND pp.validTo IS NULL
            WHERE p.Id = @Id
        ", new { Id = id });

        if (product == null)
            return Result<Product>.Failure("Product not found");

        return Result<Product>.Success(product);
    }

    public async Task<Result<Product>> CreateAsync(CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<Product>.Failure("Product name is required");

        if (request.Price <= 0)
            return Result<Product>.Failure("Price must be greater than 0");

        using var conn = _factory.Create();
        using var transaction = conn.BeginTransaction();

        try
        {
            // 1. Insert product
            var productId = await conn.QuerySingleAsync<int>(@"
            INSERT INTO Products (name, description, isActive)
            VALUES (@Name, @Description, 1);

            SELECT CAST(SCOPE_IDENTITY() as int);
        ", request, transaction);

            // 2. Insert price
            await conn.ExecuteAsync(@"
            INSERT INTO ProductPrices (productId_FK, amount, validFrom)
            VALUES (@ProductId, @Price, GETDATE())
        ", new
            {
                ProductId = productId,
                request.Price
            }, transaction);

            transaction.Commit();

            // 3. Return created product (important!)
            var createdProduct = new Product
            {
                Id = productId,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price
            };

            return Result<Product>.Success(createdProduct);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<Result> UpdateAsync(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
            return Result.Failure("Product name is required");

        using var conn = _factory.Create();
        using var transaction = conn.BeginTransaction();

        var affected = await conn.ExecuteAsync(@"
            UPDATE Products
            SET name = @Name,
                description = @Description
            WHERE Id = @Id
        ", product, transaction);

        if (affected == 0)
            return Result.Failure("Product not found");

        await conn.ExecuteAsync(@"
            UPDATE ProductPrices
            SET validTo = GETDATE()
            WHERE productId_FK = @Id
            AND validTo IS NULL
        ", new { product.Id }, transaction);

        if (product.Price > 0)
        {
            await conn.ExecuteAsync(@"
                INSERT INTO ProductPrices (productId_FK, amount, validFrom)
                VALUES (@ProductId, @Price, GETDATE())
            ", new
            {
                ProductId = product.Id,
                product.Price
            }, transaction);
        }

        transaction.Commit();

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        using var conn = _factory.Create();

        var affected = await conn.ExecuteAsync(@"
            DELETE FROM Products WHERE Id = @Id
        ", new { Id = id });

        if (affected == 0)
            return Result.Failure("Product not found");

        return Result.Success();
    }
}