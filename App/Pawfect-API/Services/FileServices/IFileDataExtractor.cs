using Pawfect_API.Models.Animal;

namespace Pawfect_API.Services.FileServices
{
    public interface IFileDataExtractor
    {
        Task<List<AnimalPersist>> ExtractAnimalModelData(IFormFile modelsDataCsv);
        Task<Byte[]> GenerateAnimalImportTemplate();
    }
}
