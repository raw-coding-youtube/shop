using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace RawCoding.Shop.UI.Authorization
{
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