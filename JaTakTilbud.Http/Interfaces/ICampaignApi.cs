using JaTakTilbud.Contracts;

namespace JaTakTilbud.Http.Interfaces;

public interface ICampaignApi
{
    Task<List<CampaignDto>> GetCampaigns();
    Task<CampaignDto?> CreateCampaign(CreateCampaignRequest request);
}