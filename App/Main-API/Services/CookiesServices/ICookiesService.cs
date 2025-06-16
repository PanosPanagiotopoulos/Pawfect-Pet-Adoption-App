namespace Main_API.Services.CookiesServices
{
    public interface ICookiesService
    {
        void AddCookie(String key, String value, DateTime expireAt);
        void DeleteCookie(String key);
    }
}
