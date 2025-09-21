using Pawfect_API.Data.Entities.Types.Search;

namespace Pawfect_API.Services.EmbeddingServices
{
    public interface IEmbeddingValueExtractor<EmbeddingModel, EmbeddingValueExtractionType, Output>: IEmbeddingValueExtractorAbstraction where EmbeddingModel : ISearchDataModel<EmbeddingValueExtractionType> where Output : class
    {
        Task<Output> ExtractValue(EmbeddingModel input);   
    }
}
