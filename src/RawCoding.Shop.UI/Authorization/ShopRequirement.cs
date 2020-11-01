using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace RawCoding.Shop.UI.Authorization
{
    public class ShopRequirement : AuthorizationHandler<ShopRequirement>, IAuthorizationRequirement
    {
        private readonly string[] _roles;

        public ShopRequirement(params string[] roles)
        {
            _roles = roles;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ShopRequirement requirement)
        {
            if (context.User != null &&
                (context.User.HasClaim(ShopConstants.Claims.Role, ShopConstants.Roles.Admin)
                 || context.User.HasClaim(x => x.Type == ShopConstants.Claims.Role && _roles.Contains(x.Value))))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}