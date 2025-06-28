using Main_API.Data.Entities.Types.Authorization;
using Main_API.Models.Lookups;
using Main_API.Services.AuthenticationServices;
using Microsoft.Extensions.Options;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using System.Collections.Generic;

namespace Main_API.Censors
{
    public class UserCensor : BaseCensor
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly UserFields _userFields;

        public UserCensor
        (
            IAuthorizationService AuthorizationService,
            ICensorFactory censorFactory,
            AuthContextBuilder contextBuilder,
            IOptions<UserFields> userFields
        )
        {
            _authorizationService = AuthorizationService;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _userFields = userFields.Value;
        }
        public override async Task<List<String>> Censor(List<String> fields, AuthContext context)
        {
            if (fields == null || fields.Count == 0) return new List<String>();
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (fields.Contains("*")) fields = this.ExtractForeign(fields, typeof(Data.Entities.User));
            
            List<String> censoredFields = new List<String>();

            if (context.OwnedResource != null)
                if (await _authorizationService.AuthorizeOwnedAsync(context.OwnedResource))
                    censoredFields.AddRange(this.ExtractNonPrefixed(fields, true));

            if (await _authorizationService.AuthorizeAsync(Permission.BrowseUsers))
            {
                censoredFields.AddRange(this.ExtractNonPrefixed(fields));
                censoredFields = [..censoredFields.Distinct()];
            }

            AuthContext shelterContext = _contextBuilder.OwnedFrom(new ShelterLookup(), context.CurrentUserId).AffiliatedWith(new ShelterLookup()).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<ShelterCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.User.User.Shelter)), shelterContext), nameof(Models.User.User.Shelter)));

            AuthContext adoptionApplicationContext = _contextBuilder.OwnedFrom(new AdoptionApplicationLookup(), context.CurrentUserId).AffiliatedWith(new AdoptionApplicationLookup()).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<AdoptionApplicationCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.User.User.RequestedAdoptionApplications)), adoptionApplicationContext), nameof(Models.User.User.RequestedAdoptionApplications)));

            // Create the file lookup for the profile photo censoring
            FileLookup fileLookup = new FileLookup();
            fileLookup.OwnerIds = [context.CurrentUserId];
            AuthContext fileContext = _contextBuilder.OwnedFrom(fileLookup).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<FileCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.User.User.ProfilePhoto)), fileContext), nameof(Models.User.User.ProfilePhoto)));

            return censoredFields;
        }

        public List<String> PseudoCensor(List<String> fields)
        {
            if (fields == null || fields.Count == 0) return new List<String>();

            List<String> nonPrefixed = [.. this.ExtractNonPrefixed(fields).Where(field => field.Equals(nameof(Models.User.User.Location)))];

            return nonPrefixed;
        }

        public List<String> ExtractNonPrefixed(List<String> fields, Boolean isOwner = false)
        {
            List<String> nonPrefixed = base.ExtractNonPrefixed(fields);
            List<String> allowedFields = isOwner ? _userFields.Owner : _userFields.External;

            return [.. nonPrefixed.Intersect(allowedFields)];
        }
            

    }
}
