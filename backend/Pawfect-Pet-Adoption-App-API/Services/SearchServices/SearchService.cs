using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Apis;
using Pawfect_Pet_Adoption_App_API.Models.Animal;

using System.Text;
using System.Text.Json;

namespace Pawfect_Pet_Adoption_App_API.Services.SearchServices
{
	public class SearchService
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<SearchService> _logger;

		public SearchService(HttpClient httpClient, ILogger<SearchService> logger)
		{
			_httpClient = httpClient;
			_logger = logger;
			_httpClient.BaseAddress = new Uri("http://localhost:5000"); // Search server URL
		}

		// Ευρετήριο ζώου
		// Εισαγωγή ζώου στον search server
		// Επιστρέφει την απόκριση του server ως String
		public async Task<String?> IndexAnimalAsync(List<AnimalIndexModel> animals)
		{
			try
			{
				String json = JsonSerializer.Serialize(animals);
				StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

				HttpResponseMessage response = await _httpClient.PostAsync("/add", content);
				response.EnsureSuccessStatusCode();

				return await response.Content.ReadAsStringAsync();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Αποτυχία index animals στον search server.");
				return null;
			}
		}

		// Αναζήτηση ζώων
		// Αναζητά ζώα με βάση το ερώτημα
		// Επιστρέφει τη λίστα των ζώων που βρέθηκαν
		public async Task<List<AnimalIndexModel>?> SearchAnimalsAsync(SearchRequest query)
		{
			try
			{
				String json = JsonSerializer.Serialize(query);
				StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

				HttpResponseMessage response = await _httpClient.PostAsync("/animals/search", content);
				response.EnsureSuccessStatusCode();

				String responseString = await response.Content.ReadAsStringAsync();
				return JsonSerializer.Deserialize<List<AnimalIndexModel>>(responseString);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Αποτυχία αναζήτησης στον search server.");
				return null;
			}

		}
	}
}

