
namespace Pawfect_Notifications.Services.CookiesServices
{
    public class CookiesService : ICookiesService
    {
        private readonly IHttpContextAccessor _ctxAccessor;
        private readonly IWebHostEnvironment _env;

        public CookiesService
        (
            IHttpContextAccessor accessor, 
            IWebHostEnvironment env
        )
        {
            _ctxAccessor = accessor;
            _env = env;
        }

        public void SetCookie(String key, String value, DateTime expireAt)
        {
            HttpResponse response = this.GetResponse();
            response.Cookies.Append(key, value, BuildOptions(expireAt));
        }

        public void DeleteCookie(String key)
        {
            HttpResponse response = this.GetResponse();
            response.Cookies.Delete(key);
        }

        public String GetCookie(String key)
        {
            HttpRequest request = _ctxAccessor.HttpContext?.Request ?? throw new InvalidOperationException("No active HttpContext.");
            if (request.Cookies.TryGetValue(key, out String value))
                return value;
            return null;
        }

        private CookieOptions BuildOptions(DateTime expiresUtc) =>
            new CookieOptions()
            {
                HttpOnly = true,
                Expires = expiresUtc,
                SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
                Secure = !_env.IsDevelopment()
            };

        private HttpResponse GetResponse() =>
            _ctxAccessor.HttpContext?.Response ?? throw new InvalidOperationException("No active HttpContext.");
    }
}
