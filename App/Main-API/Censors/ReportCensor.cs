using Main_API.Data.Entities.Types.Authorization;
using Main_API.Models.Lookups;
using Main_API.Services.AuthenticationServices;

namespace Main_API.Censors
{
    public class ReportCensor : BaseCensor
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;

        public ReportCensor
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
            if (await _authorizationService.AuthorizeOrOwnedOrAffiliated(context, Permission.BrowseReports))
            {
                censoredFields.AddRange(this.ExtractNonPrefixed(fields));
            }

            // Prepare the conversation lookup for the conversation censoring
            UserLookup userLookup = new UserLookup();
            userLookup.Ids = [];
            ReportLookup ownedReportLookup = (ReportLookup)context.OwnedResource?.OwnedFilterParams?.RequestedFilters;
            ReportLookup affiliatedReportLookup = (ReportLookup)context.AffiliatedResource?.AffiliatedFilterParams?.RequestedFilters;
            if (ownedReportLookup != null)
            {
                userLookup.Ids.AddRange(ownedReportLookup.ReporteredIds ?? []);
                userLookup.Ids.AddRange(ownedReportLookup.ReportedIds ?? []);
            }
            if (affiliatedReportLookup != null)
            {
                userLookup.Ids.AddRange(ownedReportLookup.ReporteredIds ?? []);
                userLookup.Ids.AddRange(ownedReportLookup.ReportedIds ?? []);
            }

            userLookup.Ids = [.. userLookup.Ids.Distinct()];

            AuthContext reportedContext = _contextBuilder.OwnedFrom(userLookup, userLookup.Ids).AffiliatedWith(userLookup).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<UserCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Report.Report.Reported)), reportedContext), nameof(Models.Report.Report.Reported)));

            AuthContext reporterContext = _contextBuilder.OwnedFrom(userLookup, userLookup.Ids).AffiliatedWith(userLookup).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<UserCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Report.Report.Reporter)), reporterContext), nameof(Models.Report.Report.Reporter)));

            return censoredFields;
        }
    }
}
