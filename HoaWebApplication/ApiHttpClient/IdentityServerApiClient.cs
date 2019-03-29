using HoaWebApplication.Services.Cache;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace HoaWebApplication.ApiHttpClient
{
    public class IdentityServerApiClient
    {
        public HttpClient Client { get; private set; }

        private ICacheStore _cacheStore;
        private IHostingEnvironment _hostingEnvironment;
        private IHttpContextAccessor _httpContextAccessor;

        public IdentityServerApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ICacheStore cacheStore,
            IHostingEnvironment hostingEnvironment)
        {
            _httpContextAccessor = httpContextAccessor;
            _cacheStore = cacheStore;
            _hostingEnvironment = hostingEnvironment;

            httpClient.BaseAddress = _hostingEnvironment.IsDevelopment() ? new Uri("https://localhost:44386/")
                : new Uri("TBD");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            httpClient.Timeout = new TimeSpan(0, 0, 10);
            Client = httpClient;
        }

        /// <summary>
        /// Sets the access token
        /// </summary>
        /// <returns></returns>
        public HttpClient WithAuthorizationAsync(string access_token)
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);

            return Client;
        }
    }
}
