using Application.Exceptions;
using Domain.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validation
{
    public static class UserRoleInputValidation
    {
        public static void Validate(IReadOnlyCollection<string>? roles)
        {
            if (roles == null || roles.Count == 0)
            {
                throw new BadRequestException("At least one role is required.");
            }

            var errors = new List<string>();

            var availableRoles = new[]
            {
                RoleNames.Client,
                RoleNames.Veterinarian,
                RoleNames.Admin
            };

            if (roles.Any(role => !availableRoles.Contains(role)))
            {
                errors.Add("One or more roles are invalid.");
            }

            if (roles.Distinct().Count() != roles.Count)
            {
                errors.Add("Roles cannot be repeated.");
            }

            if (errors.Count > 0)
            {
                throw new BadRequestException(string.Join(" ", errors));
            }
        }
    }
}
