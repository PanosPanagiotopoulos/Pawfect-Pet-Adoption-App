using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
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
			// Check if the key already exists
			try
			{
				await _s3Client.GetObjectMetadataAsync(_awsConfig.BucketName, key);
				// If the above line doesn't throw, the object exists
				throw new InvalidOperationException($"A file with the key '{key}' already exists in the S3 bucket.");
			}
			catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				// Object does not exist, proceed with upload
			}
			catch (Exception ex)
			{
				// Handle other exceptions
				throw new InvalidOperationException($"Error checking for existing file: {ex.Message}", ex);
			}

			// Proceed with upload
			using (Stream stream = file.OpenReadStream())
			{
				TransferUtilityUploadRequest uploadRequest = new TransferUtilityUploadRequest
				{
					InputStream = stream,
					Key = key,
					BucketName = _awsConfig.BucketName,
					ContentType = file.ContentType,
				};

				TransferUtility transferUtility = new TransferUtility(_s3Client);
				await transferUtility.UploadAsync(uploadRequest);
			}
			return await this.GetAsync(key);
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

		public async Task<Dictionary<String, Boolean>> DeleteAsync(String key)
		{
			Dictionary<String, Boolean> results = await this.DeleteAsync(new List<String> { key });
			return results;
		}

		public async Task<Dictionary<String, Boolean>> DeleteAsync(List<String> keys)
		{
			if (keys == null || !keys.Any())
				throw new ArgumentException("No keys provided for deletion.");

			DeleteObjectsRequest deleteRequest = new DeleteObjectsRequest
			{
				BucketName = _awsConfig.BucketName,
				Objects = keys.Select(key => new KeyVersion { Key = key }).ToList(),
				Quiet = false // Set to false to get detailed response
			};

			DeleteObjectsResponse response = await _s3Client.DeleteObjectsAsync(deleteRequest);

			// Create a set of successfully deleted keys
			HashSet<String> deletedKeys = new HashSet<String>(response.DeletedObjects.Select(o => o.Key));

			// Create a set of keys with errors
			HashSet<String> errorKeys = new HashSet<String>(response.DeleteErrors.Select(e => e.Key));

			// Build the result dictionary
			Dictionary<String, Boolean> result = new Dictionary<String, Boolean>();
            foreach (String key in keys)
			{
				if (deletedKeys.Contains(key))
				{
					result[key] = true; // Deleted successfully
				}
				else if (errorKeys.Contains(key))
				{
					result[key] = false; // Failed to delete
				}
				else
				{
					result[key] = true; // Assume it didn't exist, so "deleted"
				}
			}

			return result;
		}

		public String ConstructAwsKey(params String[] keyParts)
		{
			if (keyParts == null || keyParts.Length == 0)
				throw new ArgumentException("At least one key part must be provided.");

			if (keyParts.Any(part => String.IsNullOrEmpty(part)))
				throw new ArgumentException("Key parts cannot be null or empty.");

			return String.Join("-", keyParts);
		}

	}
}
