using StudyGroups.Core.Models;

namespace StudyGroups.Core.Interfaces;

public interface IFriendRequestService
{
    Task<Result<IEnumerable<FriendRequest>>> GetPendingIncomingAsync(int receiverUserId);
    Task<Result<IEnumerable<int>>> GetFriendIdsAsync(int userId);
    Task<Result<FriendRequest>> CreateAsync(int requesterUserId, int receiverUserId);
    Task<Result<FriendRequest>> RespondAsync(int requestId, int receiverUserId, bool accept);
    Task<Result> RemoveFriendAsync(int userId, int friendUserId);
}
