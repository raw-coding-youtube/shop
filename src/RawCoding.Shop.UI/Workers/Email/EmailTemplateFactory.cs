using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotLiquid;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using RawCoding.Shop.Application.Emails;
using RawCoding.Shop.Application.Projections;
using RawCoding.Shop.Domain.Extensions;
using RawCoding.Shop.Domain.Models;

namespace RawCoding.Shop.UI.Workers.Email
{
    public class EmailTemplateFactory : IEmailTemplateFactory
    {
        private readonly IWebHostEnvironment _env;
        private Dictionary<string, Template> TemplateCache { get; } = new Dictionary<string, Template>();

        public EmailTemplateFactory(IWebHostEnvironment env)
        {
            _env = env;
        }

        public Task<string> RenderOrderConfirmationAsync(Order order) =>
            Compose(
                RenderHeaderAsync(),
                RenderTemplateAsync("order-confirmation", OrderProjections.Project(order)),
                RenderFooterAsync()
            );

        public Task<string> RenderShippingConfirmationAsync(Order order) => Compose(
            RenderHeaderAsync(),
            RenderTemplateAsync("shipping-confirmation", OrderProjections.Project(order)),
            RenderFooterAsync()
        );

        private Task<string> RenderHeaderAsync() => RenderTemplateAsync("header");
        private Task<string> RenderFooterAsync() => RenderTemplateAsync("footer");

        private static async Task<string> Compose(params Task<string>[] components) =>
            string.Concat(await Task.WhenAll(components));

        private async Task<string> RenderTemplateAsync(string templateName, object seed = null)
        {
            var templatePath = Path.Combine(_env.ContentRootPath, "EmailTemplates", $"{templateName}.liquid");
            if (_env.IsDevelopment() || !TemplateCache.TryGetValue(templatePath, out var template))
            {
                var templateString = await File.ReadAllTextAsync(templatePath);
                TemplateCache[templatePath] = template = Template.Parse(templateString);
            }

            return template.Render(Hash.FromAnonymousObject(seed));
        }
    }
}