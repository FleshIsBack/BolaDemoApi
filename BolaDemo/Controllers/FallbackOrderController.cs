using BolaDemo.Data;
using Microsoft.AspNetCore.Mvc;

namespace BolaDemo.Controllers
{
    // Simulates a developer who FORGOT [Authorize] and the ownership check.
    // FallbackPolicy still requires login, but does not check ownership.
    [ApiController]
    [Route("api")]
    public class FallbackOrderController(OrderStore store) : ControllerBase
    {
        [HttpGet("forgotten/orders/{id}")]
        public IActionResult GetOrderForgotten(int id)
        {
            var order = store.GetOrderById(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }
    }
}
