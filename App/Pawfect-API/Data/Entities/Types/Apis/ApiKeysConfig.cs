namespace Pawfect_API.Data.Entities.Types.Apis
{
    public class ApiKeyConfig
    {
        public List<ApiKeyRecord> KeyRecords { get; set; }
    }
    public class ApiKeyRecord
    {
        public String ClientName { get; set; }
        public String ApiKey { get; set; }
    }
}
