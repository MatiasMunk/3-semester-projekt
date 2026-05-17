using StudyGroups.Contracts;
using StudyGroups.Http.Interfaces;

namespace StudyGroups.Http.Services;

public class PrivateMessageApi(ApiClient apiClient) : IPrivateMessageApi
{
    private readonly ApiClient _apiClient = apiClient;

    public async Task<IEnumerable<PrivateMessageDto>> GetConversation(int sessionId, int userId, int otherUserId)
    {
        return await _apiClient.GetAsync<IEnumerable<PrivateMessageDto>>(
                   $"api/private-messages?sessionId={sessionId}&userId={userId}&otherUserId={otherUserId}")
               ?? Enumerable.Empty<PrivateMessageDto>();
    }

    public async Task<IEnumerable<PrivateMessageDto>> GetUnread(int sessionId, int userId)
    {
        return await _apiClient.GetAsync<IEnumerable<PrivateMessageDto>>(
                   $"api/private-messages/unread?sessionId={sessionId}&userId={userId}")
               ?? Enumerable.Empty<PrivateMessageDto>();
    }

    public async Task<PrivateMessageDto?> Send(SendPrivateMessageRequest request)
    {
        return await _apiClient.PostAsync<SendPrivateMessageRequest, PrivateMessageDto>("api/private-messages", request);
    }
}
