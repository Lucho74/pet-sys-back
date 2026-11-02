using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Web.Authorization.Requirements;

namespace Web.Authorization.Handlers
{
    public class CreateUserRoleHandler : AuthorizationHandler<CreateUserRoleRequirement, string>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CreateUserRoleRequirement requirement, string requestedRole)
        {
            var allowedRoles = new HashSet<string>(StringComparer.Ordinal);

            if (context.User.IsInRole(RoleNames.Veterinarian))
            {
                allowedRoles.Add(RoleNames.Client);
            }

            if (context.User.IsInRole(RoleNames.Admin))
            {
                allowedRoles.Add(RoleNames.Client);
                allowedRoles.Add(RoleNames.Veterinarian);
            }

            if (context.User.IsInRole(RoleNames.SysAdmin))
            {
                allowedRoles.Add(RoleNames.Client);
                allowedRoles.Add(RoleNames.Veterinarian);
                allowedRoles.Add(RoleNames.Admin);
            }

            if (allowedRoles.Contains(requestedRole))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
