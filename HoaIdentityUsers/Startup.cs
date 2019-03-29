using HoaCommon.Services;
using HoaIdentityUsers.Data;
using HoaIdentityUsers.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Compression;

namespace HoaIdentityUsers
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            })
            .AddJwtBearer(options =>
            {
                options.Authority = "https://localhost:44367";
                options.Audience = "usermgapi";
                options.RequireHttpsMetadata = true;
            });

            //add pagination service
            services.AddScoped<IPaginationGenerator, PaginationGenerator>();

            //add action context accessor for generating pagination links
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            //add HttpContextAccessor for getting user data from static class (UserService)
            //required as the service needs access to the HttpContext
            services.AddHttpContextAccessor();

            //Configures the APIController behavior to return UnprocessableEntity result
            //instead of the default BadRequest result when model validation fails
            //Note that this result is returned automatically anytime validation fails
            services.Configure<ApiBehaviorOptions>(options =>
                options.InvalidModelStateResponseFactory = (actionContext) =>
                {
                    return new UnprocessableEntityObjectResult(actionContext.ModelState);
                }
             );

            // add support for compressing responses (eg gzip)
            services.AddResponseCompression(opt =>
            {
                opt.Providers.Add<GzipCompressionProvider>();
                opt.EnableForHttps = true;
            });
            services.Configure<GzipCompressionProviderOptions>(options =>
            options.Level = CompressionLevel.Fastest);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // use response compression (client should pass through 
            // Accept-Encoding)
            app.UseResponseCompression();

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
