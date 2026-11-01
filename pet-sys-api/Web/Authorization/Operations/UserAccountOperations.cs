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
    }
}
