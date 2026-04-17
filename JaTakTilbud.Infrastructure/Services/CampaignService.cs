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

    // =========================================================
    // GET ALL (without products)
    // =========================================================
    public async Task<Result<IEnumerable<Campaign>>> GetAllAsync()
    {
        using var conn = await _factory.CreateOpenAsync();

        var campaigns = await conn.QueryAsync<Campaign>(@"
            SELECT
                Id,
                title       AS Title,
                description AS Description,
                createdAt   AS CreatedAt,
                startTime   AS StartTime,
                endTime     AS EndTime,
                isActive    AS IsActive
            FROM Campaigns
        ");

        return Result<IEnumerable<Campaign>>.Success(campaigns);
    }

    // =========================================================
    // GET BY ID (WITH PRODUCTS + CORRECT PRICE)
    // =========================================================
    public async Task<Result<Campaign>> GetByIdAsync(int id)
    {
        using var conn = await _factory.CreateOpenAsync();

        var sql = @"
            SELECT 
                c.Id,
                c.title       AS Title,
                c.description AS Description,
                c.createdAt   AS CreatedAt,
                c.startTime   AS StartTime,
                c.endTime     AS EndTime,
                c.isActive    AS IsActive,

                cp.Id,
                cp.offerPrice       AS OfferPrice,
                cp.quantity         AS Quantity,
                cp.reservedQuantity AS ReservedQuantity,
                cp.campaignId_FK,
                cp.productId_FK,

                p.Id,
                p.name        AS Name,
                p.description AS Description,
                ISNULL(pp.amount, 0) AS Price

            FROM Campaigns c
            LEFT JOIN CampaignProducts cp ON cp.campaignId_FK = c.Id
            LEFT JOIN Products p ON p.Id = cp.productId_FK
            LEFT JOIN ProductPrices pp 
                ON pp.productId_FK = p.Id
                AND pp.validTo IS NULL
            WHERE c.Id = @Id
        ";

        var campaignDict = new Dictionary<int, Campaign>();

        var result = await conn.QueryAsync<Campaign, CampaignProduct, Product, Campaign>(
            sql,
            (campaign, cp, product) =>
            {
                if (!campaignDict.TryGetValue(campaign.Id, out var existing))
                {
                    existing = campaign;
                    existing.Products = new List<CampaignProduct>();
                    campaignDict.Add(existing.Id, existing);
                }

                if (cp != null && product != null)
                {
                    cp.Product = product;
                    existing.Products.Add(cp);
                }

                return existing;
            },
            new { Id = id },
            splitOn: "Id,Id"
        );

        var campaignResult = campaignDict.Values.FirstOrDefault();

        if (campaignResult == null)
            return Result<Campaign>.Failure("Campaign not found");

        return Result<Campaign>.Success(campaignResult);
    }

    // =========================================================
    // CREATE
    // =========================================================
    public async Task<Result<Campaign>> CreateAsync(Campaign campaign)
    {
        if (string.IsNullOrWhiteSpace(campaign.Title))
            return Result<Campaign>.Failure("Campaign title is required");

        using var conn = await _factory.CreateOpenAsync();

        campaign.CreatedAt = DateTime.UtcNow;
        campaign.IsActive = true;

        var sql = @"
            INSERT INTO Campaigns 
                (title, description, createdAt, startTime, endTime, isActive)
            VALUES 
                (@Title, @Description, @CreatedAt, @StartTime, @EndTime, @IsActive);

            SELECT CAST(SCOPE_IDENTITY() as int);
        ";

        var id = await conn.QuerySingleAsync<int>(sql, campaign);
        campaign.Id = id;

        return Result<Campaign>.Success(campaign);
    }

    // =========================================================
    // UPDATE
    // =========================================================
    public async Task<Result> UpdateAsync(Campaign campaign)
    {
        if (string.IsNullOrWhiteSpace(campaign.Title))
            return Result.Failure("Campaign title is required");

        using var conn = await _factory.CreateOpenAsync();

        var affected = await conn.ExecuteAsync(@"
            UPDATE Campaigns
            SET 
                title = @Title,
                description = @Description,
                startTime = @StartTime,
                endTime = @EndTime,
                isActive = @IsActive
            WHERE Id = @Id
        ", campaign);

        if (affected == 0)
            return Result.Failure("Campaign not found");

        return Result.Success();
    }

    // =========================================================
    // DELETE
    // =========================================================
    public async Task<Result> DeleteAsync(int id)
    {
        using var conn = await _factory.CreateOpenAsync();

        await conn.ExecuteAsync(@"
            DELETE FROM CampaignProducts 
            WHERE campaignId_FK = @Id
        ", new { Id = id });

        var affected = await conn.ExecuteAsync(@"
            DELETE FROM Campaigns 
            WHERE Id = @Id
        ", new { Id = id });

        if (affected == 0)
            return Result.Failure("Campaign not found");

        return Result.Success();
    }

    // =========================================================
    // ADD PRODUCT TO CAMPAIGN (FULL INSERT)
    // =========================================================
    public async Task<Result> AddProductAsync(
        int campaignId,
        int productId,
        decimal offerPrice,
        int quantity,
        byte[]? imageBlob = null)
    {
        using var conn = await _factory.CreateOpenAsync();

        // Prevent duplicates
        var exists = await conn.QueryFirstOrDefaultAsync<int>(@"
            SELECT 1 
            FROM CampaignProducts 
            WHERE campaignId_FK = @campaignId AND productId_FK = @productId
        ", new { campaignId, productId });

        if (exists == 1)
            return Result.Failure("Product already in campaign");

        await conn.ExecuteAsync(@"
            INSERT INTO CampaignProducts 
            (
                campaignId_FK,
                productId_FK,
                offerPrice,
                quantity,
                reservedQuantity,
                imageBlob
            )
            VALUES 
            (
                @campaignId,
                @productId,
                @offerPrice,
                @quantity,
                @reservedQuantity,
                @imageBlob
            )
        ", new
        {
            campaignId,
            productId,
            offerPrice,
            quantity,
            reservedQuantity = 0,
            imageBlob
        });

        return Result.Success();
    }

    // =========================================================
    // REMOVE PRODUCT FROM CAMPAIGN
    // =========================================================
    public async Task<Result> RemoveProductAsync(int campaignId, int productId)
    {
        using var conn = await _factory.CreateOpenAsync();

        await conn.ExecuteAsync(@"
            DELETE FROM CampaignProducts
            WHERE campaignId_FK = @campaignId AND productId_FK = @productId
        ", new { campaignId, productId });

        return Result.Success();
    }
}