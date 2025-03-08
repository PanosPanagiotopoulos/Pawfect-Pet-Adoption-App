using Microsoft.AspNetCore.Mvc;

using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Models.User;

namespace Pawfect_Pet_Adoption_App_API.Models
{

	// Μοντέλο για την initial εγγραφή χρήστη ενώς χρήστη στο σύστημα στο σύστημα
	public class RegisterPersist
	{
		[ModelBinder(BinderType = typeof(JsonModelBinder<UserPersist>))]
		public UserPersist User { get; set; }

		[ModelBinder(BinderType = typeof(JsonModelBinder<ShelterPersist>))]
		public ShelterPersist Shelter { get; set; }

		public IFormFile AttachedPhoto { get; set; }
	}
}
