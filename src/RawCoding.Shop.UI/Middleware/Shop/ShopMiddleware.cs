using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace RawCoding.Shop.UI.Middleware.Shop
{
    public class ShopMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOptionsMonitor<ShopSettings> _optionsMonitor;

        public ShopMiddleware(RequestDelegate next, IOptionsMonitor<ShopSettings> optionsMonitor)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));

            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        }

        public async Task Invoke(HttpContext context)
        {
            var adminEndpoint = context.Request.Path.StartsWithSegments("/admin") || context.Request.Path.StartsWithSegments("/api/admin");
            var closedEndpoint = context.Request.Path.StartsWithSegments("/closed");
            if (!_optionsMonitor.CurrentValue.Open && !adminEndpoint && !closedEndpoint)
            {
                context.Response.Redirect("/closed");
            }
            else if (_optionsMonitor.CurrentValue.Open && closedEndpoint)
            {
                context.Response.Redirect("/");
            }

            await _next(context);
        }
    }
}