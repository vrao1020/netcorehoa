using HoaWebApplication.Services.AccessToken;
using HoaWebApplication.Services.Cache;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace HoaWebApplication.ApiHttpClient
{
    public class ApiClient
    {
        public HttpClient Client { get; private set; }

        private ICacheStore _cacheStore;
        private IGetAPIAccessToken _getAPIAccessToken;
        private IHostingEnvironment _hostingEnvironment;
        private IHttpClientFactory _httpClientFactory;
        private IHttpContextAccessor _httpContextAccessor;

        public ApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ICacheStore cacheStore,
            IGetAPIAccessToken getAPIAccessToken, IHostingEnvironment hostingEnvironment, IHttpClientFactory httpClientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _cacheStore = cacheStore;
            _getAPIAccessToken = getAPIAccessToken;
            _hostingEnvironment = hostingEnvironment;
            _httpClientFactory = httpClientFactory;

            httpClient.BaseAddress = _hostingEnvironment.IsDevelopment() ? new Uri("https://localhost:44358/")
                : new Uri("TBD");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            httpClient.Timeout = new TimeSpan(0, 0, 10);
            Client = httpClient;
        }

        public async Task<HttpClient> WithAuthorization()
        {
            //get HttpContext via HttpClientFactory
            var context = _httpContextAccessor.HttpContext;

            //initialize empty string
            var token = "";

            //should we renew access & refresh tokens?
            // get expires_at value
            var expires_at = await context.GetTokenAsync("expires_at");

            // compare - make sure to use the exact date formats for comparison 
            // (UTC, in this case)
            if (string.IsNullOrWhiteSpace(expires_at)
                || ((DateTime.Parse(expires_at).AddHours(-1)).ToUniversalTime() < DateTime.UtcNow))
            {
                token = await RenewTokens();
            }
            else
            {
                // get access token
                token = await context.GetTokenAsync("access_token");
            }

            if (token != "")
            {
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return Client;
        }

        public HttpClient WithoutAuthorization()
        {
            Client.DefaultRequestHeaders.Remove("Authorization");
            return Client;
        }

        public void SetIfNoneMatchHeader(string etag)
        {
            Client.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag));
        }

        public HttpClient SetAuthorization(string acess_token)
        {
            if (acess_token != null)
            {
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", acess_token);
            }

            return Client;
        }

        /// <summary>
        /// This method is only used for GET requests to the API
        /// GET does not require any roles other than user being authorized
        /// </summary>
        /// <returns></returns>
        public async Task<HttpClient> WithGETOnlyAccessAuthorization()
        {
            //get the cached access token and expiration date if they exist
            var (date, access_token) = await _cacheStore.GetExpirationAccessTokenAsync("hoawebapi");

            //if access token is null or date when last access token was received is longer than 9 hours
            if (access_token == null || ((DateTime.Now - date).TotalHours > 23))
            {
                var new_access_token_ = await _getAPIAccessToken.GetAccessToken();

                //set the access token
                access_token = new_access_token_;

                //set the cache with the item
                await _cacheStore.SetCacheValueAsync("hoawebapi", DateTime.Now.AddHours(23), new_access_token_);
            }

            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);


            return Client;
        }

        private async Task<string> RenewTokens()
        {
            // get the current HttpContext to access the tokens
            var currentContext = _httpContextAccessor.HttpContext;

            var refreshToken = currentContext.GetTokenAsync("refresh_token")?.Result;

            // get new refresh token
            var response = await _httpClientFactory.CreateClient()
                .RequestRefreshTokenAsync(new RefreshTokenRequest
                {
                    Address = "https://localhost:44367/connect/token",

                    ClientId = "hoawebapp",
                    ClientSecret = "secret",
                    RefreshToken = refreshToken
                }
                );

            if (!response.IsError)
            {
                // update the tokens & exipration value                
                var old_id_token = currentContext.GetTokenAsync("id_token")?.Result;
                var new_access_token = response.AccessToken;
                var new_refresh_token = response.RefreshToken;
                var expiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(response.ExpiresIn);

                var info = await currentContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                info.Properties.UpdateTokenValue("refresh_token", new_refresh_token);
                info.Properties.UpdateTokenValue("access_token", new_access_token);
                info.Properties.UpdateTokenValue("expires_at", expiresAt.ToString("o", CultureInfo.InvariantCulture));

                await currentContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, info.Principal, info.Properties);

                // return the new access token 
                return response.AccessToken;
            }
            else
            {
                throw new Exception("Problem encountered while refreshing tokens.",
                    response.Exception);
            }
        }

    }
}
