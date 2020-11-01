using System.Threading.Tasks;
using RawCoding.Shop.Domain.Models;

namespace RawCoding.Shop.Application.Emails
{
    public interface IEmailTemplateFactory
    {
        public Task<string> RenderRegisterInvitationAsync(string link);
        public Task<string> RenderOrderConfirmationAsync(Order order);
        public Task<string> RenderShippingConfirmationAsync(Order order);
    }
}