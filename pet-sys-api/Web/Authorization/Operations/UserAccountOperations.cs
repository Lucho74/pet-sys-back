using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Web.Authorization.Operations
{
    public static class UserAccountOperations
    {
        public static readonly OperationAuthorizationRequirement Read =
            new OperationAuthorizationRequirement
            {
                Name = nameof(Read)
            };

        public static readonly OperationAuthorizationRequirement Update =
            new OperationAuthorizationRequirement
            {
                Name = nameof(Update)
            };

        public static readonly OperationAuthorizationRequirement Suspend =
            new OperationAuthorizationRequirement
            {
                Name = nameof(Suspend)
            };

        public static readonly OperationAuthorizationRequirement Reactivate =
            new OperationAuthorizationRequirement
            {
                Name = nameof(Reactivate)
            };
    }
}
