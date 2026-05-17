namespace StudyGroups.Contracts;

public class UpdateStudySessionRequest
{
    public string Title { get; set; } = "";
    public string? Description { get; set; }

    public int TopicId { get; set; }

    public string? Location { get; set; }

    public DateTime StartTime { get; set; }

    public int MaxParticipants { get; set; }
}