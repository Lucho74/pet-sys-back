namespace Web.Authorization
{
    public static class Policies
    {
        public const string UsersRead = "Users.Read";
        public const string UsersReadAccount = "Users.ReadAccount";
        public const string UsersCreate = "Users.Create";
        public const string UsersCreateWithRoles = "Users.CreateWithRoles";
        public const string UsersUpdate = "Users.Update";
        public const string UsersSuspend = "Users.Suspend";
        public const string UsersReactivate = "Users.Reactivate";
        public const string UsersAssignRoles = "Users.AssignRoles";

        public const string ClientProfilesRead = "ClientProfiles.Read";

        public const string PetsRead = "Pets.Read";
        public const string PetsReadOwn = "Pets.ReadOwn";
        public const string PetsCreate = "Pets.Create";
        public const string PetsUpdate = "Pets.Update";

        public const string ConsultationsRead = "Consultations.Read";
        public const string ConsultationsReadByPet = "Consultations.ReadByPet";
        public const string ConsultationsCreate = "Consultations.Create";
        public const string ConsultationsUpdate = "Consultations.Update";
    }
}
