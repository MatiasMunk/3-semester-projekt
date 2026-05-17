using System.ComponentModel.DataAnnotations;

namespace StudyGroups.Contracts;

public class CreateCategoryRequest
{
    [Required]
    public string Name { get; set; } = "";

    public string Icon { get; set; } = "📚";
    public string Color { get; set; } = "#4f46e5";
}
