using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using HoaIdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Linq;
using IdentityModel;
using System.Text;

namespace HoaIdentityServer.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(25)]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [StringLength(30)]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email };

                //custom claims for user for hoaauthorization
                var filtered = new List<Claim>();
                filtered.Add(new Claim("role", "user"));
                filtered.Add(new Claim("readonly", "true"));
                filtered.Add(new Claim("postcreation", "false"));
                filtered.Add(new Claim(JwtClaimTypes.Email, Input.Email));
                filtered.Add(new Claim(JwtClaimTypes.Name, Input.FirstName + " " + Input.LastName));

                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    var claimsResult = await _userManager.AddClaimsAsync(user, filtered);
                    if (!claimsResult.Succeeded) throw new Exception(claimsResult.Errors.First().Description);

                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { userId = user.Id, code = code },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        ConstructEmailTemplate($"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>."));

                    //do not sign the user in automatically as they need to confirm their email first
                    //await _signInManager.SignInAsync(user, isPersistent: false);

                    //redirect back to the web application instead
                    return Redirect("https://localhost:44360?userRegistered=true");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private string ConstructEmailTemplate(string emailMessage)
        {
            StringBuilder stringBuilder = new StringBuilder("<div class=\"\"><div class=\"aHl\"></div><div id=\":ub\" tabindex=\"-1\"></div><div id=\":um\" class=\"ii gt\"><div id=\":un\" class=\"a3s aXjCH msg8485945387776152132\"><u></u><div id=\"m_8485945387776152132YIELD_MJML\" style=\"background:#222228\"><div><div class=\"m_8485945387776152132mktEditable\"><div class=\"m_8485945387776152132mj-body\" style=\"background-color:#222228\"><div style=\"margin:0 auto;max-width:600px;background:#222228\"><table cellpadding=\"0\" cellspacing=\"0\" style=\"width:100%;font-size:0px;background:#222228\" align=\"center\"><tbody><tr><td style=\"text-align:center;vertical-align:top;font-size:0;padding:20px 0 0 0\"></td></tr><tr><td style=\"font-size:0;padding:10px 25px;padding-bottom:20px\"align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse:collapse;border-spacing:0px\" align=\"center\"><tbody><tr><td><a href=\"\" target=\"_blank\" data-saferedirecturl=\"\"><img alt=\"auth0\" src=\"\" style=\"border:none;display:block;outline:none;text-decoration:none;width:100%;max-width:150px;height:auto\" width=\"150\" height=\"auto\" class=\"CToWUd\"></a></td></tr></tbody></table></td></tr></tbody></table></div><div style=\"margin:0 auto;max-width:600px;background:white\"><table cellpadding=\"0\" cellspacing=\"0\" style=\"width:100%;font-size:0px;background:white\" align=\"center\"><tbody><tr><td style=\"text-align:center;vertical-align:top;font-size:0;padding:30px 40px 0\"><div style=\"vertical-align:top;display:inline-block;font-size:13px;text-align:left;width:100%;min-width:100%\" class=\"m_8485945387776152132mj-column-per-100\" aria-labelledby=\"mj-column-per-100\"><table width=\"100%\"><tbody><tr><td style=\"font-size:0;padding:0 0 20px 0;max-width:480px;overflow:auto\"align=\"left\"><div class=\"m_8485945387776152132mj-content\" style=\"color:#555;font-family:avenir,roboto,sans-serif;font-size:14px;line-height:1.8\"><h2 id=\"m_8485945387776152132overview\">Summary</h2><p>");
            stringBuilder.Append(emailMessage);
            stringBuilder.Append("</p></div></td></tr></tbody></table></div></td></tr></tbody></table></div><div style=\"margin:0 auto;max-width:600px;background:#222228\"><table cellpadding=\"0\" cellspacing=\"0\" style=\"width:100%;font-size:0px;background:#222228\" align=\"center\"><tbody><tr><td style=\"text-align:center;vertical-align:top;font-size:0;padding:0px\"></td></tr><tr><td style=\"font-size:0;padding:20px 30px 10px 30px\" align=\"left\"><div class=\"m_8485945387776152132mj-content\" style=\"color:lightgray;font-family:avenir,roboto,sans-serif;font-size:12px;line-height:2\">If you have any issues, please visit the website and contact the administrator.</div></td></tr></tbody></table></div></div></div></div></div><div class=\"yj6qo\"></div><div class=\"adL\"></div></div></div><div id=\":u7\" class=\"ii gt\" style=\"display:none\"><div id=\":u6\" class=\"a3s aXjCH undefined\"></div></div><div class=\"hi\"></div></div>");

            var emailTemplate = stringBuilder.ToString();

            return emailTemplate;
        }
    }
}
