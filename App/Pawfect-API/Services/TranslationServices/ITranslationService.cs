namespace Pawfect_API.Services.TranslationServices
{
    public interface ITranslationService
    {
        Task<String> TranslateAsync(String input, String sourceLang, String targetLang);
    }
}
