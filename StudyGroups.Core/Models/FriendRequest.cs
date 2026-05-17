namespace StudyGroups.Core.Models;

public class FriendRequest
{
    public int Id { get; set; }
    public int RequesterUserId { get; set; }
    public string RequesterUsername { get; set; } = "";
    public int ReceiverUserId { get; set; }
    public string ReceiverUsername { get; set; } = "";
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}
