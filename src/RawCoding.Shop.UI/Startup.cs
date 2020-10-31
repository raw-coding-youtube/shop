using System;
using System.IO;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RawCoding.Shop.Database;
using Stripe;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using RawCoding.S3;
using RawCoding.Shop.UI.Middleware.Shop;
using RawCoding.Shop.UI.Workers.Email;

namespace RawCoding.Shop.UI
{
    public class Startup
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public Startup(
            IConfiguration configuration,
            IWebHostEnvironment env)
        {
            _config = configuration;
            _env = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<StripeSettings>(_config.GetSection(nameof(StripeSettings)));
            services.Configure<ShopSettings>(_config.GetSection(nameof(ShopSettings)));

            if (_env.IsProduction())
            {
                services.AddDataProtection()
                    .SetApplicationName("RawCoding.Shop")
                    .PersistKeysToFileSystem(new DirectoryInfo(_config["DataProtectionKeys"]));
            }

            // services.AddDbContext<ApplicationDbContext>(options => options
            //     .UseNpgsql(_config.GetConnectionString("DefaultConnection")));
            // if (_env.IsProduction())
            // {
            // }
            // else
            // {
            services.AddDbContext<ApplicationDbContext>(options => options
                .UseInMemoryDatabase("Dev"));
            // }

            services.AddIdentity<IdentityUser, IdentityRole>(options =>
                {
                    if (_env.IsProduction())
                    {
                        options.Password.RequireDigit = true;
                        options.Password.RequiredLength = 8;
                        options.Password.RequireNonAlphanumeric = true;
                        options.Password.RequireUppercase = true;
                    }
                    else
                    {
                        options.Password.RequireDigit = false;
                        options.Password.RequiredLength = 6;
                        options.Password.RequireNonAlphanumeric = false;
                        options.Password.RequireUppercase = false;
                    }
                })
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.ConfigureApplicationCookie(config =>
            {
                config.Cookie.Domain = _config["CookieDomain"];
                config.LoginPath = "/Admin/Login";
            });

            services.AddAuthentication(ShopConstants.Schemas.Guest)
                .AddCookie(ShopConstants.Schemas.Guest,
                    config =>
                    {
                        config.Cookie.Domain = _config["CookieDomain"];
                        config.Cookie.Name = ShopConstants.Schemas.Guest;
                        config.ExpireTimeSpan = TimeSpan.FromDays(365);
                        config.LoginPath = "/api/cart/guest-auth";
                    });

            services.AddAuthorization(config =>
            {
                config.AddPolicy(ShopConstants.Policies.Customer, policy => policy
                    .AddAuthenticationSchemes(ShopConstants.Schemas.Guest)
                    .AddRequirements(new GuestRequirement())
                    .RequireAuthenticatedUser());

                config.AddPolicy(ShopConstants.Policies.Admin, policy => policy
                    .AddAuthenticationSchemes(IdentityConstants.ApplicationScheme)
                    .RequireClaim(ShopConstants.Claims.Role, ShopConstants.Roles.Admin)
                    .RequireAuthenticatedUser());
            });

            StripeConfiguration.ApiKey = _config.GetSection("Stripe")["SecretKey"];

            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = false;
            });

            services.AddControllers()
                .AddFluentValidation(x => x.RegisterValidatorsFromAssembly(typeof(Startup).Assembly));

            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeFolder("/Admin", ShopConstants.Policies.Admin);
                options.Conventions.AllowAnonymousToPage("/Admin/Login");
            });

            services.AddApplicationServices()
                .AddEmailService(_config)
                .AddRawCodingS3Client(() => _config.GetSection(nameof(S3StorageSettings)).Get<S3StorageSettings>())
                .AddScoped<PaymentIntentService>();

            services.AddHttpClient();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseMiddleware<ShopMiddleware>();

            app.UseStatusCodePages(context =>
            {
                var pathBase = context.HttpContext.Request.PathBase;
                var endpoint = StatusCodeEndpoint(context.HttpContext.Response.StatusCode);
                context.HttpContext.Response.Redirect(pathBase + endpoint);
                return Task.CompletedTask;
            });

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute()
                    .RequireAuthorization(ShopConstants.Policies.Customer);

                endpoints.MapRazorPages()
                    .RequireAuthorization(ShopConstants.Policies.Customer);
            });
        }

        private static string StatusCodeEndpoint(int code) =>
            code switch
            {
                StatusCodes.Status404NotFound => "/not-found",
                _ => "/",
            };

        public class GuestRequirement : AuthorizationHandler<GuestRequirement>, IAuthorizationRequirement
        {
            protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, GuestRequirement requirement)
            {
                if (context.User != null)
                {
                    if (context.User.HasClaim(ShopConstants.Claims.Role, ShopConstants.Roles.Admin)
                        || context.User.HasClaim(ShopConstants.Claims.Role, ShopConstants.Roles.Guest))
                    {
                        context.Succeed(requirement);
                    }
                }

                return Task.CompletedTask;
            }
        }
    }
}