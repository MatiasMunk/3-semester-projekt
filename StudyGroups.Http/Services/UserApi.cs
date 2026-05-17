using StudyGroups.Contracts;
using StudyGroups.Http.Interfaces;

namespace StudyGroups.Http.Services;

public class UserApi(ApiClient apiClient) : IUserApi
{
    private readonly ApiClient _apiClient = apiClient;

    public async Task<IEnumerable<UserDto>> GetAll()
    {
        return await _apiClient.GetAsync<IEnumerable<UserDto>>("api/users")
               ?? Enumerable.Empty<UserDto>();
    }
}
