using AutoMapper;

using Pawfect_API.Builders;
using Pawfect_API.Censors;
using Pawfect_API.Data.Entities.Types.Authorization;
using Pawfect_API.Exceptions;
using Pawfect_API.Models.Lookups;
using Pawfect_API.Models.Shelter;
using Pawfect_API.Query;
using Pawfect_API.Repositories.Interfaces;
using Pawfect_API.Services.AnimalServices;
using Pawfect_API.Services.AuthenticationServices;
using Pawfect_API.Services.Convention;
using Pawfect_API.Services.UserServices;

namespace Pawfect_API.Services.ShelterServices
{
	public class ShelterService : IShelterService
	{
		private readonly IShelterRepository _shelterRepository;
		private readonly IMapper _mapper;
		private readonly IUserRepository _userRepository;
		private readonly IConventionService _conventionService;
		private readonly Lazy<IAnimalService> _animalService;
		private readonly Lazy<IUserService> _userService;
        private readonly IQueryFactory _queryFactory;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly IBuilderFactory _builderFactory;

        public ShelterService
		(
			IShelterRepository shelterRepository,
			IMapper mapper,
			IUserRepository userRepository,
			IConventionService conventionService,
			Lazy<IAnimalService> animalService,
			Lazy<IUserService> userService,
			IQueryFactory queryFactory,
            IBuilderFactory builderFactory,
			ICensorFactory censorFactory,
            AuthContextBuilder contextBuilder,
			IAuthorizationService AuthorizationService,
			IAuthorizationContentResolver AuthorizationContentResolver

        )
		{
			_shelterRepository = shelterRepository;
			_mapper = mapper;
			_userRepository = userRepository;
			_conventionService = conventionService;
			_animalService = animalService;
			_userService = userService;
            _queryFactory = queryFactory;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _authorizationService = AuthorizationService;
            _authorizationContentResolver = AuthorizationContentResolver;
            _builderFactory = builderFactory;
        }

		public async Task<Models.Shelter.Shelter> Persist(ShelterPersist persist, List<String> buildFields = null)
		{

			Boolean isUpdate = _conventionService.IsValidId(persist.Id);
            Data.Entities.Shelter data = new Data.Entities.Shelter();
			String dataId = String.Empty;
			if (isUpdate)
			{
				data = await _shelterRepository.FindAsync(x => x.Id == persist.Id);

				if (data == null) throw new NotFoundException("No shelter found with id given", persist.Id, typeof(Data.Entities.Shelter));

				if (!persist.UserId.Equals(data.UserId))
					throw new InvalidOperationException("Cannot change user id on shelter");

				// Check if the user is the owner of the shelter , so that he can update it
				OwnedResource ownedResource = _authorizationContentResolver.BuildOwnedResource(new ShelterLookup(), data.UserId);

                if (!await _authorizationService.AuthorizeOrOwnedAsync(ownedResource, Permission.EditShelters))
                    throw new ForbiddenException("Unauthorised access", typeof(Data.Entities.Shelter), Permission.EditShelters);


                _mapper.Map(persist, data);
			}
			else
			{
				_mapper.Map(persist, data);
				data.Id = null;
			}

			if (isUpdate) data.Id = await _shelterRepository.UpdateAsync(data);
			else data.Id = await _shelterRepository.AddAsync(data);

			if (String.IsNullOrEmpty(data.Id))
				throw new InvalidOperationException("Failed to persist the shelter");

			if (!isUpdate)
			{
                Data.Entities.User? refUser = await _userRepository.FindAsync(x => x.Id == data.UserId);
                refUser.ShelterId = dataId;
                refUser.UpdatedAt = DateTime.UtcNow;

                if (String.IsNullOrEmpty(await _userRepository.UpdateAsync(refUser)))
                    throw new InvalidOperationException("Failed to update the connected user of the shelter");
            }
			
            return (await _builderFactory.Builder<ShelterBuilder>()
					.Build([data], buildFields ?? new List<String> { "*", nameof(Models.User.User) + ".*" }))
					.FirstOrDefault();
        }

		public async Task Delete(String id) { await this.Delete(new List<String>() { id }); }

		public async Task Delete(List<String> ids)
		{
            ShelterLookup sLookup = new ShelterLookup();
            sLookup.Ids = ids;
            sLookup.Fields = new List<String> { nameof(Models.Shelter.Shelter.Id), nameof(Models.Shelter.Shelter.User) + "." + nameof(Models.User.User.Id) };
            sLookup.Offset = 0;
            sLookup.PageSize = 10000;

            List<Data.Entities.Shelter> shelters = await sLookup.EnrichLookup(_queryFactory).CollectAsync();
			OwnedResource ownedResource = _authorizationContentResolver.BuildOwnedResource(new ShelterLookup(), [.. shelters.Select(x => x.UserId)]);

            if (!await _authorizationService.AuthorizeOrOwnedAsync(ownedResource, Permission.DeleteShelters))
				throw new ForbiddenException("Unauthorised access", typeof(Data.Entities.Shelter), Permission.DeleteShelters);


            AnimalLookup aLookup = new AnimalLookup();
			aLookup.ShelterIds = ids;
			aLookup.Fields = new List<String> { nameof(Models.Animal.Animal.Id) };
			aLookup.Offset = 0;
			aLookup.PageSize = 10000;

			List<Data.Entities.Animal> animals = await aLookup.EnrichLookup(_queryFactory).CollectAsync();
			await _animalService.Value.Delete([.. animals?.Select(x => x.Id)]);

			await _shelterRepository.DeleteManyAsync(ids);

			await _userService.Value.Delete([.. shelters?.Select(x => x.UserId)]);
		}
	}
}