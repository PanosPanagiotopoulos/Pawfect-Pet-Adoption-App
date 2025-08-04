namespace Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Mongo
{ 
	public class MongoDbConfig
	{
		public String ConnectionString { get; set; }
		public String DatabaseName { get; set; }

        public IndexSettings IndexSettings { get; set; }
    }

	public class IndexSettings
	{
        public String AnimalVectorSearchIndexName { get; set; }
        public String AnimalSchemanticIndexName { get; set; }
        public int Dims { get; set; }

        public int NumCandidates { get; set; }
        public int Topk { get; set; }
        public double VectorScoreThreshold { get; set; }
        public double TextScoreThreshold { get; set; }

        public List<IndexSynonyms> SynonymsBatch { get; set; }
    }

    public class IndexSynonyms
    {
        public String Category { get; set; }
        public List<String> Synonyms { get; set; }
    }

}
