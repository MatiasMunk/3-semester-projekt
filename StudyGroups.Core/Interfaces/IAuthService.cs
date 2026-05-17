using StudyGroups.Core.Models;

namespace StudyGroups.Core.Interfaces;

public interface IAuthService
{
    Task<Result<User>> RegisterAsync(string username, string password);
    Task<Result<User>> LoginAsync(string username, string password);
}