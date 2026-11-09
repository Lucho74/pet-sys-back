using Domain.Constants;
using Domain.Entities;

namespace Application.Validation
{
    public static class UserProfileRules
    {
        public static void ValidateCreation(string role, string? dni, string? registrationNumber)
        {
            bool isClient = role == RoleNames.Client;
            bool isProfessional = role == RoleNames.Veterinarian || role == RoleNames.Admin;

            if (isClient)
            {
                if (string.IsNullOrWhiteSpace(dni) || dni.Trim().Length > 8)
                {
                    throw new ArgumentException("A new client profile requires a Dni with at most 8 characters.");
                }
            }
            else if (dni != null)
            {
                throw new ArgumentException("Dni can only be provided when assigning Client.");
            }

            if (isProfessional)
            {
                if (string.IsNullOrWhiteSpace(registrationNumber) || registrationNumber.Trim().Length > 50)
                {
                    throw new ArgumentException("A new veterinarian profile requires a registration number with at most 50 characters.");
                }
            }
            else if (registrationNumber != null)
            {
                throw new ArgumentException("RegistrationNumber can only be provided for Veterinarian or Admin.");
            }
        }

        public static void ValidateRoleTransition(User user, string requestedRole)
        {
            bool currentIsClient = user.Role.Name == RoleNames.Client;
            bool requestedIsClient = requestedRole == RoleNames.Client;

            if (currentIsClient != requestedIsClient)
            {
                throw new ArgumentException("Client and staff accounts must remain separate. Create another account.");
            }

            if (requestedIsClient && user.ClientProfile == null)
            {
                throw new ArgumentException("The user does not have a valid client profile.");
            }

            bool isProfessional = requestedRole == RoleNames.Veterinarian || requestedRole == RoleNames.Admin;

            if (isProfessional && user.VeterinarianProfile == null)
            {
                throw new ArgumentException("The user does not have a valid veterinarian profile.");
            }
        }

        public static void ValidateRegistrationNumberUpdate(User user, string? registrationNumber)
        {
            if (registrationNumber == null)
            {
                return;
            }

            bool isProfessional = user.Role.Name == RoleNames.Veterinarian || user.Role.Name == RoleNames.Admin;

            if (!isProfessional || user.VeterinarianProfile == null)
            {
                throw new ArgumentException("The user does not have a valid veterinarian profile.");
            }

            if (string.IsNullOrWhiteSpace(registrationNumber) || registrationNumber.Trim().Length > 50)
            {
                throw new ArgumentException("Registration number must contain between 1 and 50 characters.");
            }
        }
    }
}
