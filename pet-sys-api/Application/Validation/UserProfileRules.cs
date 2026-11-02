using Application.Exceptions;
using Domain.Constants;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validation
{
    public static class UserProfileRules
    {
        public static void Validate(User? existingUser, string role, string? dni, string? registrationNumber)
        {
            bool isClient = role == RoleNames.Client;
            bool isVeterinarian = role == RoleNames.Veterinarian || role == RoleNames.Admin;

            var clientProfile = existingUser?.ClientProfile;
            var veterinarianProfile = existingUser?.VeterinarianProfile;

            if (isClient && clientProfile == null)
            {
                if (string.IsNullOrWhiteSpace(dni) ||dni.Trim().Length > 8)
                {
                    throw new BadRequestException("A new client profile requires a Dni with at most 8 characters.");
                }
            }

            if (dni != null)
            {
                if (!isClient)
                {
                    throw new BadRequestException("Dni can only be provided when assigning Client.");
                }

                if (clientProfile != null &&
                    dni.Trim() != clientProfile.Dni)
                {
                    throw new BadRequestException("Use the client profile operation to change Dni.");
                }
            }

            if (isVeterinarian && veterinarianProfile == null)
            {
                if (string.IsNullOrWhiteSpace(registrationNumber) ||
                    registrationNumber.Trim().Length > 50)
                {
                    throw new BadRequestException("A new veterinarian profile requires a registration number with at most 50 characters.");
                }
            }

            if (registrationNumber != null)
            {
                if (!isVeterinarian)
                {
                    throw new BadRequestException("RegistrationNumber can only be provided for Veterinarian or Admin.");
                }

                if (veterinarianProfile != null && registrationNumber.Trim() != veterinarianProfile.RegistrationNumber)
                {
                    throw new BadRequestException("Use the veterinarian profile operation to change the registration number.");
                }
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
                throw new BadRequestException("The user does not have a valid veterinarian profile.");
            }

            if (string.IsNullOrWhiteSpace(registrationNumber) || registrationNumber.Trim().Length > 50)
            {
                throw new BadRequestException("Registration number must contain between 1 and 50 characters.");
            }
        }
    }
}
