using Microsoft.AspNetCore.Mvc;
using StudyGroups.Contracts;
using StudyGroups.Http.Interfaces;

namespace StudyGroups.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthApi _authApi;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAuthApi authApi, ILogger<AccountController> logger)
    {
        _authApi = authApi;
        _logger = logger;
    }

    // -----------------------------
    // LOGIN VIEW
    // -----------------------------
    public IActionResult Login()
    {
        return View();
    }

    // -----------------------------
    // LOGIN POST
    // -----------------------------
    [HttpPost]
    public async Task<IActionResult> Login([FromForm] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            if (IsAjaxRequest())
                return BadRequest(ModelStateToText());

            return View(request);
        }

        try
        {
            var result = await _authApi.Login(request);

            if (result == null)
            {
                ModelState.AddModelError("", "Wrong username or password");

                if (IsAjaxRequest())
                    return BadRequest("Wrong username or password");

                return View(request);
            }

            HttpContext.Session.SetInt32("UserId", result.UserId);
            HttpContext.Session.SetString("Username", result.Username);

            if (IsAjaxRequest())
                return Ok(new { ok = true, message = "Login succeeded", username = result.Username });

            return RedirectToAction("Index", "Sessions");
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Invalid credentials"))
            {
                ModelState.AddModelError("", "Wrong username or password");

                if (IsAjaxRequest())
                    return BadRequest("Wrong username or password");

                return View(request);
            }

            if (IsAjaxRequest())
                return BadRequest(ex.Message);

            throw;
        }
    }

    // -----------------------------
    // REGISTER VIEW
    // -----------------------------
    public IActionResult Register()
    {
        return View();
    }

    // -----------------------------
    // REGISTER POST
    // -----------------------------
    [HttpPost]
    public async Task<IActionResult> Register([FromForm] RegisterRequest request)
    {
        request.Username = request.Username?.Trim() ?? "";

        _logger.LogWarning("WEB REGISTER POST received. HasForm={HasForm}, Username='{Username}', PasswordLength={PasswordLength}",
            Request.HasFormContentType,
            request.Username,
            request.Password?.Length ?? 0);

        ViewBag.Debug = $"WEB received Username='{request.Username}', PasswordLength={request.Password?.Length ?? 0}";

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            ModelState.AddModelError(nameof(request.Username), "Username is required");
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelStateToText();
            _logger.LogWarning("WEB REGISTER validation failed: {Errors}", errors);
            ViewBag.Debug += $"\nWEB validation failed: {errors}";

            if (IsAjaxRequest())
                return BadRequest(ViewBag.Debug);

            return View(request);
        }

        try
        {
            var result = await _authApi.Register(request);
            _logger.LogWarning("WEB REGISTER API success. UserId={UserId}, Username='{Username}'", result?.UserId, result?.Username);

            if (result != null)
            {
                HttpContext.Session.SetInt32("UserId", result.UserId);
                HttpContext.Session.SetString("Username", result.Username);
            }

            ViewBag.RegisterSucceeded = true;
            ViewBag.Debug += $"\nAPI success: UserId={result?.UserId}, Username='{result?.Username}'";

            if (IsAjaxRequest())
                return Ok(new { ok = true, message = "Register succeeded", debug = ViewBag.Debug, userId = result?.UserId, username = result?.Username });

            return View(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WEB REGISTER API call failed for Username='{Username}'", request.Username);
            ModelState.AddModelError("", ex.Message);
            ViewBag.Debug += $"\nAPI error: {ex.Message}";

            if (IsAjaxRequest())
                return BadRequest(ViewBag.Debug);

            return View(request);
        }
    }

    private bool IsAjaxRequest()
    {
        return Request.Headers.XRequestedWith == "XMLHttpRequest";
    }

    private string ModelStateToText()
    {
        return string.Join(" | ", ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? e.Exception?.Message : e.ErrorMessage)
            .Where(e => !string.IsNullOrWhiteSpace(e)));
    }

    // -----------------------------
    // LOGOUT
    // -----------------------------
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    // -----------------------------
    // USED BY MODAL (VERY IMPORTANT)
    // -----------------------------
    [HttpGet]
    public IActionResult IsLoggedIn()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        return Json(userId != null);
    }
}