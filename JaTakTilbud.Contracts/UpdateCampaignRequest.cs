namespace JaTakTilbud.Contracts;

public class UpdateCampaignRequest
{
    public string Title { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;

    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public bool IsActive { get; set; }
}