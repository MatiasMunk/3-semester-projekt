using StudyGroups.Core.Models;

namespace StudyGroups.Core.Interfaces;

public interface IPrivateMessageService
{
    Task<Result<IEnumerable<PrivateMessage>>> GetConversationAsync(int sessionId, int userId, int otherUserId);
    Task<Result<IEnumerable<PrivateMessage>>> GetUnreadAsync(int sessionId, int userId);
    Task<Result<PrivateMessage>> SendAsync(PrivateMessage message);
}
