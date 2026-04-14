namespace JaTakTilbud.Contracts;

/*
 * DTO'er hjælper med at:
 * - Isolere forretningslogikken fra API-laget
 * - Forenkle dataoverførslen ved kun at inkludere nødvendige felter
 * - Forbedre sikkerheden ved ikke at eksponere hele domænemodellen
 */
public class CampaignDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;

    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public bool IsActive { get; set; }

    public List<CampaignProductDto>? Products { get; set; }
}