using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Repositories.Implementations;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services.AnimalServices;
using Pawfect_Pet_Adoption_App_API.Services.Convention;
using Pawfect_Pet_Adoption_App_API.Services.UserServices;

namespace Pawfect_Pet_Adoption_App_API.Services.ShelterServices
{
	public class ShelterService : IShelterService
	{
		private readonly ShelterQuery _shelterQuery;
		private readonly ShelterBuilder _shelterBuilder;
		private readonly IShelterRepository _shelterRepository;
		private readonly IMapper _mapper;
		private readonly IUserRepository _userRepository;
		private readonly IConventionService _conventionService;
		private readonly AnimalQuery _animalQuery;
		private readonly Lazy<IAnimalService> _animalService;
		private readonly UserQuery _userQuery;
		private readonly Lazy<IUserService> _userService;

		public ShelterService
		(
			ShelterQuery shelterQuery,
			ShelterBuilder shelterBuilder,
			IShelterRepository shelterRepository,
			IMapper mapper,
			IUserRepository userRepository,
			IConventionService conventionService,
			AnimalQuery animalQuery,
			Lazy<IAnimalService> animalService,
			UserQuery userQuery,
			Lazy<IUserService> userService
		)
		{
			_shelterQuery = shelterQuery;
			_shelterBuilder = shelterBuilder;
			_shelterRepository = shelterRepository;
			_mapper = mapper;
			_userRepository = userRepository;
			_conventionService = conventionService;
			_animalQuery = animalQuery;
			_animalService = animalService;
			_userQuery = userQuery;
			_userService = userService;
		}

		public async Task<IEnumerable<ShelterDto>> QuerySheltersAsync(ShelterLookup shelterLookup)
		{
			List<Shelter> queriedShelters = await shelterLookup.EnrichLookup(_shelterQuery).CollectAsync();
			return await _shelterBuilder.SetLookup(shelterLookup).BuildDto(queriedShelters, shelterLookup.Fields.ToList());
		}

		public async Task<ShelterDto?> Get(String id, List<String> fields)
		{
			//*TODO* Add authorization service with user roles and permissions

			ShelterLookup lookup = new ShelterLookup(_shelterQuery);
			lookup.Ids = new List<String> { id };
			lookup.Fields = fields;
			lookup.PageSize = 1;
			lookup.Offset = 0;

			List<Shelter> shelter = await lookup.EnrichLookup().CollectAsync();

			if (shelter == null)
			{
				throw new InvalidDataException("Δεν βρέθηκε shelter με αυτό το ID");
			}

			return (await _shelterBuilder.SetLookup(lookup).BuildDto(shelter, fields)).FirstOrDefault();
		}

		public async Task<ShelterDto?> Persist(ShelterPersist persist, List<String> buildFields = null)
		{

			Boolean isUpdate = _conventionService.IsValidId(persist.Id);
			Shelter data = new Shelter();
			String dataId = String.Empty;
			if (isUpdate)
			{
				data = await _shelterRepository.FindAsync(x => x.Id == persist.Id);

				if (data == null) throw new InvalidDataException("No entity found with id given");

				if (!persist.UserId.Equals(data.UserId))
				{
					throw new InvalidOperationException("Cannot change user id on shelter");
				}

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
			{
				throw new InvalidOperationException("Αποτυχία κατά το persist του καταφυγίου");
			}

			User? refUser = await _userRepository.FindAsync(x => x.Id == data.UserId);
			refUser.ShelterId = dataId;
			refUser.UpdatedAt = DateTime.UtcNow;

			if (String.IsNullOrEmpty(await _userRepository.UpdateAsync(refUser)))
			{
				throw new InvalidOperationException("Αποτυχία ενημέρωσης referenced χρήστη για καταφύγιο.");
			}

			// Return dto model
			ShelterLookup lookup = new ShelterLookup(_shelterQuery);
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = buildFields ?? new List<String> { "*", nameof(User) + ".*" };
			lookup.Offset = 0;
			lookup.PageSize = 1;

			return (
					 await _shelterBuilder.SetLookup(lookup)
					.BuildDto(await lookup.EnrichLookup().CollectAsync(), lookup.Fields.ToList())
					).FirstOrDefault();
		}

		public async Task Delete(String id) { await this.Delete(new List<String>() { id }); }

		public async Task Delete(List<String> ids)
		{
			// TODO : Authorization
			AnimalLookup aLookup = new AnimalLookup(_animalQuery);
			aLookup.ShelterIds = ids;
			aLookup.Fields = new List<String> { nameof(AnimalDto.Id) };
			aLookup.Offset = 0;
			aLookup.PageSize = 10000;

			List<Animal> animals = await aLookup.EnrichLookup().CollectAsync();
			await _animalService.Value.Delete(animals?.Select(x => x.Id).ToList());

			await _shelterRepository.DeleteAsync(ids);
			
			UserLookup uLookup = new UserLookup(_userQuery);
			uLookup.ShelterIds = ids;
			uLookup.Fields = new List<String> { nameof(UserDto.Id) };
			uLookup.Offset = 0;
			uLookup.PageSize = 100;

			List<User> users = await uLookup.EnrichLookup().CollectAsync();
			await _userService.Value.Delete(animals?.Select(x => x.Id).ToList());
		}
	}
}