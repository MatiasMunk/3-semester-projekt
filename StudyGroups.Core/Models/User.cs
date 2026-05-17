namespace StudyGroups.Core.Models;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = "";
    public string Email { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    // Optional: if you later add auth
    public string? PasswordHash { get; set; }

    public List<SessionParticipant>? Sessions { get; set; }
}