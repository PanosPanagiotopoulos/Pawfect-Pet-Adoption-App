namespace Pawfect_Notifications.Data.Entities.Types.Authorization
{
    public class OwnedResource
    {
        private IEnumerable<String> _userIds;
        public OwnedFilterParams OwnedFilterParams { get; set; }

        public IEnumerable<String> UserIds
        {
            get => _userIds;
            set => _userIds = value?.OrderBy(id => id).ToList();
        }

        public OwnedResource() { }

        public OwnedResource(String userId, OwnedFilterParams ownedFilterParams) : this(new[] { userId }, ownedFilterParams) { }

        public OwnedResource(IEnumerable<String> userIds, OwnedFilterParams ownedFilterParams)
        {
            UserIds = userIds;
            OwnedFilterParams = ownedFilterParams;
        }

        public OwnedResource(OwnedFilterParams ownedFilterParams): this([], ownedFilterParams) { }
    }

    public class OwnedFilterParams
    {
        public Models.Lookups.Lookup RequestedFilters { get; set; }

        public OwnedFilterParams(Models.Lookups.Lookup lookup)
        {
            this.RequestedFilters = lookup;
        }
    }
}