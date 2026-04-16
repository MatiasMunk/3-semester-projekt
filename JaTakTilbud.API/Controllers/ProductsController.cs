using JaTakTilbud.Contracts;
using JaTakTilbud.Core.Interfaces;
using JaTakTilbud.Core.Models;
using JaTakTilbud.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace JaTakTilbud.API.Controllers;

/// <summary>
/// Handles CRUD operations for products.
/// Products are base entities used in campaigns.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    // --------------------------------------------------
    // GET ALL PRODUCTS
    // --------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _service.GetAllAsync();

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        var dto = result.Value.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price
        });

        return Ok(dto);
    }

    // --------------------------------------------------
    // GET PRODUCT BY ID
    // --------------------------------------------------
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (!result.IsSuccess)
            return NotFound(result.Error);

        var p = result.Value;

        var dto = new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price
        };

        return Ok(dto);
    }

    // --------------------------------------------------
    // CREATE PRODUCT
    // --------------------------------------------------
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            Price = request.Price
        };

        var result = await _service.CreateAsync(request);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        var created = result.Value;

        return Ok(new ProductDto
        {
            Id = created.Id,
            Name = created.Name,
            Price = created.Price
        });
    }

    // --------------------------------------------------
    // UPDATE PRODUCT
    // --------------------------------------------------
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateProductRequest request)
    {
        var existing = await _service.GetByIdAsync(id);

        if (!existing.IsSuccess)
            return NotFound(existing.Error);

        var product = existing.Value;

        product.Name = request.Name;
        product.Description = request.Description ?? string.Empty;
        product.Price = request.Price;

        var result = await _service.UpdateAsync(product);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok();
    }

    // --------------------------------------------------
    // DELETE PRODUCT
    // --------------------------------------------------
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);

        if (!result.IsSuccess)
            return NotFound(result.Error);

        return Ok();
    }
}