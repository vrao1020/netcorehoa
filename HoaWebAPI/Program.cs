using AspNetCoreRateLimit;
using HoaInfrastructure.Context;
using HoaWebAPI.Extensions.APIConventions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Serilog;
using System;
using System.Threading.Tasks;

//New .Net Core 2.2 convention that will add ProducesResponseType attribute to controller actions
//The type of ProducesResponseType attributes added depends on the Http request type (GET/POST/PUT/etc.)
[assembly: ApiConventionType(typeof(HoaWebApiConvention))]

namespace HoaWebAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //var connectionString = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName={accname};AccountKey={key};EndpointSuffix=core.windows.net");

            Log.Logger = new LoggerConfiguration()
                          .WriteTo.Console()
                          .WriteTo.Debug(Serilog.Events.LogEventLevel.Information)
                          //.WriteTo.AzureBlobStorage(connectionString, Serilog.Events.LogEventLevel.Error, "logs", "{yyyy}/{MM}/{dd}/apilog.txt", null, true, TimeSpan.FromMinutes(1), 10)
                          .CreateLogger();

            //the build part is split out so that the host can be built and seeded before it is run
            var host = CreateWebHostBuilder(args)
                       .UseSerilog()
                       .Build();

            //seed the database
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var db = services.GetRequiredService<HoaDbContext>();
                    //applies any pending migrations or creates the database if it does not exist
                    db.Database.Migrate();

                    // get the IpPolicyStore instance
                    var ipPolicyStore = services.GetRequiredService<IIpPolicyStore>();

                    // seed IP data from appsettings
                    await ipPolicyStore.SeedAsync();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding.");
                }
            }

            //run the host
            await host.RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   //add configuration to be able to access appsettings.json values
                   //and also to be able to add azure key vault to IConfigurationBuilder
                   //see https://about-azure.com/2018/02/11/using-azure-key-vault-in-asp-net-core-2-0-with-the-options-pattern/
                   .ConfigureAppConfiguration((context, config) =>
                   {
                       config.SetBasePath(context.HostingEnvironment.ContentRootPath)
                           .AddJsonFile("appsettings.json", optional: false)
                           .AddEnvironmentVariables();

                       var builtConfig = config.Build();

                       //config.AddAzureKeyVault(
                       //    builtConfig["azureKeyVault:vault"], builtConfig["azureKeyVault:clientId"],
                       //    builtConfig["azureKeyVault:clientSecret"]);

                   })
                   .UseStartup<Startup>();
    }
}
