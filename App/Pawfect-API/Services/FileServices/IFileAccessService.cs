namespace Pawfect_API.Services.FileServices
{
    public interface IFileAccessService
    {
        Task AttachUrlsAsync(List<Pawfect_API.Data.Entities.File> files);
    }
}
