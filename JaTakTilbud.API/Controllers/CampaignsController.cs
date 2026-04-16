using Microsoft.AspNetCore.Mvc;
using JaTakTilbud.Core.Interfaces;
using JaTakTilbud.Core.Models;
using JaTakTilbud.Contracts;

namespace JaTakTilbud.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CampaignsController(ICampaignService campaignService) : ControllerBase
{
    private readonly ICampaignService _campaignService = campaignService;

    // -----------------------------
    // GET: api/campaigns
    // -----------------------------
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _campaignService.GetAllAsync();

        if (result.IsFailure)
            return BadRequest(result.Error);

        var dto = result.Value.Select(c => new CampaignDto
        {
            Id = c.Id,
            Title = c.Title,
            Desc = c.Description,
            StartTime = c.StartTime,
            EndTime = c.EndTime,
            IsActive = c.IsActive
        });

        return Ok(dto);
    }

    // -----------------------------
    // GET: api/campaigns/{id}
    // -----------------------------
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _campaignService.GetByIdAsync(id);

        if (result.IsFailure)
            return NotFound(result.Error);

        var c = result.Value;

        var dto = new CampaignDto
        {
            Id = c.Id,
            Title = c.Title,
            Desc = c.Description,
            StartTime = c.StartTime,
            EndTime = c.EndTime,
            IsActive = c.IsActive,

            Products = c.Products?
                .Where(p => p.Product != null)
                .Select(p => new CampaignProductDto
                {
                    ProductId = p.ProductId,
                    Name = p.Product!.Name,
                    Description = p.Product.Description,
                    NormalPrice = p.Product.Price,

                    CampaignPrice = p.CampaignPrice,
                    Quantity = p.Quantity,
                    ReservedQuantity = p.ReservedQuantity
                })
                .ToList()
        };

        return Ok(dto);
    }

    // -----------------------------
    // POST: api/campaigns
    // -----------------------------
    [HttpPost]
    public async Task<IActionResult> Create(CreateCampaignRequest request)
    {
        var campaign = new Campaign
        {
            Title = request.Title,
            Description = request.Desc,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        var result = await _campaignService.CreateAsync(campaign);

        if (result.IsFailure)
            return BadRequest(result.Error);

        var dto = new CampaignDto
        {
            Id = result.Value.Id,
            Title = result.Value.Title,
            Desc = result.Value.Description,
            StartTime = result.Value.StartTime,
            EndTime = result.Value.EndTime,
            IsActive = result.Value.IsActive
        };

        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    // -----------------------------
    // PUT: api/campaigns/{id}
    // -----------------------------
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateCampaignRequest request)
    {
        var campaign = new Campaign
        {
            Id = id,
            Title = request.Title,
            Description = request.Desc,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsActive = request.IsActive
        };

        var result = await _campaignService.UpdateAsync(campaign);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }

    // -----------------------------
    // DELETE: api/campaigns/{id}
    // -----------------------------
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _campaignService.DeleteAsync(id);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }

    // -----------------------------
    // POST: api/campaigns/{id}/products
    // -----------------------------
    [HttpPost("{id}/products")]
    public async Task<IActionResult> AddProduct(int id, AddCampaignProductRequest request)
    {
        var result = await _campaignService.AddProductAsync(
            id,
            request.ProductId,
            request.CampaignPrice,
            request.Quantity
        );

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }

    // -----------------------------
    // DELETE: api/campaigns/{id}/products/{productId}
    // -----------------------------
    [HttpDelete("{id}/products/{productId}")]
    public async Task<IActionResult> RemoveProduct(int id, int productId)
    {
        var result = await _campaignService.RemoveProductAsync(id, productId);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }
}