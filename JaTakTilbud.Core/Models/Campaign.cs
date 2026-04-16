namespace JaTakTilbud.Core.Models;

public class Campaign
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public bool IsActive { get; set; } = true;

    public List<CampaignProduct> Products { get; set; } = new();
}