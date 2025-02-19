using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query.Queries;

namespace Pawfect_Pet_Adoption_App_API.Services.AnimalServices
{
	public class AnimalService : IAnimalService
	{
		private readonly AnimalQuery _animalQuery;
		private readonly AnimalBuilder _animalBuilder;

		public AnimalService(AnimalQuery animalQuery, AnimalBuilder animalBuilder)
		{
			_animalQuery = animalQuery;
			_animalBuilder = animalBuilder;
		}

		public async Task<IEnumerable<AnimalDto>> QueryAnimalsAsync(AnimalLookup animalLookup)
		{
			List<Animal> queriedAnimals = await animalLookup.EnrichLookup(_animalQuery).CollectAsync();
			return await _animalBuilder.SetLookup(animalLookup).BuildDto(queriedAnimals, animalLookup.Fields.ToList());
		}
	}
}