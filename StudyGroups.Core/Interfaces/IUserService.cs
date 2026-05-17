using StudyGroups.Core.Models;

namespace StudyGroups.Core.Interfaces;

public interface IUserService
{
    Task<Result<IEnumerable<User>>> GetAllAsync();
}
