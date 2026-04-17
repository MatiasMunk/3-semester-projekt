using JaTakTilbud.Contracts;

namespace JaTakTilbud.Http.Interfaces;

/*
 * API abstraction for all campaign-related operations.
 * This is used by the WinForms client to communicate with the backend.
 */
public interface ICampaignApi
{
    // Fetch all campaigns (used for dropdown selection).
    Task<List<CampaignDto>> GetCampaigns();

    // Create a new campaign.
    Task<CampaignDto?> CreateCampaign(CreateCampaignRequest request);

    /*
     * Add a product to a specific campaign.
     * Maps to: POST /api/campaigns/{id}/products
     */
    Task AddProductAsync(int campaignId, AddCampaignProductRequest request);
}