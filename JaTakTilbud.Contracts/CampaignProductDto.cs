namespace JaTakTilbud.Contracts;

public class CampaignProductDto
{
    public int ProductId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public decimal NormalPrice { get; set; }
    public decimal CampaignPrice { get; set; }

    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }

    public decimal Savings => NormalPrice - CampaignPrice;
}