using Pawfect_API.Data.Entities;

namespace Pawfect_API.Data.Entities.Types.Animals
{
    public class AnimalSearchResult
    {
        public Animal Animal { get; set; }
        public Double VectorScore { get; set; }
        public Double SemanticScore { get; set; }
        public Double CombinedScore { get; set; }
        public int VectorRank { get; set; }
        public int SemanticRank { get; set; }
    }

}
