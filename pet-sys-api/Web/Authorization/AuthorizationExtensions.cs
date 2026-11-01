using Domain.Constants;
using Microsoft.AspNetCore.Authorization;

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
                        .AddRequirements(new ReadUserAccountRequirement()))

                .AddPolicy(Policies.UsersCreate, policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireRole(
                            RoleNames.Veterinarian,
                            RoleNames.Admin,
                            RoleNames.SysAdmin))

                .AddPolicy(Policies.UsersCreateWithRoles, policy =>
                    policy.RequireAuthenticatedUser()
                        .AddRequirements(new CreateUserRolesRequirement()))

                .AddPolicy(Policies.UsersUpdate, policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireRole(
                            RoleNames.Veterinarian,
                            RoleNames.Admin,
                            RoleNames.SysAdmin))

                .AddPolicy(Policies.UsersSuspend, policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireRole(
                            RoleNames.Veterinarian,
                            RoleNames.Admin,
                            RoleNames.SysAdmin))

                .AddPolicy(Policies.UsersReactivate, policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireRole(
                            RoleNames.Veterinarian,
                            RoleNames.Admin,
                            RoleNames.SysAdmin))

                .AddPolicy(Policies.UsersAssignRoles, policy =>
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
                        .RequireRole(RoleNames.Veterinarian))

                .AddPolicy(Policies.ConsultationsUpdate, policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireRole(RoleNames.Veterinarian));

            services.AddSingleton<IAuthorizationHandler, CreateUserRolesHandler>();
            services.AddSingleton<IAuthorizationHandler, ReadUserAccountHandler>();

            return services;
        }
    }
}
