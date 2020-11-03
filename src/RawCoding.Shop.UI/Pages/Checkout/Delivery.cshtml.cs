using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;
using RawCoding.Shop.Application.CartActions;
using RawCoding.Shop.UI.Extensions;

namespace RawCoding.Shop.UI.Pages.Checkout
{
    public class Delivery : PageModel
    {
        [BindProperty] public CheckoutForm Form { get; set; }

        public async Task<IActionResult> OnGet(
            [FromServices] GetCart getCart,
            [FromServices] IWebHostEnvironment env)
        {
            var userId = User.GetUserId();
            var cart = await getCart.ByUserId(userId);
            if (cart.Products.Count <= 0)
            {
                return RedirectToPage("/Index");
            }

            Form = new CheckoutForm();

            if (env.IsDevelopment())
            {
                Form.Name = "test";
                Form.Email = "info@raw-coding.dev";
                Form.Phone = "7845556789";
                Form.Address1 = "Test";
                Form.Address2 = "";
                Form.City = "City";
                Form.Country = "Country";
                Form.PostCode = "QQ1 2RR";
                Form.State = "";
            }
            else if (cart.DeliveryInformationComplete)
            {
                Form.Name = cart.Name;
                Form.Email = cart.Email;
                Form.Phone = cart.Phone;
                Form.Address1 = cart.Address1;
                Form.Address2 = cart.Address2;
                Form.City = cart.City;
                Form.Country = cart.Country;
                Form.PostCode = cart.PostCode;
                Form.State = cart.State;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(
            [FromServices] UpdateCart updateCart)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = User?.Claims?.FirstOrDefault(x => x.Type.Equals(ClaimTypes.NameIdentifier))?.Value;
            await updateCart.Do(userId, cart =>
            {
                cart.Name = Form.Name;
                cart.Email = Form.Email;
                cart.Phone = Form.Phone;
                cart.Address1 = Form.Address1;
                cart.Address2 = Form.Address2;
                cart.City = Form.City;
                cart.Country = Form.Country;
                cart.PostCode = Form.PostCode;
                cart.State = Form.State;
                cart.DeliveryInformationComplete = true;
            });

            return RedirectToPage("/Checkout/Payment");
        }

        public class CheckoutForm
        {
            [Required] public string Name { get; set; }

            [Required]
            [DataType(DataType.EmailAddress)]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.PhoneNumber)]
            public string Phone { get; set; }

            [Required] public string Address1 { get; set; }
            public string Address2 { get; set; }
            [Required] public string City { get; set; }
            [Required] public string Country { get; set; }
            [Required] public string PostCode { get; set; }
            public string State { get; set; }
        }
    }
}