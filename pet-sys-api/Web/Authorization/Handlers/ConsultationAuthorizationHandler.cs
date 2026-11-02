using Application.Models;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Security.Claims;
using Web.Authorization.Operations;

namespace Web.Authorization.Handlers
{
    public class ConsultationAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, ConsultationDTO>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, ConsultationDTO target)
        {
            bool isAllowed = false;

            if (requirement.Name == ConsultationOperations.Update.Name)
            {
                isAllowed = CanUpdateOwnConsultation(context, target);
            }
            else if (requirement.Name == ConsultationOperations.Suspend.Name ||
                    requirement.Name == ConsultationOperations.Reactivate.Name)
            {
                isAllowed = context.User.IsInRole(RoleNames.Admin) || CanUpdateOwnConsultation(context, target);
            }

            if (isAllowed)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        private static bool CanUpdateOwnConsultation( AuthorizationHandlerContext context, ConsultationDTO target)
        {
            bool isProfessional = context.User.IsInRole(RoleNames.Veterinarian) || context.User.IsInRole(RoleNames.Admin);

            if (!isProfessional)
            {
                return false;
            }

            var callerIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(callerIdClaim, out var callerId) || callerId <= 0)
            {
                return false;
            }

            return target.VeterinarianId == callerId;
        }
    }
}
