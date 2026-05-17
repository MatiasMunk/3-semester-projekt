using StudyGroups.Contracts;

namespace StudyGroups.Http.Interfaces;

public interface IFriendRequestApi
{
    Task<IEnumerable<FriendRequestDto>> GetPendingIncoming(int userId);
    Task<IEnumerable<int>> GetFriendIds(int userId);
    Task<FriendRequestDto?> Create(CreateFriendRequestRequest request);
    Task<FriendRequestDto?> Respond(int id, RespondFriendRequestRequest request);
    Task RemoveFriend(int userId, int friendUserId);
}
