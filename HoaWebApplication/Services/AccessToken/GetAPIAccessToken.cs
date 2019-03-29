using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HoaWebApplication.Services.AccessToken
{
    public class GetAPIAccessToken : IGetAPIAccessToken
    {
        private IConfiguration _configuration;
        private IHttpClientFactory _httpClientFactory;
        private ILogger<GetAPIAccessToken> _logger;

        public GetAPIAccessToken(IConfiguration configuration, IHttpClientFactory httpClientFactory,
            ILogger<GetAPIAccessToken> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<string> GetAccessToken()
        {
            var access_token = "";

            //fetch the response by requesting an access token from the token endpoint
            try
            {
                //fetch the response by requesting an access token from the auth0 endpoint
                var client = _httpClientFactory.CreateClient();

                var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                {
                    Address = "https://localhost:44367/connect/token",

                    ClientId = "apiclient",
                    ClientSecret = "secret",
                    Scope = "hoawebapi"
                });

                //ensure success status code and throw an exception if not
                if (response.IsError)
                {
                    throw new HttpRequestException("An exception occured when trying to get an access token");
                }

                //set the access token
                access_token = response.AccessToken;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occured accessing the User Management API." +
                             $" Error Message: {ex.Message}");

                //either the API is not running or an error occured on the server
                throw new Exception("Website is unavailable at this time");
            }

            return access_token;
        }
    }
}
