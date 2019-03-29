using System;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MemoryStorage;
using HoaWebApplication.ApiHttpClient;
using HoaWebApplication.Services.Azure;
using HoaWebApplication.Services.Cache;
using HoaWebApplication.Services.Email;
using HoaWebApplication.Services.File;
using HoaWebApplication.Services.AuthenticatedUser;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http;
using HoaCommon;
using HoaWebApplication.Services.AccessToken;
using Microsoft.AspNetCore.Routing;
using System.IO;
using Firewall;
using HoaWebApplication.Services.IdentityServer;
using System.IdentityModel.Tokens.Jwt;

namespace HoaWebApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
            _staticConfig = configuration;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        //static configuration to be used to access appsettings.json in UserClaims extensions
        public static IConfiguration _staticConfig { get; private set; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            //Configure the cookie temp data provider and mark the cookie as essential
            //without this configuration, if a user does not accept the cookie policy
            //temp data cannot be used across pages (the cookie is never used)
            //as temp data uses non-sensitive info, this is okay to configure as such
            services.Configure<CookieTempDataProviderOptions>(options =>
            {
                options.Cookie.IsEssential = true;
            });

            //add CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder =>
                    {
                        builder
                        .WithOrigins("https://localhost:44315/", "https://hoa.com")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                    });
            });

            // Add authentication services
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "oidc";
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme) //by default redirects to /account/accessdenied if unauthorized (note this does not mean not authenticated)
            .AddOpenIdConnect("oidc", options =>
            {
                // Set the authority
                options.Authority = Configuration["IdentityServer:Authority"]; //hoaidentityserver

                // Configure the Auth0 Client ID and Client Secret
                options.ClientId = Configuration["IdentityServer:ClientId"];
                options.ClientSecret = Configuration["IdentityServer:ClientSecret"];

                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                // Set response type to code
                options.ResponseType = "code id_token";

                // Configure the scope
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.Scope.Add("roles");
                options.Scope.Add("hoawebapi");
                options.Scope.Add("offline_access");

                //saves JWT in Authorization cookie so that it can be accessed via HttpContext
                options.SaveTokens = true;

                // Configure the Claims Issuer to be IdentityServer
                options.ClaimsIssuer = "HoaIdentityServer";

                options.GetClaimsFromUserInfoEndpoint = true;
            });

            services.AddHttpContextAccessor();

            //typed HttpClients that creates a HttpClient with the URL set to the API
            //each client is also configured with a default handler for using gzip decompression
            //see the typed client configuration where an accept-encoding header is sent so that
            //the api sends gzip compressed responses
            services.AddHttpClient<ApiClient>()
                .ConfigurePrimaryHttpMessageHandler(handler =>
                    new HttpClientHandler()
                    {
                        AutomaticDecompression = System.Net.DecompressionMethods.GZip
                    });
            services.AddHttpClient<IdentityServerApiClient>()
                .ConfigurePrimaryHttpMessageHandler(handler =>
                    new HttpClientHandler()
                    {
                        AutomaticDecompression = System.Net.DecompressionMethods.GZip
                    });

            ////add azure connection service - this will fetch the blob container
            ////need to verify if this needs to be singleton or scoped
            services.AddSingleton<IAzureBlobContainer, AzureBlobContainer>();

            //add azure service that will fetch a single file from blob storage
            services.AddScoped<IAzureBlob, AzureBlob>();

            //add azure service that will provide a download URL to an azure blob
            //this service will modify the url to contain a sas token with a content-disposition header that will force download
            services.AddScoped<IAzureSASTokenUrl, AzureSASTokenUrl>();

            //add file validator service. This will validate file extension, size, and length and will return an appropriate error message
            services.AddScoped<IFileValidate, FileValidate>();

            //add cache service that will be used for getting/setting values
            services.AddScoped<ICacheStore, CacheStore>();

            //service that checks whether the user exists in the API when logging in
            //if does not exist creates user else updates user information if logged in via social account
            services.AddScoped<IUserService, UserService>();

            //service for sending emails 
            services.AddScoped<ISendEmail, SendEmail>();

            //service for getting access token to the API
            services.AddScoped<IGetAPIAccessToken, GetAPIAccessToken>();

            //service for managing identity users
            services.AddScoped<IIDPUserService, IDPUserService>();

            //add Automapper for mapping between entities and input/output models
            services.AddAutoMapper();

            //Redis Cache middleware for distributed caching
            //services.AddDistributedRedisCache(options =>
            //{
            //    options.Configuration = "localhost";
            //    options.InstanceName = "SampleInstance";
            //});
            services.AddDistributedMemoryCache();

            //add Hangfire services
            //use memory server in development and SQL database in production
            if (Environment.IsDevelopment())
            {
                var inMemory = GlobalConfiguration.Configuration.UseMemoryStorage();
                services.AddHangfire(config =>
                {
                    config.UseStorage(inMemory);
                });
            }
            else
            {
                //using in-memory for development as well
                var inMemory = GlobalConfiguration.Configuration.UseMemoryStorage();
                services.AddHangfire(config =>
                {
                    config.UseStorage(inMemory);
                });

                //services.AddHangfire(config =>
                //{
                //    config.UseSqlServerStorage(Configuration["HangFire"]);
                //});
            }

            services.AddMvc()
                .AddFluentValidation(fv =>
                {
                    //add fluent validation
                    fv.RunDefaultMvcValidationAfterFluentValidationExecutes = true;
                    fv.RegisterValidatorsFromAssemblyContaining<Startup>();
                    fv.RegisterValidatorsFromAssemblyContaining<ValidatorAssembly>();

                    //below call is required to validate model state of actions that take in 
                    //IEnumerable of the models/dtos that need to be validated
                    //if not added, IEnumerable models/dtos will not be validated
                    fv.ImplicitlyValidateChildProperties = true;
                })
                .AddRazorOptions(options =>
                {
                    //by default MVC only searches /pages/shared, /pages, and /pages/{current executing folder}
                    //to search for partial view names
                    //this will add an additional location for the framework to search
                    //otherwise the partial view will have to exist in one of the above locations
                    //see https://www.learnrazorpages.com/razor-pages/partial-pages for information
                    options.PageViewLocationFormats.Add("/Pages/Partial/{0}.cshtml");
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //app.UseFirewall(
            //   FirewallRulesEngine
            //       .DenyAllAccess()
            //       .ExceptFromCountries(new[] { CountryCode.US })
            //       .ExceptFromLocalhost(),
            //   accessDeniedDelegate:
            //       ctx =>
            //       {
            //           ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            //           return ctx.Response.WriteAsync("You can only access this website from the US.");
            //       });

            if (env.IsDevelopment())
            {
                app.UseStatusCodePagesWithReExecute("/NotFound");
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseStatusCodePagesWithReExecute("/NotFound");
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse =
                    r =>
                    {
                        string path = r.File.PhysicalPath;
                        if (path.EndsWith(".css") || path.EndsWith(".js") || path.EndsWith(".gif") || path.EndsWith(".jpg") || path.EndsWith(".png") || path.EndsWith(".svg"))
                        {
                            TimeSpan maxAge = new TimeSpan(7, 0, 0, 0);
                            r.Context.Response.Headers.Append("Cache-Control", "max-age=" + maxAge.TotalSeconds.ToString("0"));
                        }
                    }
            });
            app.UseCookiePolicy();

            //add hangfire service 
            //app.UseHangfireServer(new BackgroundJobServerOptions
            //{
            //    HeartbeatInterval = new TimeSpan(0, 10, 0),
            //    ServerCheckInterval = new TimeSpan(0, 10, 0),
            //    SchedulePollingInterval = new TimeSpan(0, 10, 0)
            //});
            //app.UseHangfireDashboard("/hangfire", new DashboardOptions
            //{
            //    IsReadOnlyFunc = (DashboardContext context) => true
            //});

            //schedule a recurring job that will send reminder emails if the necessary conditions are met
            //RecurringJob.AddOrUpdate<ISendEmail>((email) => email.SendReminderEmailsAsync(), Cron.Daily);

            //need to add below for LetsEncrypt as it looks for below file
            //see https://weblog.west-wind.com/posts/2017/sep/09/configuring-letsencrypt-for-aspnet-core-and-iis#2.-Fix-LetsEncrypt--WinSimple-Web.config
            //app.UseRouter(r =>
            //{
            //    r.MapGet(".well-known/acme-challenge/{id}", async (request, response, routeData) =>
            //    {
            //        var id = routeData.Values["id"] as string;
            //        var file = Path.Combine(env.ContentRootPath, ".well-known", "acme-challenge", id);
            //        await response.SendFileAsync(file);
            //    });
            //});

            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
