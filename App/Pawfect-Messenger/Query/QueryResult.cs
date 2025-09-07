namespace Pawfect_Messenger.Query
{
    public class QueryResult<T> where T: class
    {
        public List<T> Items { get; set; }
        public long Count { get; set; }
    }
}
