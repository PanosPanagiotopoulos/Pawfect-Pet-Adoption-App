namespace Main_API.Data.Entities.Types.Authorization
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