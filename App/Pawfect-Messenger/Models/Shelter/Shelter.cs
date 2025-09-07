using Pawfect_Messenger.Data.Entities.EnumTypes;
using Pawfect_Messenger.Data.Entities.HelperModels;
using Pawfect_Messenger.Models.User;

namespace Pawfect_Messenger.Models.Shelter
{
	public class Shelter
	{
		public String? Id { get; set; }

		public Models.User.User User { get; set; }

		public String? ShelterName { get; set; }

		public String? Description { get; set; }

		public String? Website { get; set; }

		public SocialMedia? SocialMedia { get; set; }

		public OperatingHours? OperatingHours { get; set; }

		public VerificationStatus? VerificationStatus { get; set; }

		public String? VerifiedBy { get; set; }
	}
}
