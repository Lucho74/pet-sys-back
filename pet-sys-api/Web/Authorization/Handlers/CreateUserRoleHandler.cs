using Microsoft.AspNetCore.Authorization;
using Web.Authorization.Requirements;
using Web.Authorization.Rules;

namespace Web.Authorization.Handlers
{
    public class CreateUserRoleHandler : AuthorizationHandler<CreateUserRoleRequirement, string>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CreateUserRoleRequirement requirement, string requestedRole)
        {
            var allowedRoles = UserAccountAccess.GetManageableRoles(context.User);

            if (allowedRoles.Contains(requestedRole))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
