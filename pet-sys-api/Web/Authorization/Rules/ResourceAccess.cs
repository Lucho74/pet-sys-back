using Application.Models;
using Domain.Constants;
using System.Security.Claims;

namespace Web.Authorization.Rules
{
    public static class ResourceAccess
    {
        public static string[] GetManageableRoles(ClaimsPrincipal user)
        {
            if (user.IsInRole(RoleNames.SysAdmin))
            {
                return new[] { RoleNames.Client, RoleNames.Veterinarian, RoleNames.Admin };
            }

            if (user.IsInRole(RoleNames.Admin))
            {
                return new[] { RoleNames.Client, RoleNames.Veterinarian };
            }

            if (user.IsInRole(RoleNames.Veterinarian))
            {
                return new[] { RoleNames.Client };
            }

            return Array.Empty<string>();
        }

        public static bool CanManageAccount(ClaimsPrincipal user, string roleName)
        {
            return user.Identity?.IsAuthenticated == true &&
                GetManageableRoles(user).Contains(roleName);
        }

        public static bool CanUpdateAccount(ClaimsPrincipal user, UserDTO target)
        {
            if (user.Identity?.IsAuthenticated != true || !IsStaff(user))
            {
                return false;
            }

            bool isOwnAccount = int.TryParse(
                user.FindFirstValue(ClaimTypes.NameIdentifier), out var callerId) &&
                callerId > 0 && target.Id == callerId;

            return isOwnAccount || CanManageAccount(user, target.RoleName);
        }

        public static bool CanUpdateRegistrationNumber(ClaimsPrincipal user)
        {
            return user.Identity?.IsAuthenticated == true &&
                (user.IsInRole(RoleNames.Admin) || user.IsInRole(RoleNames.SysAdmin));
        }

        public static bool CanReadPet(ClaimsPrincipal user, PetDTO pet)
        {
            if (user.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            if (IsStaff(user))
            {
                return true;
            }

            return user.IsInRole(RoleNames.Client) &&
                int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var callerId) &&
                callerId > 0 && pet.ClientId == callerId;
        }

        public static bool CanCreateConsultationFor(ClaimsPrincipal user, int veterinarianId)
        {
            if (user.Identity?.IsAuthenticated != true || veterinarianId <= 0)
            {
                return false;
            }

            if (user.IsInRole(RoleNames.SysAdmin) || user.IsInRole(RoleNames.Admin))
            {
                return true;
            }

            return user.IsInRole(RoleNames.Veterinarian) &&
                int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var callerId) &&
                callerId == veterinarianId;
        }

        private static bool IsStaff(ClaimsPrincipal user)
        {
            return user.IsInRole(RoleNames.Veterinarian) ||
                user.IsInRole(RoleNames.Admin) ||
                user.IsInRole(RoleNames.SysAdmin);
        }
    }
}
