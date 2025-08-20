using Pawfect_API.Data.Entities.Types.Authorization;
using Pawfect_API.Models.Lookups;
using Pawfect_API.Services.AuthenticationServices;

namespace Pawfect_API.Censors
{
    public class FileCensor : BaseCensor
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;

        public FileCensor
        (
            IAuthorizationService AuthorizationService,
            ICensorFactory censorFactory,
            AuthContextBuilder contextBuilder
        )
        {
            _authorizationService = AuthorizationService;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
        }
        public override async Task<List<String>> Censor(List<String> fields, AuthContext context)
        {
            if (fields == null || fields.Count == 0) return new List<String>();
            if (context == null) throw new ArgumentNullException(nameof(context));

            List<String> censoredFields = new List<String>();
            if (await _authorizationService.AuthorizeOrOwnedAsync(context.OwnedResource, Permission.BrowseFiles))
            {
                censoredFields.AddRange(this.ExtractNonPrefixed(fields));
            }

            AuthContext userContext = _contextBuilder.OwnedFrom(new UserLookup(), context.CurrentUserId).AffiliatedWith(new UserLookup()).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<UserCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.File.File.Owner)), userContext), nameof(Models.File.File.Owner)));

            return censoredFields;
        }
    }
}
