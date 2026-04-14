namespace JaTakTilbud.Core.Models;

public class CampaignProduct
{
    public int CampaignId { get; set; }
    public int ProductId { get; set; }

    public decimal CampaignPrice { get; set; }

    public int Quantity { get; set; }           // total allocated to campaign
    public int ReservedQuantity { get; set; }   // how many already reserved by customers by presseing "Ja Tak"

    public Product? Product { get; set; }
}