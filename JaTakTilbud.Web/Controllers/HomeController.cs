using JaTakTilbud.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;

namespace JaTakTilbud.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index(string theaction = "none")
        {
            // If an access_token was provided, then this part shows how to get at it
            string? accessToken = HttpContext.GetTokenAsync("access_token").Result;
            if (accessToken != null)
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(accessToken);
                string? name = jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == "name")?.Value;
                ViewBag.name = name;
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
