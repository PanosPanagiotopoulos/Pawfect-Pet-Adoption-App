namespace Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation
{
    public class PermissionPolicyProviderConfig
    {
        public Policy[] Policies { get; set; }
    }

    public class Policy
    {
        public String Permission { get; set; }
        public String[] Roles { get; set; }
        public String[] AffiliatedRoles { get; set; }
    }
}