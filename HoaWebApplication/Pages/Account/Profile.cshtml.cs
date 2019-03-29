using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HoaWebApplication.ApiHttpClient;
using HoaWebApplication.Extensions.Streams;
using HoaEntities.Models.OutputModels;
using HoaEntities.Models.UpdateModels;
using HoaWebApplication.Services.AuthenticatedUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.Extensions;

namespace HoaWebApplication.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private ApiClient _apiClient;
        private IUserService _userService;
        private ILogger<ProfileModel> _logger;

        public int? MyProperty { get; set; }
        public UserViewDto UserDetails { get; set; }
        public bool SocialLogin { get; set; } = false;
        public bool ValidEmail { get; set; } = true;

        [BindProperty]
        public UserUpdateDto UserToUpdate { get; set; }

        public ProfileModel(ApiClient apiClient, IUserService userService, ILogger<ProfileModel> logger)
        {
            _apiClient = apiClient;
            _userService = userService;
            _logger = logger;
        }

        public async Task OnGet(CancellationToken cancellationToken)
        {
            //verify email exists on the claims princicle
            //if it does not exist, allow user to update it for reminders
            if (!_userService.ValidEmail())
            {
                ValidEmail = false;
            }

            //if user logged in through google/fb, set social login to true
            //this is to ensure that the user can only change the reminder email option of their profile
            if (_userService.IsSocialLogin())
            {
                SocialLogin = true;
            }

            try
            {
                var baseAPIUrl = $"api/users/currentuser";

                //call the api with a GET request
                //ensure to set the httpcompletion mode to response headers read
                //this allows the response to be read as soon as content starts arriving instead of having to wait
                //until the entire response is read
                //with this option, we can read the response content into a stream and deserialize it
                var response = await (await _apiClient.WithAuthorization()).GetAsync(baseAPIUrl,
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                //ensure success status code else throw an exception
                response.EnsureSuccessStatusCode();

                //read the response content into a stream
                var streamContent = await response.Content.ReadAsStreamAsync();
                //deserialize the stream into an object (see StreamExtensions on how this is done)
                UserDetails = streamContent.ReadAndDeserializeFromJson<UserViewDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occured accessing the API. Url: {HttpContext.Request.GetDisplayUrl()}" +
                                 $" Error Message: {ex.Message}");

                //either the API is not running or an error occured on the server
                throw new Exception("Website is unavailable at this time");
            }
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var apiUrl = "api/users";

            string email = null;

            //verify if email is valid
            if (!_userService.ValidEmail())
            {
                email = UserToUpdate.Email;
            }

            try
            {
                var response = await (await _apiClient.WithAuthorization()).PatchUserReminderEmailJsonAsync(apiUrl,
                    UserToUpdate.Reminder.Value, email, cancellationToken);

                //return the same page with an error message if the user is trying to call the API too many times
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    TempData["TooManyRequests"] = "Too many requests. Please slow down with your requests";
                    return Page();
                }

                //ensure success status code else throw an exception
                response.EnsureSuccessStatusCode();

                TempData["UserProfileUpdate"] = "Your changes were saved successfully";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occured accessing the API. Url: {HttpContext.Request.GetDisplayUrl()}" +
                             $" Error Message: {ex.Message}");

                //either the API is not running or an error occured on the server
                throw new Exception("Website is unavailable at this time");
            }

            return RedirectToPage("/Account/Profile");


        }
    }
}