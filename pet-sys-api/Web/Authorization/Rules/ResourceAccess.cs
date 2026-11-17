using Application.Models;
using Domain.Enums;
using System.Security.Claims;

namespace Web.Authorization.Rules
{
    public static class ResourceAccess
    {
        public static UserRole[] GetManageableRoles(ClaimsPrincipal user)
        {
            if (user.IsInRole(nameof(UserRole.SysAdmin)))
            {
                return new[] { UserRole.Client, UserRole.Veterinarian, UserRole.Admin };
            }

            if (user.IsInRole(nameof(UserRole.Admin)))
            {
                return new[] { UserRole.Client, UserRole.Veterinarian };
            }

            if (user.IsInRole(nameof(UserRole.Veterinarian)))
            {
                return new[] { UserRole.Client };
            }

            return Array.Empty<UserRole>();
        }

        public static bool CanManageAccount(ClaimsPrincipal user, UserRole role)
        {
            return user.Identity?.IsAuthenticated == true &&
                GetManageableRoles(user).Contains(role);
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
                (user.IsInRole(nameof(UserRole.Admin)) || user.IsInRole(nameof(UserRole.SysAdmin)));
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

            return user.IsInRole(nameof(UserRole.Client)) &&
                int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var callerId) &&
                callerId > 0 && pet.ClientId == callerId;
        }

        public static bool CanCreateConsultationFor(ClaimsPrincipal user, int veterinarianId)
        {
            if (user.Identity?.IsAuthenticated != true || veterinarianId <= 0)
            {
                return false;
            }

            if (user.IsInRole(nameof(UserRole.SysAdmin)) || user.IsInRole(nameof(UserRole.Admin)))
            {
                return true;
            }

            return user.IsInRole(nameof(UserRole.Veterinarian)) &&
                int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var callerId) &&
                callerId == veterinarianId;
        }

        private static bool IsStaff(ClaimsPrincipal user)
        {
            return user.IsInRole(nameof(UserRole.Veterinarian)) ||
                user.IsInRole(nameof(UserRole.Admin)) ||
                user.IsInRole(nameof(UserRole.SysAdmin));
        }
    }
}
