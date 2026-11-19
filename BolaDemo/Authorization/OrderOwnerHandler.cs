using System.Security.Claims;
using BolaDemo.Models;
using Microsoft.AspNetCore.Authorization;

namespace BolaDemo.Authorization
{
    // The rule: "the logged-in user must own the order"
    public class OrderOwnerRequirement : IAuthorizationRequirement { }

    public class OrderOwnerHandler : AuthorizationHandler<OrderOwnerRequirement, Order>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, OrderOwnerRequirement requirement, Order order)
        {
            // User id from the signed JWT, never from the request
            var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(claim, out var userId) && order.UserId == userId)
            {
                context.Succeed(requirement);
            }

            // No Succeed = denied (deny by default)
            return Task.CompletedTask;
        }
    }
}
