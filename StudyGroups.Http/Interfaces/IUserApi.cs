using StudyGroups.Contracts;

namespace StudyGroups.Http.Interfaces;

public interface IUserApi
{
    Task<IEnumerable<UserDto>> GetAll();
}
