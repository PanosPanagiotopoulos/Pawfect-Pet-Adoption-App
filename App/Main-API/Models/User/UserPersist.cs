using Main_API.Data.Entities.EnumTypes;

namespace Main_API.Models.User
{
	public class UserPersist
	{
		public String Id { get; set; }

		public String Email { get; set; }

		public String Password { get; set; }


		public String FullName { get; set; }


		/// <value>
		/// Ο ρόλος του χρήστη στο σύστημα.
		/// </value>
		public UserRole Role { get; set; } // Enum: User, Shelter, Admin

		/// <value>
		/// Ο αριθμός τηλεφώνου του χρήστη στο σύστημα
		/// </value>
		public String Phone { get; set; }


		/// <value>
		/// Η τοποθεσία του χρήστη
		/// </value>
		public Location Location { get; set; }


		/// <value>
		/// Ο τρόπος πρόσβασης του χρήστη. Π.χ Local άν συνδέεται με email, password ή Google άν συνδέεται με google.
		/// </value>
		/// [ Google, Local ]
		public AuthProvider AuthProvider { get; set; }

		/// <value>
		/// To id του χρήστη στην εξωτερική υπηρεσία που επέλεξε να εγγραφεί/συνδεθεί
		/// </value>
		public String? AuthProviderId { get; set; }

		public String ProfilePhotoId { get; set; }

		// *TODO* Set how the saving , parsing and validation of form file data will be done
		//public IFormFile? AttachedPhoto { get; set; }

		public Boolean HasPhoneVerified { get; set; }
		public Boolean HasEmailVerified { get; set; }

	}
}
