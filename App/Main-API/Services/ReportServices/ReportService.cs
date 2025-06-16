using AutoMapper;

using Main_API.Builders;
using Main_API.Censors;
using Main_API.Data.Entities;
using Main_API.Data.Entities.Types.Authorization;
using Main_API.Exceptions;
using Main_API.Models.AdoptionApplication;
using Main_API.Models.Lookups;
using Main_API.Models.Report;
using Main_API.Query;
using Main_API.Query.Queries;
using Main_API.Repositories.Implementations;
using Main_API.Repositories.Interfaces;
using Main_API.Services.AuthenticationServices;
using Main_API.Services.Convention;

namespace Main_API.Services.ReportServices
{
	public class ReportService : IReportService
	{
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        private readonly IReportRepository _reportRepository;
		private readonly IMapper _mapper;
		private readonly IConventionService _conventionService;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly IAuthorizationService _authorizationService;

        public ReportService
			(
				IQueryFactory queryFactory,
                IBuilderFactory builderFactory,
                IReportRepository reportRepository,
				IMapper mapper,
				IConventionService conventionService,
                ICensorFactory censorFactory,
                AuthContextBuilder contextBuilder,
				IAuthorizationService AuthorizationService
            )
		{
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
            _reportRepository = reportRepository;
			_mapper = mapper;
			_conventionService = conventionService;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _authorizationService = AuthorizationService;
        }

		public async Task<Models.Report.Report> Persist(ReportPersist persist, List<String> fields)
		{
			Boolean isUpdate = _conventionService.IsValidId(persist.Id);
            Data.Entities.Report data = new Data.Entities.Report();
			String dataId = String.Empty;
			if (isUpdate)
			{
				data = await _reportRepository.FindAsync(x => x.Id == persist.Id);

				if (data == null) throw new NotFoundException("No report found with id given", persist.Id, typeof(Data.Entities.Report));

                if (!await _authorizationService.AuthorizeAsync(Permission.EditReports))
                    throw new ForbiddenException("Unauthorised access editing reports", typeof(Data.Entities.Report), Permission.EditReports);

                _mapper.Map(persist, data);
				data.UpdatedAt = DateTime.UtcNow;
			}
			else
			{
                if (!await _authorizationService.AuthorizeAsync(Permission.CreateReports))
                    throw new ForbiddenException("Unauthorised access editing reports", typeof(Data.Entities.Report), Permission.CreateReports);

                _mapper.Map(persist, data);
				data.Id = null; // Ensure new ID is generated
				data.CreatedAt = DateTime.UtcNow;
				data.UpdatedAt = DateTime.UtcNow;
			}

			if (isUpdate) dataId = await _reportRepository.UpdateAsync(data);
			else dataId = await _reportRepository.AddAsync(data);

			if (String.IsNullOrEmpty(dataId))
				throw new InvalidOperationException("Failed to persist report");

			// Return dto model
			ReportLookup lookup = new ReportLookup();
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = fields;
			lookup.Offset = 0;
			lookup.PageSize = 1;

            AuthContext context = _contextBuilder.OwnedFrom(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<ReportCensor>().Censor([.. lookup.Fields], context);
            if (!censoredFields.Any()) throw new ForbiddenException("Unauthorised access when querying reports");

            lookup.Fields = censoredFields;
            return (await _builderFactory.Builder<ReportBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
					.Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), censoredFields))
					.FirstOrDefault();
        }

		public async Task Delete(String id) { await this.Delete(new List<String>() { id }); }

		public async Task Delete(List<String> ids)
		{
            if (!await _authorizationService.AuthorizeAsync(Permission.DeleteReports))
                throw new ForbiddenException("Unauthorised access deleting reports", typeof(Data.Entities.Report), Permission.DeleteReports);

            await _reportRepository.DeleteAsync(ids);
		}
	}
}