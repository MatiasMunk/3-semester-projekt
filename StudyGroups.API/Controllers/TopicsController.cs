using Microsoft.AspNetCore.Mvc;
using StudyGroups.Contracts;
using StudyGroups.Core.Interfaces;
using StudyGroups.Core.Models;

namespace StudyGroups.API.Controllers;

[ApiController]
[Route("api/categories")]
public class TopicsController(ICategoriesService categoryService) : ControllerBase
{
    private readonly ICategoriesService _categoryService = categoryService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _categoryService.GetAllAsync();

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value.Select(ToDto));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryRequest request)
    {
        var result = await _categoryService.CreateAsync(new Topic
        {
            Name = request.Name,
            Icon = request.Icon,
            Color = request.Color
        });

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(ToDto(result.Value));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCategoryRequest request)
    {
        var result = await _categoryService.UpdateAsync(new Topic
        {
            Id = id,
            Name = request.Name,
            Icon = request.Icon,
            Color = request.Color
        });

        if (result.IsFailure)
        {
            return result.Error == "Category not found"
                ? NotFound(result.Error)
                : BadRequest(result.Error);
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoryService.DeleteAsync(id);

        if (result.IsFailure)
        {
            return result.Error == "Category not found"
                ? NotFound(result.Error)
                : Conflict(result.Error);
        }

        return NoContent();
    }

    private static CategoryDto ToDto(Topic topic)
    {
        return new CategoryDto
        {
            Id = topic.Id,
            Name = topic.Name,
            Slug = topic.Slug,
            Icon = topic.Icon,
            Color = topic.Color
        };
    }
}
