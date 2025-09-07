namespace Pawfect_Messenger.Data.Entities.Types.Authorisation
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