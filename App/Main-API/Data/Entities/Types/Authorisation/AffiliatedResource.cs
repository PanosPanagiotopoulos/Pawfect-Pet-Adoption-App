namespace Main_API.Data.Entities.Types.Authorization
{
    public class AffiliatedResource
    {
        public IEnumerable<String> AffiliatedRoles { get; set; }
        public AffiliatedFilterParams AffiliatedFilterParams { get; set; }
        
        public AffiliatedResource() {}

        public AffiliatedResource(AffiliatedFilterParams affiliatedFilterParams) 
        {
            this.AffiliatedFilterParams = affiliatedFilterParams;
        }

        public AffiliatedResource(IEnumerable<String> affiliatedRoles, AffiliatedFilterParams affiliatedFilterParams): this(affiliatedFilterParams)
        {
            AffiliatedRoles = affiliatedRoles;
        }
    }
    public class AffiliatedFilterParams
    {
        public Models.Lookups.Lookup RequestedFilters { get; set; }

        public AffiliatedFilterParams(Models.Lookups.Lookup lookup)
        {
            RequestedFilters = lookup;
        }
    }
  }
