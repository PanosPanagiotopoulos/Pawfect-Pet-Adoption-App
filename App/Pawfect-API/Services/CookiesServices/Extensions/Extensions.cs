namespace Pawfect_API.Services.CookiesServices.Extensions
{
    public static class Extension
    {
        public static IServiceCollection AddCookiesServices(this IServiceCollection services)
        {
            services.AddScoped<ICookiesService, CookiesService>();

            return services;
        }
    }
}
