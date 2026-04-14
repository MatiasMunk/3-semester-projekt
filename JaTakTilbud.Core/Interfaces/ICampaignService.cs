using JaTakTilbud.Core.Common;
using JaTakTilbud.Core.Models;

namespace JaTakTilbud.Core.Interfaces;

public interface ICampaignService
{
    // -----------------------------
    // Campaign CRUD
    // -----------------------------
    Result<IEnumerable<Campaign>> GetAll();

    Result<Campaign> GetById(int id);

    Result<Campaign> Create(Campaign campaign);

    Result Update(Campaign campaign);

    Result Delete(int id);

    // -----------------------------
    // Campaign - Product relations
    // -----------------------------
    Result AddProduct(int campaignId, int productId);

    Result RemoveProduct(int campaignId, int productId);
}