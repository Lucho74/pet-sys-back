using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Web.Authorization.Handlers;
using Web.Authorization.Operations;
using Web.Authorization.Requirements;

namespace Web.Authorization
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
        {
            var authenticatedPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            services.AddAuthorizationBuilder()
                .SetDefaultPolicy(authenticatedPolicy)
                .SetFallbackPolicy(authenticatedPolicy)

                .AddPolicy(Policies.UsersRead, policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireRole(
                            RoleNames.Veterinarian,
                            RoleNames.Admin,
                            RoleNames.SysAdmin))

                .AddPolicy(Policies.UsersReadAccount, policy =>
                    policy.RequireAuthenticatedUser()
                        .AddRequirements(UserAccountOperations.Read))

                .AddPolicy(Policies.UsersCreate, policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireRole(
                            RoleNames.Veterinarian,
                            RoleNames.Admin,
                            RoleNames.SysAdmin))

                .AddPolicy(Policies.UsersCreateWithRole, policy =>
                    policy.RequireAuthenticatedUser()
                        .AddRequirements(new CreateUserRoleRequirement()))

                .AddPolicy(Policies.UsersUpdate, policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireRole(
                            RoleNames.Veterinarian,
                            RoleNames.Admin,
                            RoleNames.SysAdmin))

                .AddPolicy(Policies.UsersUpdateAccount, policy =>
                    policy.RequireAuthenticatedUser()
                        .AddRequirements(UserAccountOperations.Update))

                .AddPolicy(Policies.UsersSuspend, policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireRole(
                            RoleNames.Veterinarian,
                            RoleNames.Admin,
                            RoleNames.SysAdmin))

                .AddPolicy(Policies.UsersSuspendAccount, policy =>
                    policy.RequireAuthenticatedUser()
                        .AddRequirements(UserAccountOperations.Suspend))

                .AddPolicy(Policies.UsersReactivate, policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireRole(
                            RoleNames.Veterinarian,
                            RoleNames.Admin,
                            RoleNames.SysAdmin))

                .AddPolicy(Policies.UsersReactivateAccount, policy =>
                    policy.RequireAuthenticatedUser()
                        .AddRequirements(UserAccountOperations.Reactivate))

                .AddPolicy(Policies.UsersAssignRole, policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireRole(RoleNames.SysAdmin))

                .AddPolicy(Policies.PetsRead, policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireRole(
                            RoleNames.Veterinarian,
                            RoleNames.Admin))

                .AddPolicy(Policies.PetsReadOwn, policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireRole(RoleNames.Client))

                .AddPolicy(Policies.PetsCreate, policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireRole(
                            RoleNames.Veterinarian,
                            RoleNames.Admin))

                .AddPolicy(Policies.PetsUpdate, policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireRole(
                            RoleNames.Veterinarian,
                            RoleNames.Admin))

                .AddPolicy(Policies.PetsSuspend, policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireRole(
                            RoleNames.Veterinarian,
                            RoleNames.Admin))

                .AddPolicy(Policies.PetsReactivate, policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireRole(
                            RoleNames.Veterinarian,
                            RoleNames.Admin))

                .AddPolicy(Policies.ConsultationsRead, policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireRole(
                            RoleNames.Veterinarian,
                            RoleNames.Admin))

                .AddPolicy(Policies.ConsultationsReadByPet, policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireRole(
                            RoleNames.Client,
                            RoleNames.Veterinarian,
                            RoleNames.Admin))

                .AddPolicy(Policies.ConsultationsCreate, policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireRole(
                            RoleNames.Veterinarian,
                            RoleNames.Admin))

                .AddPolicy(Policies.ConsultationsUpdate, policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireRole(
                        RoleNames.Veterinarian,
                        RoleNames.Admin))

                .AddPolicy(Policies.ConsultationsUpdateOwn, policy =>
                    policy.RequireAuthenticatedUser()
                        .AddRequirements(ConsultationOperations.Update));

            services.AddSingleton<IAuthorizationHandler, CreateUserRoleHandler>();
            services.AddSingleton<IAuthorizationHandler, UserAccountAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, ConsultationAuthorizationHandler>();

            return services;
        }
    }
}
