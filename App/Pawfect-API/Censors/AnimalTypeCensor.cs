using Pawfect_API.Data.Entities.Types.Authorization;
using Pawfect_API.Services.AuthenticationServices;

namespace Pawfect_API.Censors
{
    public class AnimalTypeCensor: BaseCensor
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICensorFactory _censorFactory;

        public AnimalTypeCensor
        (
            IAuthorizationService AuthorizationService,
            ICensorFactory censorFactory
        )
        {
            _authorizationService = AuthorizationService;
            _censorFactory = censorFactory;
        }
        public override async Task<List<String>> Censor(List<String> fields, AuthContext context)
        {
            if (fields == null || fields.Count == 0) return new List<String>();
            if (context == null) throw new ArgumentNullException(nameof(context));

            List<String> censoredFields = new List<String>();
            if (await _authorizationService.AuthorizeAsync(Permission.BrowseAnimalTypes))
            {
                censoredFields.AddRange(this.ExtractNonPrefixed(fields));
            }

            return censoredFields;
        }
    }
}
