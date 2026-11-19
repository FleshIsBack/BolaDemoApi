using BolaDemo.Authorization;
using BolaDemo.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolaDemo.Controllers
{
    // Same protection as GetOrderSecure, but the ownership check lives in OrderOwnerHandler
    [ApiController]
    [Route("api")]
    public class HandlerOrderController(OrderStore store, IAuthorizationService auth) : ControllerBase
    {
        [Authorize]
        [HttpGet("handler/orders/{id}")]
        public async Task<IActionResult> GetOrderWithHandler(int id)
        {
            var order = store.GetOrderById(id);
            if (order == null)
            {
                return NotFound();
            }

            var result = await auth.AuthorizeAsync(User, order, new OrderOwnerRequirement());
            if (!result.Succeeded)
            {
                return NotFound(); // 404, not 403: don't reveal that the order exists
            }

            return Ok(order);
        }
    }
}
