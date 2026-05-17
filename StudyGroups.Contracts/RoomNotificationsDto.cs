namespace StudyGroups.Contracts;

public class RoomNotificationsDto
{
    public IEnumerable<FriendRequestDto> PendingFriendRequests { get; set; } = Enumerable.Empty<FriendRequestDto>();
    public IEnumerable<PrivateMessageDto> UnreadPrivateMessages { get; set; } = Enumerable.Empty<PrivateMessageDto>();
    public IEnumerable<int> FriendUserIds { get; set; } = Enumerable.Empty<int>();
}
