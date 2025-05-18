using Application.Dtos.Response;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AuthMS.Authorization
{
    // Comprueba que el UserId del token coincida con el response.UserId
    public class SameUserHandler : AuthorizationHandler<SameUserRequirement, ReservationResponse>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            SameUserRequirement requirement,
            ReservationResponse resource)
        {
            var sub = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(sub, out var userId) && resource.UserId == userId)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
