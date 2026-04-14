using Dapper;
using JaTakTilbud.Core.Common;
using JaTakTilbud.Core.Interfaces;
using JaTakTilbud.Core.Models;
using JaTakTilbud.Infrastructure.Data;

namespace JaTakTilbud.Infrastructure.Services;

public class CampaignService : ICampaignService
{
    private readonly DbConnectionFactory _factory;

    public CampaignService(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    // -----------------------------
    // GET ALL (without products)
    // -----------------------------
    public Result<IEnumerable<Campaign>> GetAll()
    {
        using var conn = _factory.Create();

        var campaigns = conn.Query<Campaign>("SELECT * FROM Campaigns");

        return Result<IEnumerable<Campaign>>.Success(campaigns);
    }

    // -----------------------------
    // GET BY ID (WITH PRODUCTS)
    // -----------------------------
    public Result<Campaign> GetById(int id)
    {
        using var conn = _factory.Create();

        var sql = @"
                SELECT 
                    c.Id, c.Title, c.Desc, c.CreatedAt, c.StartTime, c.EndTime, c.IsActive,
                    p.Id, p.Name, p.NormalPrice
                FROM Campaigns c
                LEFT JOIN CampaignProducts cp ON cp.CampaignId = c.Id
                LEFT JOIN Products p ON p.Id = cp.ProductId
                WHERE c.Id = @Id
                ";

        var campaignDict = new Dictionary<int, Campaign>();

        var result = conn.Query<Campaign, CampaignProduct, Campaign>(
            sql,
            (campaign, product) =>
            {
                if (!campaignDict.TryGetValue(campaign.Id, out var existing))
                {
                    existing = campaign;
                    existing.Products = new List<CampaignProduct>();
                    campaignDict.Add(existing.Id, existing);
                }

                if (product != null)
                {
                    existing.Products.Add(product);
                }

                return existing;
            },
            new { Id = id },
            splitOn: "Id"
        );

        var campaignResult = campaignDict.Values.FirstOrDefault();

        if (campaignResult == null)
            return Result<Campaign>.Failure("Campaign not found");

        return Result<Campaign>.Success(campaignResult);
    }

    // -----------------------------
    // CREATE
    // -----------------------------
    public Result<Campaign> Create(Campaign campaign)
    {
        if (string.IsNullOrWhiteSpace(campaign.Title))
            return Result<Campaign>.Failure("Campaign title is required");

        using var conn = _factory.Create();

        campaign.CreatedAt = DateTime.UtcNow;
        campaign.IsActive = true;

        var sql = @"
                INSERT INTO Campaigns (Title, Desc, CreatedAt, StartTime, EndTime, IsActive)
                VALUES (@Title, @Desc, @CreatedAt, @StartTime, @EndTime, @IsActive);
                SELECT CAST(SCOPE_IDENTITY() as int);
                ";

        var id = conn.QuerySingle<int>(sql, campaign);
        campaign.Id = id;

        return Result<Campaign>.Success(campaign);
    }

    // -----------------------------
    // UPDATE
    // -----------------------------
    public Result Update(Campaign campaign)
    {
        if (string.IsNullOrWhiteSpace(campaign.Title))
            return Result.Failure("Campaign title is required");

        using var conn = _factory.Create();

        var affected = conn.Execute(@"
                                    UPDATE Campaigns
                                    SET Title = @Title,
                                        Desc = @Desc,
                                        StartTime = @StartTime,
                                        EndTime = @EndTime,
                                        IsActive = @IsActive
                                    WHERE Id = @Id
                                    ", campaign);

        if (affected == 0)
            return Result.Failure("Campaign not found");

        return Result.Success();
    }

    // -----------------------------
    // DELETE
    // -----------------------------
    public Result Delete(int id)
    {
        using var conn = _factory.Create();

        // Important: delete relations first
        conn.Execute("DELETE FROM CampaignProducts WHERE CampaignId = @Id", new { Id = id });

        var affected = conn.Execute(
            "DELETE FROM Campaigns WHERE Id = @Id",
            new { Id = id });

        if (affected == 0)
            return Result.Failure("Campaign not found");

        return Result.Success();
    }

    // -----------------------------
    // ADD PRODUCT TO CAMPAIGN
    // -----------------------------
    public Result AddProduct(int campaignId, int productId)
    {
        using var conn = _factory.Create();

        var exists = conn.QueryFirstOrDefault<int>(@"
                                                    SELECT 1 FROM CampaignProducts 
                                                    WHERE CampaignId = @CampaignId AND ProductId = @ProductId
                                                    ", new { campaignId, productId });

        if (exists == 1)
            return Result.Failure("Product already in campaign");

        conn.Execute(@"
                    INSERT INTO CampaignProducts (CampaignId, ProductId)
                    VALUES (@CampaignId, @ProductId)
                    ", new { campaignId, productId });

        return Result.Success();
    }

    // -----------------------------
    // REMOVE PRODUCT FROM CAMPAIGN
    // -----------------------------
    public Result RemoveProduct(int campaignId, int productId)
    {
        using var conn = _factory.Create();

        conn.Execute(@"
                    DELETE FROM CampaignProducts
                    WHERE CampaignId = @CampaignId AND ProductId = @ProductId
                    ", new { campaignId, productId });

        return Result.Success();
    }
}