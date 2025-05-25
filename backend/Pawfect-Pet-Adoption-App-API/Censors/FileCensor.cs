using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;

namespace Pawfect_Pet_Adoption_App_API.Censors
{
    public class FileCensor : BaseCensor
    {
        private readonly IAuthorisationService _authorisationService;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;

        public FileCensor
        (
            IAuthorisationService authorisationService,
            ICensorFactory censorFactory,
            AuthContextBuilder contextBuilder
        )
        {
            _authorisationService = authorisationService;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
        }
        public override async Task<List<String>> Censor(List<String> fields, AuthContext context)
        {
            if (fields == null || fields.Count == 0) return new List<String>();
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (fields.Contains("*")) fields = this.ExtractForeign(fields, typeof(Data.Entities.File));

            List<String> censoredFields = new List<String>();
            if (await _authorisationService.AuthorizeOrOwnedAsync(context.OwnedResource, Permission.BrowseFiles))
            {
                censoredFields.AddRange(this.ExtractNonPrefixed(fields));
            }

            AuthContext userContext = _contextBuilder.OwnedFrom(new UserLookup(), context.CurrentUserId).AffiliatedWith(new UserLookup()).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<UserCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.File.File.Owner)), userContext), nameof(Models.File.File.Owner)));

            return censoredFields;
        }
    }
}
