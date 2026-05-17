using StudyGroups.Contracts;
using StudyGroups.Http.Interfaces;

namespace StudyGroups.Http.Services;

public class StudySessionApi : IStudySessionApi
{
    private readonly ApiClient _apiClient;

    public StudySessionApi(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // =========================================================
    // GET ALL SESSIONS
    // =========================================================
    public async Task<IEnumerable<StudySessionDto>?> GetAll(string? category)
{
    var endpoint = "api/studysessions";

    if (!string.IsNullOrWhiteSpace(category))
        endpoint += $"?category={category}";

    return await _apiClient.GetAsync<IEnumerable<StudySessionDto>>(endpoint);
}

    // =========================================================
    // GET SESSION BY ID
    // =========================================================
    public async Task<StudySessionDto?> GetById(int sessionId)
    {
        return await _apiClient.GetAsync<StudySessionDto>($"api/studysessions/{sessionId}");
    }

    // =========================================================
    // CREATE SESSION
    // =========================================================
    public async Task Create(CreateStudySessionRequest request)
    {
        await _apiClient.PostAsync<CreateStudySessionRequest, object?>(
            "api/studysessions",
            request
        );
    }

    // =========================================================
    // JOIN SESSION
    // =========================================================
    public async Task Join(int sessionId, int userId)
    {
        var request = new JoinSessionRequest
        {
            UserId = userId
        };

        await _apiClient.PostAsync<JoinSessionRequest, object?>(
            $"api/studysessions/{sessionId}/join",
            request
        );
    }

    // =========================================================
    // LEAVE SESSION (recommended)
    // =========================================================
    public async Task Leave(int sessionId, int userId)
    {
        var request = new LeaveSessionRequest
        {
            UserId = userId
        };

        await _apiClient.PostAsync<LeaveSessionRequest, object?>(
            $"api/studysessions/{sessionId}/leave",
            request
        );
    }
}