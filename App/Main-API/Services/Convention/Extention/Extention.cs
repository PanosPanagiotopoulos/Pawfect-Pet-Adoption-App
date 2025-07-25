﻿using Main_API.Services.BreedServices;

namespace Main_API.Services.Convention.Extention
{
	public static class Extention
	{
		public static IServiceCollection AddConventionServices(this IServiceCollection services)
		{
			services.AddScoped<IConventionService, ConventionService>();
			services.AddScoped(provider => new Lazy<IConventionService>(() => provider.GetRequiredService<IConventionService>()));

			return services;
		}
	}
}
