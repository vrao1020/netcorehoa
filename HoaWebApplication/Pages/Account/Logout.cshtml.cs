using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HoaWebApplication.Pages.Account
{
    public class LogoutModel : PageModel
    {

        public IActionResult OnGet()
        {
            return RedirectToPage("/index");
        }

        public async Task OnPost()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync("oidc");
            //await HttpContext.SignOutAsync("oidc", new AuthenticationProperties
            //{
            //    // Indicate here where Auth0 should redirect the user after a logout.
            //    // Note that the resulting absolute Uri must be whitelisted in the 
            //    // **Allowed Logout URLs** settings for the client.
            //    RedirectUri = Url.Page("/")
            //});
        }
    }
}