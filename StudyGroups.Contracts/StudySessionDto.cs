namespace StudyGroups.Contracts;

public class StudySessionDto
{
    public int Id { get; set; }

    public string Title { get; set; } = "";
    public string Description { get; set; } = "";

    public int TopicId { get; set; }
    public string TopicName { get; set; } = "";
    public string TopicIcon { get; set; } = "";

    public string Location { get; set; } = "";

    public DateTime StartTime { get; set; }

    public int MaxParticipants { get; set; }
    public int CurrentParticipants { get; set; }

    public int CreatedByUserId { get; set; }

    public List<SessionParticipantDto>? Participants { get; set; }
}

public class SessionParticipantDto
{
    public int UserId { get; set; }
    public string? Username { get; set; }
}