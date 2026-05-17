using Microsoft.AspNetCore.Mvc;
using StudyGroups.Contracts;
using StudyGroups.Core.Interfaces;

namespace StudyGroups.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly ILogger<AuthController> _logger = logger;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        _logger.LogWarning("API REGISTER received Username='{Username}', PasswordLength={PasswordLength}",
            request.Username,
            request.Password?.Length ?? 0);

        var result = await _authService.RegisterAsync(request.Username.Trim(), request.Password);

        if (result.IsFailure)
        {
            _logger.LogWarning("API REGISTER failed for Username='{Username}': {Error}", request.Username, result.Error);
            return BadRequest(result.Error);
        }

        _logger.LogWarning("API REGISTER succeeded. UserId={UserId}, Username='{Username}'", result.Value.Id, result.Value.Username);

        return Ok(new AuthResponse
        {
            UserId = result.Value.Id,
            Username = result.Value.Username
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Username.Trim(), request.Password);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new AuthResponse
        {
            UserId = result.Value.Id,
            Username = result.Value.Username
        });
    }
}