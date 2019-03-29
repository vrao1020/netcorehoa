using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HoaEntities.Models.OutputModels;
using HoaEntities.Models.UpdateModels;
using HoaWebApplication.ApiHttpClient;
using HoaWebApplication.Extensions.Streams;
using HoaWebApplication.Models.PaginationModels;
using HoaWebApplication.Services.Cache;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HoaWebApplication.Services.IdentityServer
{
    public class IDPUserService : IIDPUserService
    {
        private readonly IdentityServerApiClient identityServerApiClient;
        private readonly IConfiguration configuration;
        private readonly ICacheStore cacheStore;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<IDPUserService> logger;
        private readonly IHttpClientFactory httpClientFactory;

        public IDPUserService(IdentityServerApiClient identityServerApiClient,
            IConfiguration configuration, ICacheStore cacheStore, IHttpContextAccessor httpContextAccessor,
            ILogger<IDPUserService> logger, IHttpClientFactory httpClientFactory)
        {
            this.identityServerApiClient = identityServerApiClient;
            this.configuration = configuration;
            this.cacheStore = cacheStore;
            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetUserManagementAccessTokenAsync()
        {
            var (date, access_token) = await cacheStore.GetExpirationAccessTokenAsync("usrmgapi");

            //if access token is null or date when last access token was received is longer than 23 hours
            if (access_token == null || ((DateTime.Now - date).TotalHours > 23))
            {
                try
                {
                    //fetch the response by requesting an access token from the auth0 endpoint
                    var client = httpClientFactory.CreateClient();

                    var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                    {
                        Address = "https://localhost:44367/connect/token",

                        ClientId = "idpclient",
                        ClientSecret = "secret",
                        Scope = "usermgapi"
                    });

                    //ensure success status code and throw an exception if not
                    if (response.IsError)
                    {
                        throw new HttpRequestException("An exception occured when trying to get an access token");
                    }

                    //set the access token
                    access_token = response.AccessToken;

                    //set the cache with the item
                    await cacheStore.SetCacheValueAsync("usrmgapi", DateTime.Now.AddHours(23), access_token);
                }
                catch (HttpRequestException ex)
                {
                    logger.LogError($"An error occured accessing the User Management API." +
                                 $" Error Message: {ex.Message}");

                    //either the API is not running or an error occured on the server
                    throw new Exception("Website is unavailable at this time");
                }
            }

            return access_token;
        }

        public async Task<string> GetAdminEmail()
        {
            var adminEmail = "";

            try
            {
                var accesstoken = await GetUserManagementAccessTokenAsync();

                var response = await (identityServerApiClient.WithAuthorizationAsync(accesstoken))
                                .GetAsync($"api/UserClaims/getadminemail");

                //ensure success status code else throw an exception
                response.EnsureSuccessStatusCode();

                //read the response content into a stream
                var streamContent = await response.Content.ReadAsStreamAsync();
                //deserialize the stream into an object (see StreamExtensions on how this is done)
                adminEmail = streamContent.ReadAndDeserializeFromJson<string>();
            }
            catch (HttpRequestException ex)
            {
                logger.LogError($"An error occured accessing the API." +
                                 $" Error Message: {ex.Message}");

                //either the API is not running or an error occured on the server
                throw new Exception("Website is unavailable at this time");
            }

            return adminEmail;
        }

        public async Task<(IEnumerable<IDPUserViewDto>, XPaginationHeaderDto)> GetAllUsersAsync(int pageNum, int pageSize)
        {
            //initialize list of users
            List<IDPUserViewDto> userList;
            XPaginationHeaderDto headerToReturn;

            try
            {
                var response = await (identityServerApiClient.WithAuthorizationAsync(await GetUserManagementAccessTokenAsync()))
                                .GetAsync($"api/UserClaims/getusers?pageNum={pageNum}&pageSize={pageSize}");

                //ensure success status code else throw an exception
                response.EnsureSuccessStatusCode();

                //deserialize header into pagination dto
                var header = response.Headers.GetValues("X-Pagination").FirstOrDefault();
                headerToReturn = JsonConvert.DeserializeObject<XPaginationHeaderDto>(header);

                //read the response content into a stream
                var streamContent = await response.Content.ReadAsStreamAsync();
                //deserialize the stream into an object (see StreamExtensions on how this is done)
                userList = streamContent.ReadAndDeserializeFromJson<List<IDPUserViewDto>>();
            }
            catch (HttpRequestException ex)
            {
                logger.LogError($"An error occured accessing the API." +
                                 $" Error Message: {ex.Message}");

                //either the API is not running or an error occured on the server
                throw new Exception("Website is unavailable at this time");
            }

            return (userList, headerToReturn);
        }

        public async Task<IEnumerable<string>> GetBoardMemberEmails()
        {
            //initialize list of emails
            List<string> emailList;

            try
            {
                var accesstoken = await GetUserManagementAccessTokenAsync();

                var response = await (identityServerApiClient.WithAuthorizationAsync(accesstoken))
                                .GetAsync($"api/UserClaims/getboardemails");

                //ensure success status code else throw an exception
                response.EnsureSuccessStatusCode();

                //read the response content into a stream
                var streamContent = await response.Content.ReadAsStreamAsync();
                //deserialize the stream into an object (see StreamExtensions on how this is done)
                emailList = streamContent.ReadAndDeserializeFromJson<List<string>>();
            }
            catch (HttpRequestException ex)
            {
                logger.LogError($"An error occured accessing the API." +
                                 $" Error Message: {ex.Message}");

                //either the API is not running or an error occured on the server
                throw new Exception("Website is unavailable at this time");
            }

            return emailList;
        }

        public async Task<IDPUserViewDto> GetUserAsync(string userId)
        {
            //initialize user
            IDPUserViewDto userToReturn;

            try
            {
                var accesstoken = await GetUserManagementAccessTokenAsync();

                var response = await (identityServerApiClient.WithAuthorizationAsync(accesstoken))
                                .GetAsync($"api/UserClaims/getuser{userId}");

                //ensure success status code else throw an exception
                response.EnsureSuccessStatusCode();

                //read the response content into a stream
                var streamContent = await response.Content.ReadAsStreamAsync();
                //deserialize the stream into an object (see StreamExtensions on how this is done)
                userToReturn = streamContent.ReadAndDeserializeFromJson<IDPUserViewDto>();
            }
            catch (HttpRequestException ex)
            {
                logger.LogError($"An error occured accessing the API." +
                                 $" Error Message: {ex.Message}");

                //either the API is not running or an error occured on the server
                throw new Exception("Website is unavailable at this time");
            }

            return userToReturn;
        }

        public async Task UpdateUser(bool ReadOnlyAccess, string Role, bool PostCreation, string id)
        {
            //set the values appropriately
            var readAccess = ReadOnlyAccess ? "true" : "false";
            var postAccess = PostCreation ? "true" : "false";

            //initialize the user to update
            var updateData = new IDPUserUpdateDto(id, Role, readAccess, postAccess);

            try
            {
                //send the updated user data
                var response = await (identityServerApiClient.WithAuthorizationAsync(await GetUserManagementAccessTokenAsync()))
                                .PutAsJsonAsync<IDPUserUpdateDto>("api/UserClaims/UpdateUser", updateData);

                //ensure success status code else throw an exception
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                logger.LogError($"An error occured accessing the API." +
                                 $" Error Message: {ex.Message}");

                //either the API is not running or an error occured on the server
                throw new Exception("Website is unavailable at this time");
            }
        }
    }
}
