using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Options;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Aws;

namespace Pawfect_Pet_Adoption_App_API.Services.AwsServices
{
	public class AwsService : IAwsService
	{
		private readonly IAmazonS3 _s3Client;
		private readonly AwsConfig _awsConfig;

		public AwsService(IOptions<AwsConfig> awsConfig)
		{
			_awsConfig = awsConfig.Value;
			_s3Client = new AmazonS3Client(_awsConfig.AccessKey, _awsConfig.SecretKey, RegionEndpoint.GetBySystemName(_awsConfig.Region));
		}

		public async Task<String> UploadAsync(IFormFile file, String key)
		{
			try
			{
				using (Stream stream = file.OpenReadStream())
				{
					// TODO: Add ACL or Validation To File Handling?
					TransferUtilityUploadRequest uploadRequest = new TransferUtilityUploadRequest
					{
						InputStream = stream,
						Key = key,
						BucketName = _awsConfig.BucketName,
						ContentType = file.ContentType,
						CannedACL = S3CannedACL.PublicRead // Set ACL to public-read for permanent access
					};

					TransferUtility transferUtility = new TransferUtility(_s3Client);
					await transferUtility.UploadAsync(uploadRequest);
				}
				return await this.GetAsync(key); // Return the public URL after upload
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to upload file to S3: {ex.Message}", ex);
			}
		}

		public async Task<String> GetAsync(String key)
		{
			try
			{
				return await Task.FromResult($"https://{_awsConfig.BucketName}.s3.{_awsConfig.Region}.amazonaws.com/{key}");
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to generate file URL: {ex.Message}", ex);
			}
		}
	}
}
