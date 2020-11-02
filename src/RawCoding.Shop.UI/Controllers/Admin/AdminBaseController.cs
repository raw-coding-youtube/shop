using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RawCoding.Shop.UI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(ShopConstants.Policies.ShopManager)]
    public class AdminBaseController : ControllerBase
    {
    }
}