using Microsoft.AspNetCore.Mvc;
using StudyGroups.Contracts;
using StudyGroups.Core.Interfaces;
using StudyGroups.Core.Models;

namespace StudyGroups.API.Controllers;

[ApiController]
[Route("api/users")]
public class AccountsControllers(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _userService.GetAllAsync();

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value.Select(ToDto));
    }

    private static UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };
    }
}
