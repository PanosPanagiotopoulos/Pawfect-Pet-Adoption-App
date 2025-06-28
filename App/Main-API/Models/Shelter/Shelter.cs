using Main_API.Data.Entities.EnumTypes;
using Main_API.Data.Entities.HelperModels;
using Main_API.Models.User;

namespace Main_API.Models.Shelter
{
	public class Shelter
	{
		public String? Id { get; set; }

		/// <value>
		/// Τα δεδομένα του αντίστοιχου χρήστη για το καταφύγιο
		/// </value>
		public User.User? User { get; set; }

		public String? ShelterName { get; set; }

		public String? Description { get; set; }


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
		public VerificationStatus? VerificationStatus { get; set; }


		/// <value>
		/// Το id του admin που επιβεβαίωσε την εγγραφή.
		/// Βλέπει σε admin user αφού επιβεβαιωθεί
		/// </value>
		public String? VerifiedBy { get; set; }

		public List<Animal.Animal>? Animals { get; set; }

		public List<AdoptionApplication.AdoptionApplication> ReceivedAdoptionApplications { get; set; }
	}
}
