using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Web.Authorization.Requirements;

namespace Web.Authorization.Handlers
{
    public class CreateUserRolesHandler : AuthorizationHandler<CreateUserRolesRequirement, IReadOnlyCollection<string>>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CreateUserRolesRequirement requirement, IReadOnlyCollection<string> requestedRoles)
        {
            if (requestedRoles.Count == 0)
            {
                return Task.CompletedTask;
            }

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

            if (requestedRoles.All(role => allowedRoles.Contains(role)))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
