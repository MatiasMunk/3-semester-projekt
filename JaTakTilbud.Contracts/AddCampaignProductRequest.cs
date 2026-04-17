namespace JaTakTilbud.Contracts;

public class AddCampaignProductRequest
{
    public int ProductId { get; set; }

    public decimal CampaignPrice { get; set; }

    public int Quantity { get; set; }

    public byte[]? ImageBlob { get; set; }
}