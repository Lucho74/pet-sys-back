using Application.Models;
using Microsoft.AspNetCore.Authorization;

namespace Web.Authorization
{
    public class ReadUserAccountHandler : AuthorizationHandler<ReadUserAccountRequirement, UserDTO>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ReadUserAccountRequirement requirement, UserDTO target)
        {
            var readableRoles = UserAccountAccess.GetReadableRoles(context.User);

            if (target.Roles.Count > 0 && target.Roles.All(role => readableRoles.Contains(role)))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
