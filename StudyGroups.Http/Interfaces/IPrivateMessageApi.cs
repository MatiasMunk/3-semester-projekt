using StudyGroups.Contracts;

namespace StudyGroups.Http.Interfaces;

public interface IPrivateMessageApi
{
    Task<IEnumerable<PrivateMessageDto>> GetConversation(int sessionId, int userId, int otherUserId);
    Task<IEnumerable<PrivateMessageDto>> GetUnread(int sessionId, int userId);
    Task<PrivateMessageDto?> Send(SendPrivateMessageRequest request);
}
