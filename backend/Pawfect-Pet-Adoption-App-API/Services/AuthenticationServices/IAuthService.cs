using Pawfect_Pet_Adoption_App_API.Models;

namespace Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices
{
	public interface IAuthService
	{
		/// <summary>
		/// Ανταλλάσσει έναν κωδικό εξουσιοδότησης με έναν access token από το Google.
		/// </summary>
		/// <param name="authorizationCode">Ο κωδικός εξουσιοδότησης για ανταλλαγή.</param>
		/// <returns>Το αντικείμενο GoogleTokenResponse που περιέχει το access token.</returns>
		/// <exception cref="InvalidOperationException">Εάν λείπει ο κωδικός εξουσιοδότησης ή τα διαπιστευτήρια του Google.</exception>
		Task<GoogleTokenResponse?> ExchangeCodeForAccessToken(String? authorizationCode);

		/// <summary>
		/// Ανακτά τις πληροφορίες του χρήστη από το Google χρησιμοποιώντας το access token.
		/// </summary>
		/// <param name="accessToken">Το access token για την ανάκτηση των πληροφοριών του χρήστη.</param>
		/// <returns>Το αντικείμενο GoogleUserInfo που περιέχει τις πληροφορίες του χρήστη.</returns>
		/// <exception cref="InvalidOperationException">Εάν λείπει το access token.</exception>
		Task<GoogleUserInfo?> GetGoogleUserInfo(String? accessToken);

		/// <summary>
		/// Ανακτά τα διαπιστευτήρια του χρήστη από το Google χρησιμοποιώντας τον κωδικό εξουσιοδότησης.
		/// </summary>
		/// <param name="authorisationCode">Ο κωδικός εξουσιοδότησης για την ανάκτηση των διαπιστευτηρίων.</param>
		/// <returns>Το ζεύγος (email, sub) που περιέχει το email και τον μοναδικό αναγνωριστικό του χρήστη.</returns>
		/// <exception cref="InvalidOperationException">Εάν αποτύχει η ανταλλαγή του κωδικού εξουσιοδότησης με το access token ή η ανάκτηση των πληροφοριών του χρήστη.</exception>
		Task<(String, String)> RetrieveGoogleCredentials(String? authorisationCode);
	}

}
