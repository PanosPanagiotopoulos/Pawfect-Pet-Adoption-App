using Microsoft.AspNetCore.Mvc;

using Pawfect_API.Models.Shelter;
using Pawfect_API.Models.User;

namespace Pawfect_API.Models
{

	// Μοντέλο για την initial εγγραφή χρήστη ενώς χρήστη στο σύστημα στο σύστημα
	public class RegisterPersist
	{
		public UserPersist User { get; set; }

		public ShelterPersist Shelter { get; set; }
	}
}
