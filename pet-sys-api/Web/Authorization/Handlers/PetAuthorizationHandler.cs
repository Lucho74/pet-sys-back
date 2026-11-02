using Application.Models;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Security.Claims;
using Web.Authorization.Operations;

namespace Web.Authorization.Handlers
{
    public class PetAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, PetDTO>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, PetDTO target)
        {
            if (requirement.Name != PetOperations.Read.Name)
            {
                return Task.CompletedTask;
            }

            bool isProfessional = context.User.IsInRole(RoleNames.Veterinarian) || context.User.IsInRole(RoleNames.Admin);

            if (isProfessional)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            if (!context.User.IsInRole(RoleNames.Client))
            {
                return Task.CompletedTask;
            }

            var callerIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(callerIdClaim, out var callerId) && callerId > 0 && target.ClientId == callerId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
