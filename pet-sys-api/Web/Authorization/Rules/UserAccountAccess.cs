using Domain.Constants;
using System.Security.Claims;

namespace Web.Authorization.Rules
{
    public static class UserAccountAccess
    {
        public static string[] GetManageableRoles(ClaimsPrincipal user)
        {
            var roles = new HashSet<string>(StringComparer.Ordinal);

            if (user.IsInRole(RoleNames.Veterinarian))
            {
                roles.Add(RoleNames.Client);
            }

            if (user.IsInRole(RoleNames.Admin))
            {
                roles.Add(RoleNames.Client);
                roles.Add(RoleNames.Veterinarian);
            }

            if (user.IsInRole(RoleNames.SysAdmin))
            {
                roles.Add(RoleNames.Client);
                roles.Add(RoleNames.Veterinarian);
                roles.Add(RoleNames.Admin);
            }

            return roles.ToArray();
        }
    }
}
