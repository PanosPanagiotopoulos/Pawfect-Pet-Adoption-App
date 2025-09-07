
namespace Pawfect_Messenger.Services.FileServices
{
    public interface IFileAccessService
    {
        Task AttachUrlsAsync(List<Pawfect_Messenger.Data.Entities.File> files);
    }
}
