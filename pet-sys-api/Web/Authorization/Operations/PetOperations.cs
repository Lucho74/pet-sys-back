using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Web.Authorization.Operations
{
    public static class PetOperations
    {
        public static readonly OperationAuthorizationRequirement Read =
            new OperationAuthorizationRequirement
            {
                Name = nameof(Read)
            };
    }
}
