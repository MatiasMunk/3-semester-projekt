using JaTakTilbud.Core.Common;
using JaTakTilbud.Core.Models;

namespace JaTakTilbud.Core.Interfaces;

public interface ICampaignService
{
    // -----------------------------
    // GET ALL CAMPAIGNS (no products)
    // -----------------------------
    Task<Result<IEnumerable<Campaign>>> GetAllAsync();

    // -----------------------------
    // GET CAMPAIGN BY ID (WITH PRODUCTS)
    // -----------------------------
    Task<Result<Campaign>> GetByIdAsync(int id);

    // -----------------------------
    // CREATE CAMPAIGN
    // -----------------------------
    Task<Result<Campaign>> CreateAsync(Campaign campaign);

    // -----------------------------
    // UPDATE CAMPAIGN
    // -----------------------------
    Task<Result> UpdateAsync(Campaign campaign);

    // -----------------------------
    // DELETE CAMPAIGN
    // -----------------------------
    Task<Result> DeleteAsync(int id);

    // -----------------------------
    // ADD PRODUCT TO CAMPAIGN
    // -----------------------------
    Task<Result> AddProductAsync(
        int campaignId,
        int productId,
        decimal offerPrice,
        int quantity
    );

    // -----------------------------
    // REMOVE PRODUCT FROM CAMPAIGN
    // -----------------------------
    Task<Result> RemoveProductAsync(int campaignId, int productId);
}