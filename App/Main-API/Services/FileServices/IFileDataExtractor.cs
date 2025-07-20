using Main_API.Models.Animal;

namespace Pawfect_Pet_Adoption_App_API.Services.FileServices
{
    public interface IFileDataExtractor
    {
        Task<List<AnimalPersist>> ExtractAnimalModelData(IFormFile modelsDataCsv);
        Task<Byte[]> GenerateAnimalImportTemplate();
    }
}
