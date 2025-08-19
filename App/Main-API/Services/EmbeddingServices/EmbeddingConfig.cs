namespace Pawfect_Pet_Adoption_App_API.Services.EmbeddingServices
{
    public class EmbeddingConfig
    {
        public String Model { get; set; }
        public String ApiKey { get; set; }
        public int Dims { get; set; }
        public int ChunkSize { get; set; }
        public Double OverlapSize { get; set; }
    }
}
