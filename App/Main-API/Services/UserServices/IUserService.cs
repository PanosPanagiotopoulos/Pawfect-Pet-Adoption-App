using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.User;

namespace Pawfect_Pet_Adoption_App_API.Services.UserServices
{
	public interface IUserService
	{
		/// <summary>
		/// Εγγραφή μη επιβεβαιωμένου χρήστη.
		/// </summary>
		/// <param name="registerPersist">Τα στοιχεία του χρήστη για εγγραφή.</param>
		/// <returns>Το ID του εγγεγραμμένου χρήστη.</returns>
		/// <exception cref="Exception">Ρίχνεται όταν αποτυγχάνει η εγγραφή του χρήστη.</exception>
		Task<Models.User.User?> RegisterUserUnverifiedAsync(RegisterPersist registerPersist, List<String> fields);

		/// <summary>
		/// Επαλήθευση OTP χρήστη.
		/// </summary>
		/// <param name="phonenumber">Ο αριθμός τηλεφώνου του χρήστη.</param>
		/// <param name="otpVerification">Τα στοιχεία επαλήθευσης OTP.</param>
		/// <returns>True αν η επαλήθευση είναι επιτυχής, αλλιώς false.</returns>
		Boolean VerifyOtp(String phonenumber, int? OTP);

		/// <summary>
		/// Δημιουργία νέου OTP και αποστολή στον χρήστη.
		/// </summary>
		/// <param name="phonenumber">Ο αριθμός τηλεφώνου του χρήστη.</param>
		/// <exception cref="Exception">Ρίχνεται όταν αποτυγχάνει η δημιουργία ή αποστολή του OTP.</exception>
		Task GenerateNewOtpAsync(String phonenumber);

		/// <summary>
		/// Αποστολή email επιβεβαίωσης στον χρήστη.
		/// </summary>
		/// <param name="email">Η διεύθυνση email του χρήστη.</param>
		/// <exception cref="Exception">Ρίχνεται όταν αποτυγχάνει η αποστολή του email επιβεβαίωσης.</exception>
		Task SendVerficationEmailAsync(String email);

		/// <summary>
		/// Επαλήθευση email χρήστη.
		/// </summary>
		/// <param name="email">Η διεύθυνση email του χρήστη.</param>
		/// <param name="token">Το token επαλήθευσης.</param>
		/// <returns>True αν η επαλήθευση είναι επιτυχής, αλλιώς false.</returns>
		String VerifyEmail(String token);

		/// <summary>
		/// Αποθήκευση χρήστη.
		/// </summary>
		/// <param name="userPersist">Τα στοιχεία του χρήστη για αποθήκευση.</param>
		/// <param name="allowCreation">Επιτρέπει τη δημιουργία νέου χρήστη αν δεν υπάρχει.</param>
		/// <returns>Το ID του αποθηκευμένου χρήστη.</returns>
		/// <exception cref="Exception">Ρίχνεται όταν αποτυγχάνει η αποθήκευση του χρήστη.</exception>
		Task<Models.User.User?> Persist(UserPersist userPersist, Boolean allowCreation = true, List<String> buildFields = null, Boolean buildDto = true);

		/// <summary>
		/// Αποθήκευση χρήστη.
		/// </summary>
		/// <param name="user">Ο χρήστης για αποθήκευση.</param>
		/// <param name="allowCreation">Επιτρέπει τη δημιουργία νέου χρήστη αν δεν υπάρχει.</param>
		/// <returns>Το ID του αποθηκευμένου χρήστη.</returns>
		/// <exception cref="Exception">Ρίχνεται όταν αποτυγχάνει η αποθήκευση του χρήστη.</exception>
		Task<Models.User.User?> Persist(Data.Entities.User user, Boolean allowCreation = true, List<String> buildFields = null, Boolean buildDto = true);

		/// <summary>
		/// Επαλήθευση χρήστη.
		/// </summary>
		/// <param name="toRegisterUser">Τα στοιχεία του χρήστη για επαλήθευση.</param>
		/// <returns>True αν η επαλήθευση είναι επιτυχής, αλλιώς false.</returns>
		/// <exception cref="Exception">Ρίχνεται όταν αποτυγχάνει η επαλήθευση του χρήστη.</exception>
		Task<Boolean> VerifyUserAsync(String id, String email);

		/// <summary>
		/// Ανάκτηση χρήστη.
		/// </summary>
		/// <param name="id">Το ID του χρήστη.</param>
		/// <param name="email">Η διεύθυνση email του χρήστη.</param>
		/// <returns>Ο χρήστης αν βρεθεί, αλλιώς null.</returns>
		Task<Data.Entities.User?> RetrieveUserAsync(String id, String email);

		/// <summary>
		/// Αποστολή email επαναφοράς κωδικού.
		/// </summary>
		/// <param name="email">Η διεύθυνση email του χρήστη.</param>
		/// <exception cref="Exception">Ρίχνεται όταν αποτυγχάνει η αποστολή του email επαναφοράς κωδικού.</exception>
		Task SendResetPasswordEmailAsync(String email);

		Task<String> VerifyResetPasswordToken(String token);

		/// <summary>
		/// Επαναφορά κωδικού χρήστη.
		/// </summary>
		/// <param name="email">Η διεύθυνση email του χρήστη.</param>
		/// <param name="password">Ο νέος κωδικός.</param>
		/// <param name="token">Το token επαλήθευσης.</param>
		/// <returns>True αν η επαναφορά είναι επιτυχής, αλλιώς false.</returns>
		/// <exception cref="Exception">Ρίχνεται όταν αποτυγχάνει η επαναφορά του κωδικού.</exception>
		Task<Boolean> ResetPasswordAsync(String password, String token);

		String ExtractUserCredential(Data.Entities.User user);

		/// <summary>
		/// Ανακτά τις πληροφορίες του χρήστη από το Google χρησιμοποιώντας το access token.
		/// </summary>
		/// <param name="accessToken">Το access token για την ανάκτηση των πληροφοριών του χρήστη.</param>
		/// <returns>Το αντικείμενο GoogleUserInfo που περιέχει τις πληροφορίες του χρήστη.</returns>
		/// <exception cref="InvalidOperationException">Εάν λείπει το access token.</exception>
		Task<Data.Entities.User?> GetGoogleUser(String AuthorizationCode);

		Task<(String, String)> RetrieveGoogleCredentials(String AuthorizationCode);
		Task Delete(String id);
		Task Delete(List<String> ids);
	}
}