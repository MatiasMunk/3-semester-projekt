using JaTakTilbud.Contracts;
using JaTakTilbud.Http.Interfaces;

namespace JaTakTilbud.Http.Services;

/// <summary>
/// Handles HTTP calls to Campaign endpoints.
/// </summary>
public class CampaignApi : ICampaignApi
{
    private readonly ApiClient _client;

    public CampaignApi(ApiClient client)
    {
        _client = client;
    }

    // =============================
    // GET ALL CAMPAIGNS
    // =============================
    public async Task<List<CampaignDto>> GetCampaigns()
    {
        return await _client.GetAsync<List<CampaignDto>>("api/campaigns")
               ?? new List<CampaignDto>();
    }

    // =============================
    // CREATE CAMPAIGN
    // =============================
    public async Task<CampaignDto?> CreateCampaign(CreateCampaignRequest request)
    {
        return await _client.PostAsync<CreateCampaignRequest, CampaignDto>("api/campaigns", request);
    }

    // =============================
    // ADD PRODUCT TO CAMPAIGN (IMPORTANT)
    // =============================
    public async Task AddProductAsync(int campaignId, AddCampaignProductRequest request)
    {
        await _client.PostAsync<AddCampaignProductRequest, object>(
            $"api/campaigns/{campaignId}/products",
            request
        );
    }
}