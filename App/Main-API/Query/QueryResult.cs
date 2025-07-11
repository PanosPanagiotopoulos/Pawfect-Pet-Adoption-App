namespace Pawfect_Pet_Adoption_App_API.Query
{
    public class QueryResult<T> where T: class
    {
        public List<T> Items { get; set; }
        public long Count { get; set; }
    }
}
