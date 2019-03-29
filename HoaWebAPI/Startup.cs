using HoaInfrastructure.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation.AspNetCore;
using System.IO;
using System;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using HoaWebAPI.Extensions.ExceptionHandlers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using HoaCommon;
using Microsoft.AspNetCore.Routing;
using HoaCommon.Services;
using HoaWebAPI.Extensions.ServiceConfiguration;

namespace HoaWebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            _environment = environment;
            _staticConfig = configuration;
        }

        public IConfiguration Configuration { get; }
        private IHostingEnvironment _environment;

        //static configuration to be used to access appsettings.json in UserClaims extensions
        public static IConfiguration _staticConfig { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (_environment.IsDevelopment()) //setup database depending on environment
            {
                //services.AddDbContext<HoaDbContext>(opt => opt.UseInMemoryDatabase("HOA"));
                services.AddDbContext<HoaDbContext>(opt => opt.UseSqlServer(Configuration["ConnectionStrings:TestDBConnection"]));
            }
            else
            {
                services.AddDbContext<HoaDbContext>(opt => opt.UseSqlServer(Configuration["ConnectionStrings:DBConnection"]));
            }

            services.AddMvc(configure =>
                {
                    //add XML input/output formatters for content negotioation
                    configure.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter()); //add XML output formatter for content negotiation
                    configure.InputFormatters.Add(new XmlDataContractSerializerInputFormatter(configure)); //add XML input formatter for content negotiation
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddFluentValidation(fv =>
                { //add fluent validation
                    fv.RunDefaultMvcValidationAfterFluentValidationExecutes = true;
                    fv.RegisterValidatorsFromAssemblyContaining<ValidatorAssembly>();
                    //below call is required to validate model state of actions that take in 
                    //IEnumerable of the models/dtos that need to be validated
                    //if not added, IEnumerable models/dtos will not be validated
                    fv.ImplicitlyValidateChildProperties = true;
                });

            //add Sieve for pagination, filtering, and sorting
            services.AddSieve(Configuration);

            //add pagination service
            services.AddScoped<IPaginationGenerator, PaginationGenerator>();

            //add repository services for CRUD operations
            services.AddRepositories();

            //add sort/filter services
            services.AddSortingFiltering();

            //add action context accessor for generating pagination links
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            //add HttpContextAccessor for getting user data from static class (UserService)
            //required as the service needs access to the HttpContext
            services.AddHttpContextAccessor();

            //add Automapper for mapping between entities and input/output models
            services.AddAutoMapper();

            //Configures the APIController behavior to return UnprocessableEntity result
            //instead of the default BadRequest result when model validation fails
            //Note that this result is returned automatically anytime validation fails
            services.Configure<ApiBehaviorOptions>(options =>
                options.InvalidModelStateResponseFactory = (actionContext) =>
                {
                    return new UnprocessableEntityObjectResult(actionContext.ModelState);
                }
             );

            //add Swagger for API information
            services.AddSwagger();

            //add JWT authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                options.Authority = Configuration["IdentityServer:Authority"];
                options.Audience = Configuration["IdentityServer:Audience"];
                options.RequireHttpsMetadata = true;
            });

            //add claims authorization for user endpoint. This is to prevent regular users from accessing most of the user endpoints
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminUser", policy => policy.RequireClaim(Configuration["AuthorizationClaims:Admin"], Configuration["AuthorizationClaimsValues:Admin"]));
                options.AddPolicy("CRUDAccess", policy => policy.RequireClaim(Configuration["AuthorizationClaims:ReadOnly"], Configuration["AuthorizationClaimsValues:ReadOnly"]));
                options.AddPolicy("PostCreation", policy => policy.RequireClaim(Configuration["AuthorizationClaims:PostCreation"], Configuration["AuthorizationClaimsValues:PostCreation"]));
            });

            ////Redis Cache middleware for distributed caching
            //services.AddDistributedRedisCache(options =>
            //{
            //    options.Configuration = "localhost";
            //    options.InstanceName = "SampleInstance";
            //});

            //add distributed memory cache as redis cache pricing is expensive on azure
            services.AddDistributedMemoryCache();

            //Marvin cache header middleware - not a cache - just adds caching headers
            services.AddHttpCacheHeaders(
                expirationModelOptions =>
                {
                    expirationModelOptions.MaxAge = Int32.Parse(Configuration["Cache:MaxAge"]);
                },
                validationModelOptions =>
                {
                    validationModelOptions.MustRevalidate = true;
                    validationModelOptions.ProxyRevalidate = true;
                });


            //add api rate limiting middleware services
            services.AddIPRateLimit(Configuration);

            // add support for compressing responses (eg gzip)
            services.AddResponseCompression(opt =>
            {
                opt.Providers.Add<GzipCompressionProvider>();
                opt.EnableForHttps = true;
            });
            services.Configure<GzipCompressionProviderOptions>(options =>
            options.Level = CompressionLevel.Fastest);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IMapper autoMapper)
        {
            //verifies mapping between models is valid and throws errors if incorrect mappings 
            //exist between properties. Will display errors on starting the project
            autoMapper.ConfigurationProvider.AssertConfigurationIsValid();

            //add rate limiting middleware before any other because we need to ensure limits are being checked first
            app.UseIpRateLimiting();

            // use response compression (client should pass through 
            // Accept-Encoding)
            app.UseResponseCompression();

            if (!env.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();

            //add swagger endpoint
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "HOA API V1");
            });
            //need to determine if nswag is an alternative
            app.UseAuthentication();

            app.UseHttpCacheHeaders();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //add exception handling middleware
                //this needs to be added just prior to MVC as it will send the response to the user
                app.UseMiddleware(typeof(ExceptionHandlingMiddleware));
            }

            //need to add below for LetsEncrypt as it looks for below file
            //see https://weblog.west-wind.com/posts/2017/sep/09/configuring-letsencrypt-for-aspnet-core-and-iis#2.-Fix-LetsEncrypt--WinSimple-Web.config
            app.UseRouter(r =>
            {
                r.MapGet(".well-known/acme-challenge/{id}", async (request, response, routeData) =>
                {
                    var id = routeData.Values["id"] as string;
                    var file = Path.Combine(env.ContentRootPath, ".well-known", "acme-challenge", id);
                    await response.SendFileAsync(file);
                });
            });

            app.UseMvc();
        }
    }
}
