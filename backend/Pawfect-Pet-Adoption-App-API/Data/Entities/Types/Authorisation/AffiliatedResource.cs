namespace Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation
{
    public class AffiliatedResource
    {
        public IEnumerable<String> AffiliatedRoles { get; set; }
        public AffiliatedFilterParams AffiliatedFilterParams { get; set; }
        
        public String AffiliatedId { get; set; }

        public AffiliatedResource() {}

        public AffiliatedResource(AffiliatedFilterParams affiliatedFilterParams) 
        {
            this.AffiliatedFilterParams = affiliatedFilterParams;
        }
        public AffiliatedResource(AffiliatedFilterParams affiliatedFilterParams, String affiliatedId) : this(affiliatedFilterParams)
        {
            this.AffiliatedId = affiliatedId;
        }

        public AffiliatedResource(IEnumerable<String> affiliatedRoles, AffiliatedFilterParams affiliatedFilterParams): this(affiliatedFilterParams)
        {
            AffiliatedRoles = affiliatedRoles;
        }

        public AffiliatedResource(IEnumerable<String> affiliatedRoles, AffiliatedFilterParams affiliatedFilterParams, String affiliatedId) : this(affiliatedRoles, affiliatedFilterParams)
        {
            this.AffiliatedId = affiliatedId;
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
