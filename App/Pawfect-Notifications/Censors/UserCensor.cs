using Pawfect_Notifications.Data.Entities.Types.Authorization;
using Pawfect_Notifications.Services.AuthenticationServices;

namespace Pawfect_Notifications.Censors
{
    public class UserCensor : BaseCensor
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;

        public UserCensor
        (
            IAuthorizationService authorizationService,
            ICensorFactory censorFactory,
            AuthContextBuilder contextBuilder,
            IAuthorizationContentResolver authorizationContentResolver
        )
        {
            _authorizationService = authorizationService;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _authorizationContentResolver = authorizationContentResolver;
        }
        public override async Task<List<String>> Censor(List<String> fields, AuthContext context)
        {
            if (fields == null || fields.Count == 0) return new List<String>();
            if (context == null) throw new ArgumentNullException(nameof(context));

            List<String> censoredFields = new List<String>();

            if (await _authorizationService.AuthorizeAsync(Permission.BrowseUsers))
            {
                censoredFields.AddRange(this.ExtractNonPrefixed(fields));
                censoredFields = [..censoredFields.Distinct()];
            }
            return censoredFields;
        }
    }
}
