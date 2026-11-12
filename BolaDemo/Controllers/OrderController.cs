using Microsoft.AspNetCore.Mvc;
using BolaDemo.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BolaDemo.Controllers
{
    [ApiController]
    [Route("api")]
    public class OrderController(OrderStore store) : ControllerBase
    {
        [Authorize]
        [HttpGet("vulnerable/orders/{id}")]
        public IActionResult GetOrderVulnerable(int id)
        {
            var order = store.GetOrderById(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }
        [Authorize]
        [HttpGet("secure/orders/{id}")]
        public IActionResult GetOrderSecure(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }
            var order = store.GetByIdForUser(id, userId);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }
    }
}
