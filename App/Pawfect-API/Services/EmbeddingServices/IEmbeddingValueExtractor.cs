using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Search;

namespace Pawfect_Pet_Adoption_App_API.Services.EmbeddingServices
{
    public interface IEmbeddingValueExtractor<EmbeddingModel, EmbeddingValueExtractionType, Output>: IEmbeddingValueExtractorAbstraction where EmbeddingModel : ISearchDataModel<EmbeddingValueExtractionType> where Output : class
    {
        Task<Output> ExtractValue(EmbeddingModel input);   
    }
}
