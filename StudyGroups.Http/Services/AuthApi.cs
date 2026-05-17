using StudyGroups.Contracts;
using StudyGroups.Http.Interfaces;

namespace StudyGroups.Http.Services;

public class AuthApi : IAuthApi
{
    private readonly ApiClient _api;

    public AuthApi(ApiClient api)
    {
        _api = api;
    }

    public async Task<AuthResponse?> Login(LoginRequest request)
    {
        return await _api.PostAsync<LoginRequest, AuthResponse>(
            "api/auth/login",
            request
        );
    }

    public async Task<AuthResponse?> Register(RegisterRequest request)
    {
        Console.WriteLine($"AUTH API REGISTER sending Username='{request.Username}', PasswordLength={request.Password?.Length ?? 0}");

        return await _api.PostAsync<RegisterRequest, AuthResponse>(
            "api/auth/register",
            request
        );
    }
}