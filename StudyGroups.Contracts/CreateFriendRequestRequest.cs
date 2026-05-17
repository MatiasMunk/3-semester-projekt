namespace StudyGroups.Contracts;

public class CreateFriendRequestRequest
{
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
}
