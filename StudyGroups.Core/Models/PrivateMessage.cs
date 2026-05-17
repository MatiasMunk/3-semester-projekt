namespace StudyGroups.Core.Models;

public class PrivateMessage
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public int SenderUserId { get; set; }
    public string SenderUsername { get; set; } = "";
    public int ReceiverUserId { get; set; }
    public string ReceiverUsername { get; set; } = "";
    public string Message { get; set; } = "";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
