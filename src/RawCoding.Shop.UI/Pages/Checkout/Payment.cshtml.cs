using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RawCoding.Shop.Application.CartActions;
using RawCoding.Shop.Application.Orders;
using RawCoding.Shop.Domain.Extensions;
using RawCoding.Shop.UI.Extensions;
using Stripe;

namespace RawCoding.Shop.UI.Pages.Checkout
{
    public class Payment : PageModel
    {
        public string ClientSecret { get; set; }

        public async Task<IActionResult> OnGet(
            [FromServices] IOptionsMonitor<StripeSettings> optionsMonitor,
            [FromServices] GetCart getCart,
            [FromServices] PaymentIntentService paymentIntentService)
        {
            var userId = User.GetUserId();
            StripeConfiguration.ApiKey = optionsMonitor.CurrentValue.SecretKey;

            var cart = await getCart.ByUserId(userId);
            if (cart == null || cart.Products.Count <= 0)
            {
                return RedirectToPage("/Index");
            }

            var paymentIntent = await paymentIntentService.CreateAsync(new PaymentIntentCreateOptions
            {
                CaptureMethod = "manual",
                Amount = cart.Total(),
                Currency = "gbp",
                ReceiptEmail = cart.Email,
                Shipping = new ChargeShippingOptions
                {
                    Name = cart.Name,
                    Phone = cart.Phone,
                    Address = new AddressOptions
                    {
                        Line1 = cart.Address1,
                        Line2 = cart.Address2,
                        City = cart.City,
                        Country = cart.Country,
                        PostalCode = cart.PostCode,
                        State = cart.State,
                    },
                },
            });

            ClientSecret = paymentIntent.ClientSecret;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(
            string paymentId,
            [FromServices] CreateOrder createOrder,
            [FromServices] GetCart getCart,
            [FromServices] PaymentIntentService paymentIntentService,
            [FromServices] ILogger<Payment> logger)
        {
            var userId = User.GetUserId();
            var cartId = await getCart.Id(userId);

            if (cartId == 0)
            {
                logger.LogWarning($"Cart not found for {userId} with payment id {paymentId}");
                return RedirectToPage("/Checkout/Error");
            }

            var payment = await paymentIntentService.CaptureAsync(paymentId);

            if (payment == null)
            {
                logger.LogWarning($"Payment Intent not found {paymentId}");
                return RedirectToPage("/Checkout/Error");
            }

            var order = new Domain.Models.Order
            {
                StripeReference = paymentId,
                CartId = cartId,
            };
            await createOrder.Do(order);

            return RedirectToPage("/Checkout/Success", new {orderId = order.Id});
        }
    }
}