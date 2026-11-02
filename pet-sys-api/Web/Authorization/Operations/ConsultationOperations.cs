using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Web.Authorization.Operations
{
    public static class ConsultationOperations
    {
        public static readonly OperationAuthorizationRequirement Update =
            new OperationAuthorizationRequirement
            {
                Name = nameof(Update)
            };
    }
}
