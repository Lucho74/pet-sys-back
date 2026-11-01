using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Web.Authorization.Operations;
using Web.Authorization.Rules;

namespace Web.Authorization.Handlers
{
    public class UserAccountAuthorizationHandler
        : AuthorizationHandler<OperationAuthorizationRequirement, UserDTO>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            UserDTO target)
        {
            if (requirement.Name == UserAccountOperations.Read.Name &&
                CanReadAccount(context, target))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        private static bool CanReadAccount(
            AuthorizationHandlerContext context,
            UserDTO target)
        {
            var readableRoles =
                UserAccountAccess.GetReadableRoles(context.User);

            return target.Roles.Count > 0 &&
                target.Roles.All(role => readableRoles.Contains(role));
        }
    }
}
