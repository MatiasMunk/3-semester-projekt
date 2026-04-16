using JaTakTilbud.Contracts;
using JaTakTilbud.Http.Interfaces;

namespace JaTakTilbud.Http.Services;

public class CampaignApi : ICampaignApi
{
    private readonly ApiClient _client;

    public CampaignApi(ApiClient client)
    {
        _client = client;
    }

    public async Task<List<CampaignDto>> GetCampaigns()
    {
        return await _client.GetAsync<List<CampaignDto>>("campaigns")
               ?? new List<CampaignDto>();
    }

    public async Task<CampaignDto?> CreateCampaign(CreateCampaignRequest request)
    {
        return await _client.PostAsync<CreateCampaignRequest, CampaignDto>("campaigns", request);
    }
}