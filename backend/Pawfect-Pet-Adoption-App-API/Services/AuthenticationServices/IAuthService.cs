using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Apis;

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
	}

}
