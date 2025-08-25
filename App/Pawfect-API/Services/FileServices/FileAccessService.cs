using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Pawfect_API.Data.Entities.EnumTypes;
using Pawfect_API.Data.Entities.Types.Files;
using Pawfect_API.Services.AwsServices;

namespace Pawfect_API.Services.FileServices
{
    public class FileAccessService : IFileAccessService
    {
        private readonly ILogger<FileAccessService> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly FilesConfig _filesConfig;
        private readonly IAwsService _awsService;

        private const String CACHE_KEY_PREFIX = "presigned_url:";

        public FileAccessService
        (
            ILogger<FileAccessService> logger,
            IOptions<FilesConfig> options,
            IMemoryCache memoryCache,
            IAwsService awsService
        )
        {
            this._logger = logger;
            this._memoryCache = memoryCache;
            this._filesConfig = options.Value;
            this._awsService = awsService;
        }
        public async Task AttachUrlsAsync(List<Data.Entities.File> files)
        {
            List<Data.Entities.File> privateFiles = [.. files.Where(file => file.AccessType == FileAccessType.Private)];
            if (privateFiles.Count == 0) return;

            List<Task<String>> urlTasks = privateFiles.Select(file => this.GeneratePreSignedUrlTask(file)).ToList();

            String[] preSignedUrls = await Task.WhenAll(urlTasks);

            if (preSignedUrls.Length != privateFiles.Count)
                throw new InvalidOperationException("Failed to extract all pre-signed urls");

            for (int i = 0; i < preSignedUrls.Length; i++)
                privateFiles[i].SourceUrl = preSignedUrls[i];
        }

        private async Task<String> GeneratePreSignedUrlTask(Data.Entities.File file)
        {
            String cacheKey = $"{CACHE_KEY_PREFIX}{file.AwsKey}";
            TimeSpan preSignedUrlsExpiry = TimeSpan.FromMinutes(_filesConfig.PrivateFilesExpiryTime);
            TimeSpan cacheExpiry = preSignedUrlsExpiry.Subtract(TimeSpan.FromMinutes(2));


            if (_memoryCache.TryGetValue(cacheKey, out String cachedUrl))
                return cachedUrl;

            String newUrl = await _awsService.GeneratePresignedUrlAsync(file.AwsKey, preSignedUrlsExpiry);

            _memoryCache.Set(cacheKey, newUrl, cacheExpiry);

            return newUrl;
        }
    }
}
