using Application.Models;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Security.Claims;
using Web.Authorization.Operations;
using Web.Authorization.Rules;

namespace Web.Authorization.Handlers
{
    public class UserAccountAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, UserDTO>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, UserDTO target)
        {
            bool isAllowed = false;

            if (requirement.Name == UserAccountOperations.Read.Name)
            {
                isAllowed = CanAccessManagedAccount(context, target);
            }
            else if (requirement.Name == UserAccountOperations.Update.Name)
            {
                isAllowed = CanUpdateAccount(context, target);
            }

            if (isAllowed)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        private static bool CanAccessManagedAccount(AuthorizationHandlerContext context, UserDTO target)
        {
            var manageableRoles = UserAccountAccess.GetManageableRoles(context.User);

            return manageableRoles.Contains(target.RoleName);
        }

        private static bool CanUpdateAccount(AuthorizationHandlerContext context, UserDTO target)
        {
            var callerIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(callerIdClaim, out var callerId) || callerId <= 0)
            {
                return false;
            }

            bool canEditOwnAccount =
                context.User.IsInRole(RoleNames.Veterinarian) ||
                context.User.IsInRole(RoleNames.Admin) ||
                context.User.IsInRole(RoleNames.SysAdmin);

            if (target.Id == callerId && canEditOwnAccount)
            {
                return true;
            }

            return CanAccessManagedAccount(context, target);
        }
    }
}
