using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Data.Entities.HelperModels;
using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Models.User;

namespace Pawfect_Pet_Adoption_App_API.Models.Shelter
{
	public class ShelterDto
	{
		public String Id { get; set; }

		/// <value>
		/// Τα δεδομένα του αντίστοιχου χρήστη για το καταφύγιο
		/// </value>
		public UserDto? User { get; set; }

		public String ShelterName { get; set; }

		public String Description { get; set; }


		/// <value>
		/// Η ιστοσελίδα του καταφυγίου
		/// </value>
		public String? Website { get; set; }

		/// <value>
		/// Τα Links για τα Social Media του καταφυγίου
		/// </value>
		public SocialMedia? SocialMedia { get; set; }


		/// <value>
		/// Οι ώρες λειτουργίας του καταφυγίου.
		/// </value>
		public OperatingHours? OperatingHours { get; set; }


		/// <value>
		/// Η κατάσταση αιτήματος εγγραφής του καταφυγίου στο σύστημα.
		/// [ Pending, Verified, Rejected ]
		/// </value>
		/// 
		public VerificationStatus VerificationStatus { get; set; }


		/// <value>
		/// Το id του admin που επιβεβαίωσε την εγγραφή.
		/// Βλέπει σε admin user αφού επιβεβαιωθεί
		/// </value>
		public String? VerifiedBy { get; set; }

		public List<AnimalDto> Animals { get; set; }
	}
}
