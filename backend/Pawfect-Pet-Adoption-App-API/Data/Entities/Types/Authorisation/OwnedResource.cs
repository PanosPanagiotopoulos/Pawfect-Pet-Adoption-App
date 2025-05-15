namespace Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation
{
    public class OwnedResource
    {
        private IEnumerable<string> _userIds;
        public OwnedFilterParams OwnedFilterParams { get; set; }

        public IEnumerable<string> UserIds
        {
            get => _userIds;
            set => _userIds = value?.OrderBy(id => id).ToList();
        }

        public OwnedResource(string userId) : this(new[] { userId }) { }

        public OwnedResource(IEnumerable<string> userIds)
        {
            UserIds = userIds;
        }

        public OwnedResource(string userId, OwnedFilterParams ownedFilterParams) : this(new[] { userId }, ownedFilterParams) { }

        public OwnedResource(IEnumerable<string> userIds, OwnedFilterParams ownedFilterParams)
        {
            UserIds = userIds;
            OwnedFilterParams = ownedFilterParams;
        }
    }

    public class OwnedFilterParams
    {
        public Models.Lookups.Lookup Lookup { get; set; }

        public OwnedFilterParams(Models.Lookups.Lookup lookup)
        {
            this.Lookup = lookup;
        }
    }
}