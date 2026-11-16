using Domain.Enums;
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
        public static UserRole Validate(UserRole? role)
        {
            if (role is not (
                UserRole.Client or
                UserRole.Veterinarian or
                UserRole.Admin))
            {
                throw new ArgumentException("The requested role is invalid.");
            }

            return role.Value;
        }
    }
}
