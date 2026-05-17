using StudyGroups.Contracts;

namespace StudyGroups.Http.Interfaces;

public interface IAuthApi
{
    Task<AuthResponse?> Login(LoginRequest request);
    Task<AuthResponse?> Register(RegisterRequest request);
}