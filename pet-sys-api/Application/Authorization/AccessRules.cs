using Application.Models;
using Domain.Enums;
using System;
using System.Linq;

namespace Application.Authorization
{
    public static class AccessRules
    {
        public static bool IsStaff(UserRole callerRole)
        {
            return callerRole == UserRole.SysAdmin ||
                   callerRole == UserRole.Admin ||
                   callerRole == UserRole.Veterinarian;
        }

        public static UserRole[] GetManageableRoles(UserRole callerRole)
        {
            return callerRole switch
            {
                UserRole.SysAdmin => new[]
                {
                    UserRole.Admin,
                    UserRole.Veterinarian,
                    UserRole.Client
                },

                UserRole.Admin => new[]
                {
                    UserRole.Veterinarian,
                    UserRole.Client
                },

                UserRole.Veterinarian => new[]
                {
                    UserRole.Client
                },

                _ => Array.Empty<UserRole>()
            };
        }

        public static bool CanManageAccount(UserRole callerRole, UserRole targetRole)
        {
            return GetManageableRoles(callerRole).Contains(targetRole);
        }

        public static bool CanUpdateAccount(int callerId, UserRole callerRole, int targetId, UserRole targetRole)
        {
            if (callerId <= 0 || targetId <= 0 || !IsStaff(callerRole))
            {
                return false;
            }

            bool isOwnAccount = callerId == targetId;

            return isOwnAccount || CanManageAccount(callerRole, targetRole);
        }

        public static bool CanUpdateRegistrationNumber(UserRole callerRole)
        {
            return callerRole == UserRole.Admin || callerRole == UserRole.SysAdmin;
        }

        public static bool CanChangeUserRole(UserRole callerRole)
        {
            return callerRole == UserRole.SysAdmin;
        }

        public static bool CanReadPet(int callerId, UserRole callerRole, int clientId)
        {
            if (callerId <= 0)
            {
                return false;
            }

            if (IsStaff(callerRole))
            {
                return true;
            }

            return callerRole == UserRole.Client && callerId == clientId;
        }

        public static bool CanCreateConsultationFor(int callerId, UserRole callerRole, int veterinarianId)
        {
            if (callerId <= 0 || veterinarianId <= 0)
            {
                return false;
            }

            if (callerRole == UserRole.SysAdmin || callerRole == UserRole.Admin)
            {
                return true;
            }

            return callerRole == UserRole.Veterinarian && callerId == veterinarianId;
        }

        public static bool CanChangeConsultationProfessional(UserRole callerRole)
        {
            return callerRole == UserRole.SysAdmin || callerRole == UserRole.Admin;
        }
    }
}
