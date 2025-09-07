using Pawfect_Messenger.Data.Entities.EnumTypes;

namespace Pawfect_Messenger.Models.User
{
	public class User
	{
		public String Id { get; set; }
		public String Email { get; set; }
		public String FullName { get; set; }
		public File.File ProfilePhoto { get; set; }
        public Shelter.Shelter Shelter { get; set; }
    }
}
