using Microsoft.AspNetCore.Mvc;
using JaTakTilbud.Core.Interfaces;
using JaTakTilbud.Core.Models;
using JaTakTilbud.Contracts;

namespace JaTakTilbud.API.Controllers;

/*
 * Denne controller håndterer CRUD-operationer for kampagner.
 * Den bruger ICampaignService til at udføre forretningslogikken og returnerer passende HTTP-responser baseret på resultatet af hver operation.
 */
[ApiController]
[Route("api/[controller]")]
public class CampaignsController : ControllerBase
{
    private readonly ICampaignService _campaignService;

    public CampaignsController(ICampaignService campaignService)
    {
        _campaignService = campaignService;
    }

    // -----------------------------
    // GET: api/campaigns
    // -----------------------------
    [HttpGet]
    public IActionResult GetAll()
    {
        var result = _campaignService.GetAll();

        if (result.IsFailure)
            return BadRequest(result.Error);

        var dto = result.Value.Select(c => new CampaignDto
        {
            Id = c.Id,
            Title = c.Title,
            Desc = c.Desc,
            StartTime = c.StartTime,
            EndTime = c.EndTime,
            IsActive = c.IsActive
        });

        return Ok(dto);
    }

    // -----------------------------
    // GET: api/campaigns/{id}
    // (WITH PRODUCTS)
    // -----------------------------
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var result = _campaignService.GetById(id);

        if (result.IsFailure)
            return NotFound(result.Error);

        var c = result.Value;

        // Map the campaign to a DTO, including its products.
        // We need to check for null products before accessing their properties.
        var dto = new CampaignDto
        {
            Id = c.Id,
            Title = c.Title,
            Desc = c.Desc,
            StartTime = c.StartTime,
            EndTime = c.EndTime,
            IsActive = c.IsActive,

            // Use where to filter out any products that might be null, and then select to create the DTOs
            Products = c.Products
            .Select(p =>
            {
                if (p.Product == null)
                    return null;

                var product = p.Product;

                return new CampaignProductDto
                {
                    ProductId = p.ProductId,
                    Name = product.Name,
                    Description = product.Description,
                    NormalPrice = product.NormalPrice,
                    CampaignPrice = p.CampaignPrice,
                    Quantity = p.Quantity,
                    ReservedQuantity = p.ReservedQuantity
                };
            })
            .Where(x => x != null)
            .Cast<CampaignProductDto>()
            .ToList()
        };

        return Ok(dto);
    }

    // -----------------------------
    // POST: api/campaigns
    // -----------------------------
    [HttpPost]
    public IActionResult Create(CreateCampaignRequest request)
    {
        var campaign = new Campaign
        {
            Title = request.Title,
            Desc = request.Desc,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        var result = _campaignService.Create(campaign);

        if (result.IsFailure)
            return BadRequest(result.Error);

        var dto = new CampaignDto
        {
            Id = result.Value.Id,
            Title = result.Value.Title,
            Desc = result.Value.Desc,
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
    public IActionResult Update(int id, UpdateCampaignRequest request)
    {
        var campaign = new Campaign
        {
            Id = id,
            Title = request.Title,
            Desc = request.Desc,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsActive = request.IsActive
        };

        var result = _campaignService.Update(campaign);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }

    // -----------------------------
    // DELETE: api/campaigns/{id}
    // -----------------------------
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var result = _campaignService.Delete(id);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }

    // -----------------------------
    // POST: api/campaigns/{id}/products/{productId}
    // -----------------------------
    [HttpPost("{id}/products/{productId}")]
    public IActionResult AddProduct(int id, int productId)
    {
        var result = _campaignService.AddProduct(id, productId);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }

    // -----------------------------
    // DELETE: api/campaigns/{id}/products/{productId}
    // -----------------------------
    [HttpDelete("{id}/products/{productId}")]
    public IActionResult RemoveProduct(int id, int productId)
    {
        var result = _campaignService.RemoveProduct(id, productId);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }
}