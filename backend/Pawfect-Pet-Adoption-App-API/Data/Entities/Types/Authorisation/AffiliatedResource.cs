namespace Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation
{
    public class AffiliatedResource
    {
        public IEnumerable<String> UserIds { get; set; }
        public IEnumerable<String> AffiliatedRoles { get; set; }
        public AffiliatedFilterParams AffiliatedFilterParams { get; set; }

        public AffiliatedResource(String userId) : this(new[] { userId }) { }

        public AffiliatedResource() {}
        public AffiliatedResource(IEnumerable<String> userIds)
        {
            UserIds = userIds;
        }

        public AffiliatedResource(String userId, IEnumerable<String> affiliatedRoles) : this(new[] { userId }, affiliatedRoles) { }

        public AffiliatedResource(IEnumerable<String> userIds, IEnumerable<String> affiliatedRoles)
        {
            UserIds = userIds;
            AffiliatedRoles = affiliatedRoles;
        }

        public AffiliatedResource(String userId, AffiliatedFilterParams affiliatedFilterParams) : this(new[] { userId }, affiliatedFilterParams) { }

        public AffiliatedResource(IEnumerable<String> userIds, AffiliatedFilterParams affiliatedFilterParams)
        {
            UserIds = userIds;
            AffiliatedFilterParams = affiliatedFilterParams;
        }

        public AffiliatedResource(String userId, IEnumerable<String> affiliatedRoles, AffiliatedFilterParams affiliatedFilterParams) : this(new[] { userId }, affiliatedRoles, affiliatedFilterParams) { }

        public AffiliatedResource(IEnumerable<String> userIds, IEnumerable<String> affiliatedRoles, AffiliatedFilterParams affiliatedFilterParams)
        {
            UserIds = userIds;
            AffiliatedRoles = affiliatedRoles;
            AffiliatedFilterParams = affiliatedFilterParams;
        }

        public AffiliatedResource(AffiliatedFilterParams affiliatedFilterParams)
        {
            AffiliatedFilterParams = affiliatedFilterParams;
        }

    }
    public class AffiliatedFilterParams
    {
        public Models.Lookups.Lookup Lookup { get; set; }

        public AffiliatedFilterParams(Models.Lookups.Lookup lookup)
        {
            Lookup = lookup;
        }
    }
  }
