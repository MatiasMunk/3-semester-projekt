using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace JaTakTilbud.Web.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            return Challenge(OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public ActionResult Logout(int id)
        {
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}
