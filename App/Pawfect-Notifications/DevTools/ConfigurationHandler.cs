using System.Text.RegularExpressions;

namespace Pawfect_Notifications.DevTools
{
	public static class ConfigurationHandler
	{
		public static IConfiguration ReplacePlaceholders(IConfiguration configuration)
		{
			// Convert configuration to a dictionary for easy lookup
			Dictionary<String, String?> environmentVariables = configuration.AsEnumerable()
				.Where(kvp => kvp.Value != null) // Ensure no null values
				.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			// Process individual key-value placeholders
			foreach (KeyValuePair<String, String> kvp in environmentVariables.ToList())
			{
				if (kvp.Value != null && HasPlaceholder(kvp.Value))
				{
					String resolvedValue = ReplacePlaceholderValue(kvp.Value, environmentVariables);
					configuration[kvp.Key] = resolvedValue;
				}
			}

			// Process nested sections including lists
			ReplaceSectionPlaceholders(configuration, environmentVariables);

			return configuration;
		}

		/// <summary>
		/// Recursively replaces placeholders inside configuration sections (including lists).
		/// </summary>
		private static void ReplaceSectionPlaceholders(IConfiguration configuration, Dictionary<String, String?> envVars)
		{
			foreach (IConfigurationSection section in configuration.GetChildren())
			{
				if (section.Value == null) // If it's an object or list
				{
					ReplaceSectionPlaceholders(section, envVars); // Recursively process nested objects

					// 🔹 Correctly handle lists (arrays)
					List<IConfigurationSection> children = section.GetChildren().ToList();
					if (children.Count > 0 && children.All(c => int.TryParse(c.Key, out _))) // If all keys are numbers, it's a list
					{
						for (int i = 0; i < children.Count; i++)
						{
							String? value = children[i].Value;
							if (value != null && HasPlaceholder(value))
							{
								String resolvedValue = ReplacePlaceholderValue(value, envVars);
								section[$"{i}"] = resolvedValue;
							}
						}
					}
				}
				else if (HasPlaceholder(section.Value))
				{
					String resolvedValue = ReplacePlaceholderValue(section.Value, envVars);
					section.Value = resolvedValue;
				}
			}
		}


		/// <summary>
		/// Checks if a value contains a placeholder pattern like "%{Key}%" or "${Key}$".
		/// </summary>
		private static Boolean HasPlaceholder(String value) =>
			value.Contains("%{") && value.Contains("}%") || value.Contains("${") && value.Contains("}$");

		/// <summary>
		/// Resolves a placeholder value by looking it up in the environment variables dictionary.
		/// </summary>
		private static String ReplacePlaceholderValue(String value, Dictionary<String, String?> envVars)
		{
			return Regex.Replace(value, @"[%$]{(\w+)}[%$]", match =>
			{
				String key = match.Groups[1].Value;
				if (envVars.TryGetValue(key, out String? resolvedValue) && resolvedValue != null)
				{
					return resolvedValue;
				}
				throw new Exception($"Placeholder '{key}' not found in environment configuration.");
			});
		}

	}
}
