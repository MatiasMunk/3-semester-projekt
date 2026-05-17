using StudyGroups.Core.Models;

namespace StudyGroups.Core.Interfaces;

public interface ICategoriesService
{
    Task<Result<IEnumerable<Topic>>> GetAllAsync();
    Task<Result<Topic>> CreateAsync(Topic category);
    Task<Result> UpdateAsync(Topic category);
    Task<Result> DeleteAsync(int id);
}
