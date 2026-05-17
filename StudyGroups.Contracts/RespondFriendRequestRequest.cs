namespace StudyGroups.Contracts;

public class RespondFriendRequestRequest
{
    public int ReceiverUserId { get; set; }
    public bool Accept { get; set; }
}
