using System.ComponentModel.DataAnnotations;

namespace StudyGroups.Contracts;

public class SendPrivateMessageRequest
{
    public int SessionId { get; set; }
    public int SenderUserId { get; set; }
    public int ReceiverUserId { get; set; }

    [Required]
    public string Message { get; set; } = "";
}
