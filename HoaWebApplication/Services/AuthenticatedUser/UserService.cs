using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using HoaWebApplication.ApiHttpClient;
using HoaEntities.Models.InputModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;

namespace HoaWebApplication.Services.AuthenticatedUser
{
    public class UserService : IUserService
    {
        private IHttpContextAccessor _httpContextAccessor;
        private ApiClient _apiClient;
        private ILogger<UserService> _logger;

        public UserService(IHttpContextAccessor httpContextAccessor, ApiClient apiClient,
            ILogger<UserService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<(bool userCreated, string UserName, string UserEmail)> CreateUpdateUserAsync(CancellationToken cancellationToken)
        {
            if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                //need to initialize TempData as its not available by default in a non-controller
                var factory = _httpContextAccessor.HttpContext.RequestServices.GetService(typeof(ITempDataDictionaryFactory)) as ITempDataDictionaryFactory;
                var tempData = factory.GetTempData(_httpContextAccessor.HttpContext);

                if (tempData.Peek("UserCreatedUpdated") as bool? == false)
                {
                    tempData["UserCreatedUpdated"] = true;

                    if (await UserExistsAsync(cancellationToken))
                    {
                        //user already exists. update the user but only if they used a social login
                        if (IsSocialLogin())
                        {
                            var baseAPIUrl = $"api/users";

                            try
                            {
                                //get users name
                                var name = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

                                //send the updated user name
                                var response = await (await _apiClient.WithAuthorization()).PatchUserNameJsonAsync(baseAPIUrl,
                                    name.Substring(0, name.IndexOf(" ")),
                                    name.Substring(name.IndexOf(" ") + 1),
                                    cancellationToken);

                                //ensure success status code else throw an exception
                                response.EnsureSuccessStatusCode();
                            }
                            catch (HttpRequestException ex)
                            {
                                _logger.LogError($"An error occured accessing the API." +
                                 $" Error Message: {ex.Message}");

                                throw new Exception("Website is unavailable at this time. Please try again later.");
                            }
                        }

                        //user was not created so return false
                        return (false, null, null);
                    }
                    else
                    {
                        //user does not exist, create the user
                        var (userName, userEmail) = await CreateUserAsync(cancellationToken);

                        //user was created so return true
                        return (true, userName, userEmail);
                    }
                }
            }

            //user was not logged in so return false
            return (false, null, null);
        }

        public async Task<(string userName, string userEmail)> CreateUserAsync(CancellationToken cancellationToken)
        {
            var baseAPIUrl = $"api/users";

            try
            {
                var userToCreate = GetUserForCreation();

                //if user does not exist - create the user
                var response = await (await _apiClient.WithAuthorization()).PostAsJsonAsync<UserInputDto>(baseAPIUrl, userToCreate, cancellationToken);
                //ensure success status code else throw an exception
                response.EnsureSuccessStatusCode();

                return ($"{userToCreate.FirstName} {userToCreate.LastName}", $"{userToCreate.Email}");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occured accessing the API." +
                                 $" Error Message: {ex.Message}");

                throw new Exception("Website is unavailable at this time. Please try again later.");
            }
        }

        public UserInputDto GetUserForCreation()
        {
            var name = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            UserInputDto userToCreate = new UserInputDto
            {
                Email = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? $"updateyouremail@notexists.com",
                FirstName = name.Substring(0, name.IndexOf(" ")),
                LastName = name.Substring(name.IndexOf(" ") + 1)
            };

            return userToCreate;
        }

        public bool IsSocialLogin()
        {
            //find the name identifier
            var nameIdentifier = _httpContextAccessor.HttpContext.User
                .Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/identityprovider")?.Value;

            // when user logs in via Google/FB
            if (nameIdentifier == "Google" || nameIdentifier == "Facebook")
            {
                return true;
            }

            return false;
        }

        public async Task<bool> UserExistsAsync(CancellationToken cancellationToken)
        {
            var baseAPIUrl = $"api/users/currentuser";

            try
            {
                var response = await (await _apiClient.WithAuthorization()).GetAsync(baseAPIUrl, cancellationToken);

                return !response.IsSuccessStatusCode ? false : true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occured accessing the API." +
                                 $" Error Message: {ex.Message}");

                throw new Exception("Website is unavailable at this time. Please try again later.");
            }
        }

        public bool ValidEmail()
        {
            if (_httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value == null)
            {
                return false;
            }

            return true;
        }
    }
}

