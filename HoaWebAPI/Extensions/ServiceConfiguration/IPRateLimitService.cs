using AspNetCoreRateLimit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HoaWebAPI.Extensions.ServiceConfiguration
{
    public static class IPRateLimitService
    {
        public static IServiceCollection AddIPRateLimit(this IServiceCollection services, IConfiguration Configuration)
        {
            //add api rate limiting middleware services
            // needed to load configuration from appsettings.json
            services.AddOptions();
            // needed to store rate limit counters and ip rules
            services.AddMemoryCache();
            //add rate limiting middleware to prevent users from calling the API too frequently                      
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            //load ip rules from appsettings.json
            services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));
            //add singleton services that will store rules/counters
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            // configure the resolvers
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            return services;
        }
    }
}
