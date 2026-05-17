using System.ComponentModel.DataAnnotations;

namespace StudyGroups.Contracts;

public class RegisterRequest
{
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = "";
}
