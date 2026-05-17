namespace StudyGroups.Core.Models;

public class SessionParticipant
{
    public int Id { get; set; }

    public int SessionId { get; set; }
    public StudySession? Session { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public DateTime JoinedAt { get; set; }
}