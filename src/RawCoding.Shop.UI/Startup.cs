using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RawCoding.Shop.Database;
using Stripe;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RawCoding.S3;
using RawCoding.Shop.UI.Extensions;
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
            services.Configure<StripeSettings>(_config.GetSection(nameof(StripeSettings)));
            services.Configure<ShopSettings>(_config.GetSection(nameof(ShopSettings)));
            StripeConfiguration.ApiKey = _config.GetSection("Stripe")["SecretKey"];

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                if (_env.IsProduction())
                {
                    options.UseNpgsql(_config.GetConnectionString("DefaultConnection"));
                }
                else
                {
                    options.UseInMemoryDatabase("Dev");
                }
            });

            services
                .AddControllersAndPages()
                .AddShopAuthentication(_env, _config)
                .AddApplicationServices()
                .AddEmailService(_config)
                .AddRawCodingS3Client(() => _config.GetSection(nameof(S3StorageSettings)).Get<S3StorageSettings>())
                .AddScoped<PaymentIntentService>();
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
            }

            app.UseStaticFiles()
                .UseCookiePolicy()
                .UseRouting()
                .UseMiddleware<ShopMiddleware>()
                .UseStatusCodePages(context =>
                {
                    var pathBase = context.HttpContext.Request.PathBase;
                    var endpoint = StatusCodeEndpoint(context.HttpContext.Response.StatusCode);
                    context.HttpContext.Response.Redirect(pathBase + endpoint);
                    return Task.CompletedTask;
                })
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
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
    }
}