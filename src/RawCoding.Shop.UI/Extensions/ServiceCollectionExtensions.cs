using System;
using System.IO;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RawCoding.Shop.Database;
using RawCoding.Shop.UI.Authorization;

namespace RawCoding.Shop.UI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddShopAuthentication(
            this IServiceCollection services,
            IWebHostEnvironment env,
            IConfiguration config)
        {
            if (env.IsProduction())
            {
                services.AddDataProtection()
                    .SetApplicationName("RawCoding.Shop")
                    .PersistKeysToFileSystem(new DirectoryInfo(config["DataProtectionKeys"]));
            }

            services.AddIdentity<IdentityUser, IdentityRole>(options =>
                {
                    if (env.IsProduction())
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
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Domain = config["CookieDomain"];
                options.AccessDeniedPath= "/Admin/Login";
                options.LoginPath = "/Admin/Login";
            });

            services.AddAuthentication(ShopConstants.Schemas.Guest)
                .AddCookie(ShopConstants.Schemas.Guest,
                    options =>
                    {
                        options.Cookie.Domain = config["CookieDomain"];
                        options.Cookie.Name = ShopConstants.Schemas.Guest;
                        options.ExpireTimeSpan = TimeSpan.FromDays(365);
                        options.LoginPath = "/api/cart/guest-auth";
                    });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(ShopConstants.Policies.Customer, policy => policy
                    .AddAuthenticationSchemes(ShopConstants.Schemas.Guest)
                    .AddRequirements(new ShopRequirement(ShopConstants.Roles.Guest))
                    .RequireAuthenticatedUser());

                options.AddPolicy(ShopConstants.Policies.ShopManager, policy => policy
                    .AddAuthenticationSchemes(IdentityConstants.ApplicationScheme)
                    .AddRequirements(new ShopRequirement(ShopConstants.Roles.ShopManager))
                    .RequireAuthenticatedUser());

                options.AddPolicy(ShopConstants.Policies.Admin, policy => policy
                    .AddAuthenticationSchemes(IdentityConstants.ApplicationScheme)
                    .RequireClaim(ShopConstants.Claims.Role, ShopConstants.Roles.Admin)
                    .RequireAuthenticatedUser());
            });

            return services;
        }

        public static IServiceCollection AddControllersAndPages(this IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = false;
            });

            services.AddControllers()
                .AddFluentValidation(x => x.RegisterValidatorsFromAssembly(typeof(Startup).Assembly));

            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeFolder("/Admin", ShopConstants.Policies.ShopManager);
                options.Conventions.AuthorizePage("/Admin/UserManagement", ShopConstants.Policies.Admin);
                options.Conventions.AllowAnonymousToPage("/Admin/Login");
                options.Conventions.AllowAnonymousToPage("/Admin/Register");
            });

            return services;
        }
    }
}