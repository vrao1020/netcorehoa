using System.Threading.Tasks;
using HoaWebApplication.Services.Email;
using HoaWebApplication.Services.AuthenticatedUser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using HoaWebApplication.Services.Azure;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.Extensions;

namespace HoaWebApplication.Pages
{
    public class IndexModel : PageModel
    {
        private ISendEmail _sendEmail;
        private IUserService _userService;
        private IAzureSASTokenUrl _azureSASTokenUrl;
        private ILogger<IndexModel> _logger;
        private const string AzureBlobFolder = "hoafiles"; //azure blob storage folder where files will be stored/fetched    

        //used for GET action using the Download handler to download the file
        [BindProperty(SupportsGet = true)]
        public string FileName { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool UserRegistered { get; set; } = false;

        [BindProperty(SupportsGet = true)]
        public bool PasswordReset { get; set; } = false;

        [BindProperty(SupportsGet = true)]
        public bool ForgotPassword { get; set; } = false;

        public bool UserCreated { get; set; } = false;

        public IndexModel(ISendEmail sendEmail, IUserService userService,
            IAzureSASTokenUrl azureSASTokenUrl, ILogger<IndexModel> logger)
        {
            _sendEmail = sendEmail;
            _userService = userService;
            _azureSASTokenUrl = azureSASTokenUrl;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            //service will create or update user information as needed
            var (userCreated, userName, userEmail) = await _userService.CreateUpdateUserAsync(cancellationToken);

            if (userCreated)
            {
                UserCreated = true;
                await _sendEmail.SendUserCreatedEmailAsync(userName, userEmail);
            }

            return Page();
        }

        //this action is used for downloading files
        public IActionResult OnGetDownload()
        {
            try
            {
                //get the url for the file from Azure 
                var urlToDownload = _azureSASTokenUrl.GetDownloadUrlWithSAS(FileName, AzureBlobFolder);
                return Redirect(urlToDownload);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occured accessing the API. Url: {HttpContext.Request.GetDisplayUrl()}" +
                                 $" Error Message: {ex.Message}");

                //either the API is not running or an error occured on the server
                TempData["Error"] = "An error occured while processing your request. Please try again later.";
                return Page();
            }
        }
    }
}