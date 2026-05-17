namespace StudyGroups.Contracts;

public class CreateStudySessionRequest
{
    public string Title { get; set; } = "";
    public string? Description { get; set; }

    public int TopicId { get; set; }

    public string? Location { get; set; }

    public DateTime StartTime { get; set; }

    public int MaxParticipants { get; set; }

    public int UserId { get; set; }
}