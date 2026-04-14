namespace JaTakTilbud.Contracts;

public class CreateCampaignRequest
{
    public string Title { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;

    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}