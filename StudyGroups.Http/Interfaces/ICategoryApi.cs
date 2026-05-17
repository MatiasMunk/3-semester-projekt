using StudyGroups.Contracts;

namespace StudyGroups.Http.Interfaces;

public interface ICategoryApi
{
    Task<IEnumerable<CategoryDto>> GetAll();
    Task<CategoryDto?> Create(CreateCategoryRequest request);
    Task Update(int id, UpdateCategoryRequest request);
    Task Delete(int id);
}
