using StudyGroups.Contracts;
using StudyGroups.Http.Interfaces;

namespace StudyGroups.Http.Services;

public class FriendRequestApi(ApiClient apiClient) : IFriendRequestApi
{
    private readonly ApiClient _apiClient = apiClient;

    public async Task<IEnumerable<FriendRequestDto>> GetPendingIncoming(int userId)
    {
        return await _apiClient.GetAsync<IEnumerable<FriendRequestDto>>($"api/friend-requests/pending?userId={userId}")
               ?? Enumerable.Empty<FriendRequestDto>();
    }

    public async Task<IEnumerable<int>> GetFriendIds(int userId)
    {
        return await _apiClient.GetAsync<IEnumerable<int>>($"api/friend-requests/friends?userId={userId}")
               ?? Enumerable.Empty<int>();
    }

    public async Task<FriendRequestDto?> Create(CreateFriendRequestRequest request)
    {
        return await _apiClient.PostAsync<CreateFriendRequestRequest, FriendRequestDto>("api/friend-requests", request);
    }

    public async Task<FriendRequestDto?> Respond(int id, RespondFriendRequestRequest request)
    {
        return await _apiClient.PostAsync<RespondFriendRequestRequest, FriendRequestDto>($"api/friend-requests/{id}/respond", request);
    }

    public async Task RemoveFriend(int userId, int friendUserId)
    {
        await _apiClient.DeleteAsync($"api/friend-requests/friends/{friendUserId}?userId={userId}");
    }
}
