using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Censors;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.Exceptions;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services.AnimalServices;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;
using Pawfect_Pet_Adoption_App_API.Services.Convention;
using Pawfect_Pet_Adoption_App_API.Services.UserServices;

namespace Pawfect_Pet_Adoption_App_API.Services.ShelterServices
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
        private readonly IAuthorisationService _authorisationService;
        private readonly IAuthorisationContentResolver _authorisationContentResolver;
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
			IAuthorisationService authorisationService,
			IAuthorisationContentResolver authorisationContentResolver

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
            _authorisationService = authorisationService;
            _authorisationContentResolver = authorisationContentResolver;
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
				OwnedResource ownedResource = _authorisationContentResolver.BuildOwnedResource(new ShelterLookup(), data.UserId);

                if (!await _authorisationService.AuthorizeOrOwnedAsync(ownedResource, Permission.EditShelters))
                    throw new ForbiddenException("Unauthorised access", typeof(Data.Entities.Shelter), Permission.EditShelters);


                _mapper.Map(persist, data);
			}
			else
			{
				_mapper.Map(persist, data);
				data.Id = null;
			}

			if (isUpdate) dataId = await _shelterRepository.UpdateAsync(data);
			else dataId = await _shelterRepository.AddAsync(data);

			if (String.IsNullOrEmpty(dataId))
				throw new InvalidOperationException("Failed to persist the shelter");

			Data.Entities.User? refUser = await _userRepository.FindAsync(x => x.Id == data.UserId);
			refUser.ShelterId = dataId;
			refUser.UpdatedAt = DateTime.UtcNow;

			if (String.IsNullOrEmpty(await _userRepository.UpdateAsync(refUser)))
				throw new InvalidOperationException("Failed to update the connected user of the shelter");

			// Return dto model
			ShelterLookup lookup = new ShelterLookup();
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = buildFields ?? new List<String> { "*", nameof(Models.User.User) + ".*" };
			lookup.Offset = 0;
			lookup.PageSize = 1;
            
			AuthContext context = _contextBuilder.OwnedFrom(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<ShelterCensor>().Censor([.. lookup.Fields], context);
            if (!censoredFields.Any()) throw new ForbiddenException("Unauthorised access when querying shelters");

            lookup.Fields = censoredFields;
            return (await _builderFactory.Builder<ShelterBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
					.Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), censoredFields))
					.FirstOrDefault();
        }

		public async Task Delete(String id) { await this.Delete(new List<String>() { id }); }

		public async Task Delete(List<String> ids)
		{
            ShelterLookup sLookup = new ShelterLookup();
            sLookup.Ids = ids;
            sLookup.Fields = new List<String> { nameof(Models.Shelter.Shelter.Id), nameof(Models.Shelter.Shelter.User) + "." + nameof(Models.User.User.Id) };
            sLookup.Offset = 1;
            sLookup.PageSize = 10000;

            List<Data.Entities.Shelter> shelters = await sLookup.EnrichLookup(_queryFactory).CollectAsync();
			OwnedResource ownedResource = _authorisationContentResolver.BuildOwnedResource(new ShelterLookup(), [.. shelters.Select(x => x.UserId)]);

            if (!await _authorisationService.AuthorizeOrOwnedAsync(ownedResource, Permission.DeleteShelters))
				throw new ForbiddenException("Unauthorised access", typeof(Data.Entities.Shelter), Permission.DeleteShelters);


            AnimalLookup aLookup = new AnimalLookup();
			aLookup.ShelterIds = ids;
			aLookup.Fields = new List<String> { nameof(Models.Animal.Animal.Id) };
			aLookup.Offset = 1;
			aLookup.PageSize = 10000;

			List<Data.Entities.Animal> animals = await aLookup.EnrichLookup(_queryFactory).CollectAsync();
			await _animalService.Value.Delete([.. animals?.Select(x => x.Id)]);

			await _shelterRepository.DeleteAsync(ids);

			UserLookup uLookup = new UserLookup();
			uLookup.ShelterIds = ids;
			uLookup.Fields = new List<String> { nameof(Models.User.User.Id) };
			uLookup.Offset = 1;
			uLookup.PageSize = 10000;

			List<Data.Entities.User> users = await uLookup.EnrichLookup(_queryFactory).CollectAsync();
			await _userService.Value.Delete([.. users?.Select(x => x.Id)]);
		}
	}
}