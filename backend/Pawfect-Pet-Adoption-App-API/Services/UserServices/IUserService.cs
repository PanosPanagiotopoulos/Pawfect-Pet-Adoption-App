using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.User;

namespace Pawfect_Pet_Adoption_App_API.Services.UserServices
{
	public interface IUserService
	{
		/// <summary>
		/// Querying δεδομένων χρήστη.
		/// </summary>
		/// <param name="userLookup">Τα στοιχεία ζητούμενα στοιχεία querying για τον χρήστη.</param>
		/// <returns>Το Λίστα απο DTO user.</returns>
		Task<IEnumerable<UserDto>> QueryUsersAsync(UserLookup userLookup);

		/// <summary>
		/// Εγγραφή μη επιβεβαιωμένου χρήστη.
		/// </summary>
		/// <param name="registerPersist">Τα στοιχεία του χρήστη για εγγραφή.</param>
		/// <returns>Το ID του εγγεγραμμένου χρήστη.</returns>
		/// <exception cref="Exception">Ρίχνεται όταν αποτυγχάνει η εγγραφή του χρήστη.</exception>
		Task<String?> RegisterUserUnverifiedAsync(RegisterPersist registerPersist);

		/// <summary>
		/// Επαλήθευση OTP χρήστη.
		/// </summary>
		/// <param name="phonenumber">Ο αριθμός τηλεφώνου του χρήστη.</param>
		/// <param name="otpVerification">Τα στοιχεία επαλήθευσης OTP.</param>
		/// <returns>True αν η επαλήθευση είναι επιτυχής, αλλιώς false.</returns>
		Boolean VerifyOtp(String? phonenumber, int? OTP);

		/// <summary>
		/// Δημιουργία νέου OTP και αποστολή στον χρήστη.
		/// </summary>
		/// <param name="phonenumber">Ο αριθμός τηλεφώνου του χρήστη.</param>
		/// <exception cref="Exception">Ρίχνεται όταν αποτυγχάνει η δημιουργία ή αποστολή του OTP.</exception>
		Task GenerateNewOtpAsync(String? phonenumber);

		/// <summary>
		/// Αποστολή email επιβεβαίωσης στον χρήστη.
		/// </summary>
		/// <param name="email">Η διεύθυνση email του χρήστη.</param>
		/// <exception cref="Exception">Ρίχνεται όταν αποτυγχάνει η αποστολή του email επιβεβαίωσης.</exception>
		Task SendVerficationEmailAsync(String? email);

		/// <summary>
		/// Επαλήθευση email χρήστη.
		/// </summary>
		/// <param name="email">Η διεύθυνση email του χρήστη.</param>
		/// <param name="token">Το token επαλήθευσης.</param>
		/// <returns>True αν η επαλήθευση είναι επιτυχής, αλλιώς false.</returns>
		Boolean VerifyEmail(String? email, String? token);

		/// <summary>
		/// Αποθήκευση χρήστη.
		/// </summary>
		/// <param name="userPersist">Τα στοιχεία του χρήστη για αποθήκευση.</param>
		/// <param name="allowCreation">Επιτρέπει τη δημιουργία νέου χρήστη αν δεν υπάρχει.</param>
		/// <returns>Το ID του αποθηκευμένου χρήστη.</returns>
		/// <exception cref="Exception">Ρίχνεται όταν αποτυγχάνει η αποθήκευση του χρήστη.</exception>
		Task<String?> PersistUserAsync(UserPersist userPersist, Boolean allowCreation = true);

		/// <summary>
		/// Αποθήκευση χρήστη.
		/// </summary>
		/// <param name="user">Ο χρήστης για αποθήκευση.</param>
		/// <param name="allowCreation">Επιτρέπει τη δημιουργία νέου χρήστη αν δεν υπάρχει.</param>
		/// <returns>Το ID του αποθηκευμένου χρήστη.</returns>
		/// <exception cref="Exception">Ρίχνεται όταν αποτυγχάνει η αποθήκευση του χρήστη.</exception>
		Task<String?> PersistUserAsync(User user, Boolean allowCreation = true);

		/// <summary>
		/// Επαλήθευση χρήστη.
		/// </summary>
		/// <param name="toRegisterUser">Τα στοιχεία του χρήστη για επαλήθευση.</param>
		/// <returns>True αν η επαλήθευση είναι επιτυχής, αλλιώς false.</returns>
		/// <exception cref="Exception">Ρίχνεται όταν αποτυγχάνει η επαλήθευση του χρήστη.</exception>
		Task<Boolean> VerifyUserAsync(String? id, String? email);

		/// <summary>
		/// Ανάκτηση χρήστη.
		/// </summary>
		/// <param name="id">Το ID του χρήστη.</param>
		/// <param name="email">Η διεύθυνση email του χρήστη.</param>
		/// <returns>Ο χρήστης αν βρεθεί, αλλιώς null.</returns>
		Task<User?> RetrieveUserAsync(String? id, String? email);

		/// <summary>
		/// Αποστολή email επαναφοράς κωδικού.
		/// </summary>
		/// <param name="email">Η διεύθυνση email του χρήστη.</param>
		/// <exception cref="Exception">Ρίχνεται όταν αποτυγχάνει η αποστολή του email επαναφοράς κωδικού.</exception>
		Task SendResetPasswordEmailAsync(String? email);

		/// <summary>
		/// Επαναφορά κωδικού χρήστη.
		/// </summary>
		/// <param name="email">Η διεύθυνση email του χρήστη.</param>
		/// <param name="password">Ο νέος κωδικός.</param>
		/// <param name="token">Το token επαλήθευσης.</param>
		/// <returns>True αν η επαναφορά είναι επιτυχής, αλλιώς false.</returns>
		/// <exception cref="Exception">Ρίχνεται όταν αποτυγχάνει η επαναφορά του κωδικού.</exception>
		Task<Boolean> ResetPasswordAsync(String? email, String? password, String? token);
	}
}