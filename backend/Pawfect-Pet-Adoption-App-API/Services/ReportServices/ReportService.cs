﻿using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Censors;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.Exceptions;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Report;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Repositories.Implementations;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;
using Pawfect_Pet_Adoption_App_API.Services.Convention;

namespace Pawfect_Pet_Adoption_App_API.Services.ReportServices
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
        private readonly IAuthorisationService _authorisationService;

        public ReportService
			(
				IQueryFactory queryFactory,
                IBuilderFactory builderFactory,
                IReportRepository reportRepository,
				IMapper mapper,
				IConventionService conventionService,
                ICensorFactory censorFactory,
                AuthContextBuilder contextBuilder,
				IAuthorisationService authorisationService
            )
		{
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
            _reportRepository = reportRepository;
			_mapper = mapper;
			_conventionService = conventionService;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _authorisationService = authorisationService;
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

                if (!await _authorisationService.AuthorizeAsync(Permission.EditReports))
                    throw new ForbiddenException("Unauthorised access editing reports", typeof(Data.Entities.Report), Permission.EditReports);

                _mapper.Map(persist, data);
				data.UpdatedAt = DateTime.UtcNow;
			}
			else
			{
                if (!await _authorisationService.AuthorizeAsync(Permission.CreateReports))
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
            if (!await _authorisationService.AuthorizeAsync(Permission.DeleteReports))
                throw new ForbiddenException("Unauthorised access deleting reports", typeof(Data.Entities.Report), Permission.DeleteReports);

            await _reportRepository.DeleteAsync(ids);
		}
	}
}