using HoaWebAPI.Extensions.Sieve;
using HoaWebAPI.Extensions.Sieve.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sieve.Models;
using Sieve.Services;

namespace HoaWebAPI.Extensions.ServiceConfiguration
{
    public static class SieveServices
    {
        public static IServiceCollection AddSieve(this IServiceCollection services, IConfiguration Configuration)
        {
            //add Sieve for pagination, filtering, and sorting
            services.Configure<SieveOptions>(Configuration.GetSection("Sieve"));

            //adds custom filter methods for Sieve. This is because there is transformation 
            //taking place between Entities and View Models. In order to filter on the 
            //View Model properties, custom filters need to be added as otherwise Entities 
            //won't be able to filter properly
            services.AddScoped<ISieveCustomFilterMethods, SieveCustomFilterMethods>();

            //add sieve processor that maps Entity properties for sorting/filtering
            services.AddScoped<ISieveProcessor, ApplicationSieveProcessor>();

            return services;
        }
    }
}
