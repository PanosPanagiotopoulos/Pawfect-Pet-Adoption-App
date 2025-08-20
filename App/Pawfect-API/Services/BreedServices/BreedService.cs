using AutoMapper;
using Pawfect_API.Builders;
using Pawfect_API.Censors;
using Pawfect_API.Data.Entities.Types.Authorization;
using Pawfect_API.Exceptions;
using Pawfect_API.Models.Breed;
using Pawfect_API.Models.Lookups;
using Pawfect_API.Query;
using Pawfect_API.Repositories.Interfaces;
using Pawfect_API.Services.AuthenticationServices;
using Pawfect_API.Services.Convention;

namespace Pawfect_API.Services.BreedServices
{
	public class BreedService : IBreedService
	{
		private readonly IBreedRepository _breedRepository;
		private readonly IMapper _mapper;
		private readonly IConventionService _conventionService;
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        private readonly IAuthorizationService _authorizationService;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly ICensorFactory _censorFactory;

        public BreedService
		(
			IBreedRepository breedRepository,
			IMapper mapper,
			IConventionService conventionService,
			IQueryFactory queryFactory,
            IBuilderFactory builderFactory,
			IAuthorizationService AuthorizationService,
			AuthContextBuilder contextBuilder,
			ICensorFactory censorFactory
        )
		{
			_breedRepository = breedRepository;
			_mapper = mapper;
			_conventionService = conventionService;
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
            _authorizationService = AuthorizationService;
            _contextBuilder = contextBuilder;
            _censorFactory = censorFactory;
        }

		public async Task<Models.Breed.Breed?> Persist(BreedPersist persist, List<String> fields)
		{
			if (!await _authorizationService.AuthorizeAsync(Permission.EditBreeds))
				throw new ForbiddenException("You are not authorized to edit breeds", typeof(Data.Entities.Breed), Permission.EditBreeds);

            Boolean isUpdate = _conventionService.IsValidId(persist.Id);
            Data.Entities.Breed data = new Data.Entities.Breed();
			String dataId = String.Empty;
			if (isUpdate)
			{
				data = await _breedRepository.FindAsync(x => x.Id == persist.Id);

				if (data == null) throw new NotFoundException("No breed found with this id to persist", persist.Id, typeof(Data.Entities.Breed));

				_mapper.Map(persist, data);
				data.UpdatedAt = DateTime.UtcNow;
			}
			else
			{
				_mapper.Map(persist, data);
				data.Id = null; // Ensure new ID is generated
				data.CreatedAt = DateTime.UtcNow;
				data.UpdatedAt = DateTime.UtcNow;
			}

			if (isUpdate) dataId = await _breedRepository.UpdateAsync(data);
			else dataId = await _breedRepository.AddAsync(data);

			if (String.IsNullOrEmpty(dataId))
				throw new InvalidOperationException("Failed to persist breed");

			// Return dto model
			BreedLookup lookup = new BreedLookup();
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = fields;
			lookup.Offset = 0;
			lookup.PageSize = 1;

            AuthContext context = _contextBuilder.OwnedFrom(lookup).AffiliatedWith(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<BreedCensor>().Censor([.. lookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying breeds");

            lookup.Fields = censoredFields;
            return (await _builderFactory.Builder<BreedBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
					.Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), censoredFields))
					.FirstOrDefault();
        }

		public async Task Delete(String id) => await this.Delete(new List<String>() { id });

		public async Task Delete(List<String> ids)
		{
            if (!await _authorizationService.AuthorizeAsync(Permission.DeleteBreeds))
                throw new ForbiddenException("You are not authorized to delete breeds", typeof(Data.Entities.Breed), Permission.DeleteBreeds);

            await _breedRepository.DeleteManyAsync(ids);
		}
	}
}