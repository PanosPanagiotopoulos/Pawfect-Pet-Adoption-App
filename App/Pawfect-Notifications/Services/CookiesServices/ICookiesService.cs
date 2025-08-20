namespace Pawfect_Notifications.Services.CookiesServices
{
    public interface ICookiesService
    {
        void SetCookie(String key, String value, DateTime expireAt);
        void DeleteCookie(String key);
        String GetCookie(String key);
    }
}
