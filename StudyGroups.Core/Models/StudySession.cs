namespace StudyGroups.Core.Models;

public class StudySession
{
    public int Id { get; set; }

    // -----------------------------
    // BASIC INFO
    // -----------------------------
    public string Title { get; set; } = "";
    public string? Description { get; set; }

    // -----------------------------
    // TOPIC (replaces Subject)
    // -----------------------------
    public int TopicId { get; set; }
    public Topic? Topic { get; set; }

    // -----------------------------
    // LOCATION / TIME
    // -----------------------------
    public string? Location { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    // -----------------------------
    // PARTICIPATION
    // -----------------------------
    public int MaxParticipants { get; set; }
    public int CurrentParticipants { get; set; }

    public List<SessionParticipant> Participants { get; set; } = new();

    // -----------------------------
    // CREATOR
    // -----------------------------
    public int CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    // -----------------------------
    // METADATA
    // -----------------------------
    public DateTime CreatedAt { get; set; }

    // -----------------------------
    // HELPER PROPERTIES (NOT MAPPED)
    // -----------------------------
    public bool IsFull => CurrentParticipants >= MaxParticipants;

    public int SpotsLeft => MaxParticipants - CurrentParticipants;

    public bool IsActive => StartTime > DateTime.UtcNow;
}