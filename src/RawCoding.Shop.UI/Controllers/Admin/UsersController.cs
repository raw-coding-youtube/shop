using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RawCoding.Shop.Application.Emails;
using RawCoding.Shop.Database;

namespace RawCoding.Shop.UI.Controllers.Admin
{
    [Authorize(ShopConstants.Policies.Admin)]
    public class UsersController : AdminBaseController
    {
        private readonly UserManager<IdentityUser> _userManager;

        public UsersController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> ListManagers([FromServices] ApplicationDbContext ctx)
        {
            var users = await _userManager
                .GetUsersForClaimAsync(new Claim(ShopConstants.Claims.Role, ShopConstants.Roles.ShopManager));

            return Ok(users.Select(x => new
            {
                x.Id,
                x.UserName,
                x.Email,
            }));
        }


        [HttpPost]
        public async Task<IActionResult> CreateUser(
            string email,
            [FromServices] IEmailSink emailSink,
            [FromServices] IEmailTemplateFactory emailTemplateFactory)
        {
            var user = new IdentityUser(email)
            {
                Email = email,
            };

            await _userManager.CreateAsync(user, $"!{Guid.NewGuid().ToString()}");
            await _userManager.AddClaimAsync(user, new Claim(ShopConstants.Claims.Role, ShopConstants.Roles.ShopManager));
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);

            var link = Url.Page("/Admin/Register", "Get", new {email, code}, protocol: HttpContext.Request.Scheme);
            await emailSink.SendAsync(new SendEmailRequest
            {
                To = email,
                Subject = "Raw Coding - Account Activation",
                Message = await emailTemplateFactory.RenderRegisterInvitationAsync(link),
                Html = true,
            });

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> ListManagers(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NoContent();
            }

            await _userManager.DeleteAsync(user);
            return Ok();
        }
    }
}