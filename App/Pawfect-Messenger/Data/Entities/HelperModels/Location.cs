using Newtonsoft.Json.Linq;

/// <summary>
///   Η αναλυτική τοποθεσία του καταφυγίου
/// </summary>
public class Location
{
	public String Address { get; set; }

	public String Number { get; set; }
	public String City { get; set; }

	public String ZipCode { get; set; }

	// Utility method to get the location from a google people api response
	public static Location FromGoogle(String jsonResponse)
	{
		try
		{
			JObject person = JObject.Parse(jsonResponse);

			JArray addresses = person["addresses"] as JArray;
			if (addresses == null || !addresses.Any())
			{
				return null;
			}

			var firstAddress = addresses.First;
			String formattedValue = firstAddress["formattedValue"]?.ToString();
			String streetAddress = firstAddress["streetAddress"]?.ToString();
			String city = firstAddress["city"]?.ToString();
			String postalCode = firstAddress["postalCode"]?.ToString();

			String number = "";
			String addressLine = streetAddress;
			if (!String.IsNullOrEmpty(streetAddress))
			{
				String[] parts = streetAddress.Split(' ');
				if (parts.Length > 0)
				{
					number = parts[0];
					addressLine = String.Join(" ", parts.Skip(1));
				}
			}

			return new Location
			{
				Address = addressLine,
				Number = number,
				City = city,
				ZipCode = postalCode
			};
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error parsing Google address: " + ex.Message);
			return null;
		}
	}
}