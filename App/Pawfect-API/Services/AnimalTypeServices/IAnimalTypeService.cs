using Pawfect_API.Models.AnimalType;
using Pawfect_API.Models.Lookups;

namespace Pawfect_API.Services.AnimalTypeServices
{
	public interface IAnimalTypeService
	{
		Task<AnimalType?> Persist(AnimalTypePersist persist, List<String> fields);

		Task Delete(String id);
		Task Delete(List<String> ids);

	}
}