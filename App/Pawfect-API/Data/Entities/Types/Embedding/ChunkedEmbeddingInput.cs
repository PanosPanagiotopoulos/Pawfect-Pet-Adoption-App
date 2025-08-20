namespace Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Embedding
{
    public class ChunkedEmbeddingInput<TInput> where TInput : class
    {
        public TInput Content { get; set; }
        public String SourceId { get; set; }
        public String SourceType { get; set; }
    }
}
