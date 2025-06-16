namespace Main_API.Services.ReportServices.Extentions
{
	public static class Extention
	{
		public static IServiceCollection AddReportServices(this IServiceCollection services)
		{
			services.AddScoped<IReportService, ReportService>();
			services.AddScoped(provider => new Lazy<IReportService>(() => provider.GetRequiredService<IReportService>()));

			return services;
		}
	}
}
