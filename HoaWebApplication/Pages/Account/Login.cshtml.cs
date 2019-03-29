using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HoaWebApplication.Pages.Account
{
    public class LoginModel : PageModel
    {
        public async Task OnGetAsync()
        {
            await HttpContext.ChallengeAsync("oidc", new AuthenticationProperties() { RedirectUri = "/" });
            TempData["UserCreatedUpdated"] = false;
        }
    }
}