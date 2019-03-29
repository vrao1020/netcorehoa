using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Serilog;
using System;

namespace HoaWebApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var connectionString = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName={accname};AccountKey={key};EndpointSuffix=core.windows.net");

            //Log.Logger = new LoggerConfiguration()
            //              .WriteTo.Console()
            //              .WriteTo.Debug(Serilog.Events.LogEventLevel.Information)
            //              .WriteTo.AzureBlobStorage(connectionString, Serilog.Events.LogEventLevel.Error, "logs", "{yyyy}/{MM}/{dd}/weblog.txt", null, true, TimeSpan.FromMinutes(1), 10)
            //              .CreateLogger();

            //the build part is split out so that the host can be built and seeded before it is run
            var host = CreateWebHostBuilder(args)
                       .UseSerilog()
                       .Build();

            //run the host
            host.Run();
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
