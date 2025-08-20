namespace Pawfect_Pet_Adoption_App_Notifications.Data.Entities.Types.Authorisation
{
    public class InternalApiConfig
    {
        public String SharedSecret { get; set; }
        public List<String> AllowedServices { get; set; }
    }
}
