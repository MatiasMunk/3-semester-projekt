using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using StudyGroups.Web.Extensions;
using StudyGroups.Web.Models;
using System.Diagnostics;

namespace StudyGroups.Web.Controllers;

public class HomeController : Controller
{
    // -----------------------------
    // LANDING PAGE
    // -----------------------------
    public IActionResult Index(string theaction = "none")
    {
        var userId = HttpContext.GetUserId();

        ViewBag.Name = userId == null
            ? "Guest"
            : HttpContext.GetDisplayName();

        ViewBag.Email = null;
        ViewBag.Action = theaction;

        return View();
    }

    // -----------------------------
    // PRIVACY
    // -----------------------------
    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult SetLanguage(string culture, string returnUrl = "/")
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1)
            }
        );

        return LocalRedirect(returnUrl);
    }

    // -----------------------------
    // ERROR
    // -----------------------------
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
