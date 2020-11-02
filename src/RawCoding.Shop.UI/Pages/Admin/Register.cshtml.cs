using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RawCoding.Shop.UI.Pages.Admin
{
    public class RegisterModel : PageModel
    {
        [BindProperty] public RegisterViewModel Input { get; set; }

        public void OnGet(string email, string code)
        {
            Input = new RegisterViewModel
            {
                Email = email,
                Code = code,
            };
        }

        public async Task<IActionResult> OnPost(
            [FromServices] SignInManager<IdentityUser> signInManager,
            [FromServices] UserManager<IdentityUser> userManager)
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await userManager.FindByNameAsync(Input.Email);
            var resetResult = await userManager.ResetPasswordAsync(user, Input.Code, Input.Password);

            if (!resetResult.Succeeded)
            {
                return Page();
            }

            await signInManager.SignInAsync(user, false);

            return RedirectToPage("/Admin/Index");
        }

        public class RegisterViewModel
        {
            [Required] public string Code { get; set; }
            [Required] public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Compare(nameof(Password))]
            public string ConfirmPassword { get; set; }
        }
    }
}