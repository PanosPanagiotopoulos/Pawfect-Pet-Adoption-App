namespace Pawfect_Pet_Adoption_App_API.Database
{
    /// <summary>
    /// Μοντέλο για setting της Mongo βασης στο σύστημα
    /// </summary>
    public class MongoDbSettings
    {
        /// <value>
        /// Το connection string της βάσης.
        /// </value>
        public string ConnectionString { get; set; } = null!;
        /// <value>
        /// Το όνομα της βάσης
        /// </value>
        public string DatabaseName { get; set; } = null!;
    }

}
