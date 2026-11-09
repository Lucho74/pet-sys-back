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
        public static void Validate(string? role)
        {
            var availableRoles = new[]
            {
                RoleNames.Client,
                RoleNames.Veterinarian,
                RoleNames.Admin
            };

            if (string.IsNullOrWhiteSpace(role) || !availableRoles.Contains(role))
            {
                throw new ArgumentException("The requested role is invalid.");
            }
        }
    }
}
