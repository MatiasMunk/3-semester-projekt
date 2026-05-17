using StudyGroups.Contracts;
using StudyGroups.Http.Interfaces;

namespace StudyGroups.Http.Services;

public class CategoryApi(ApiClient apiClient) : ICategoryApi
{
    private readonly ApiClient _apiClient = apiClient;

    public async Task<IEnumerable<CategoryDto>> GetAll()
    {
        return await _apiClient.GetAsync<IEnumerable<CategoryDto>>("api/categories")
            ?? Enumerable.Empty<CategoryDto>();
    }

    public async Task<CategoryDto?> Create(CreateCategoryRequest request)
    {
        return await _apiClient.PostAsync<CreateCategoryRequest, CategoryDto>("api/categories", request);
    }

    public async Task Update(int id, UpdateCategoryRequest request)
    {
        await _apiClient.PutAsync($"api/categories/{id}", request);
    }

    public async Task Delete(int id)
    {
        await _apiClient.DeleteAsync($"api/categories/{id}");
    }
}
