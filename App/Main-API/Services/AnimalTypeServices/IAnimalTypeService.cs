using Main_API.Models.AnimalType;
using Main_API.Models.Lookups;

namespace Main_API.Services.AnimalTypeServices
{
	public interface IAnimalTypeService
	{
		Task<AnimalType?> Persist(AnimalTypePersist persist, List<String> fields);

		Task Delete(String id);
		Task Delete(List<String> ids);

	}
}