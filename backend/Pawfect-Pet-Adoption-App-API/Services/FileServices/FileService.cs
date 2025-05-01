using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Files;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.File;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query.Interfaces;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Services.AwsServices;
using Pawfect_Pet_Adoption_App_API.Services.Convention;
using System.Linq;

namespace Pawfect_Pet_Adoption_App_API.Services.FileServices
{
	public class FileService : IFileService
	{
		private readonly ILogger<FileService> _logger;
		private readonly IFileRepository _fileRepository;
		private readonly IAwsService _awsService;
		private readonly IMapper _mapper;
		private readonly FileQuery _fileQuery;
		private readonly FileBuilder _fileBuilder;
		private readonly IConventionService _conventionService;
		private readonly FilesConfig _filesConfig;

		public FileService
		(
			ILogger<FileService> logger,
			IFileRepository fileRepository,
			IAwsService awsService,
			IMapper mapper,
			FileQuery fileQuery,
			FileBuilder fileBuilder,
			IConventionService conventionService,
			IOptions<FilesConfig> filesConfig

		)
		{
			_logger = logger;
			_fileRepository = fileRepository;
			_awsService = awsService;
			_mapper = mapper;
			_fileQuery = fileQuery;
			_fileBuilder = fileBuilder;
			_conventionService = conventionService;
			_filesConfig = filesConfig.Value;
		}

		public async Task<IEnumerable<FileDto>> QueryFilesAsync(FileLookup fileLookup)
		{
			if (fileLookup.FileSaveStatuses == null)
				fileLookup.FileSaveStatuses = new List<FileSaveStatus>() { FileSaveStatus.Permanent };

			if (!fileLookup.FileSaveStatuses.Contains(FileSaveStatus.Permanent))
				fileLookup.FileSaveStatuses.Add(FileSaveStatus.Permanent);

			List<Data.Entities.File> queriedFiles = await fileLookup.EnrichLookup(_fileQuery).CollectAsync();
			return await _fileBuilder.SetLookup(fileLookup).BuildDto(queriedFiles, fileLookup.Fields.ToList());
		}

		public async Task<FileDto> Persist(FilePersist persist, List<String> fields)
		{
			return (await this.Persist(new List<FilePersist>() { persist }, fields)).FirstOrDefault();
		}
		public async Task<List<FileDto>> Persist(List<FilePersist> persists, List<String> fields)
		{
			// TODO : Authorization

			Boolean isUpdate = persists.Select(f => f.Id).Any(_conventionService.IsValidId);
			List<Data.Entities.File> persistData = new List<Data.Entities.File>();
			foreach (FilePersist persist in persists)
			{
				Data.Entities.File data = new Data.Entities.File();

				if (isUpdate)
				{
					data = await _fileRepository.FindAsync(f => f.Id == persist.Id);

					if (!data.SourceUrl.Equals(persist.SourceUrl))
					{
						throw new ArgumentException("Cannot change source url");
					}

					if (!persist.FileSaveStatus.HasValue || !(persist.FileSaveStatus.Value == FileSaveStatus.Permanent))
					{
						throw new ArgumentException("FileSaveStatus is required");
					}

					_mapper.Map(persist, data);

					data.UpdatedAt = DateTime.UtcNow;

					persistData.Add(data);
				}
				else
				{
					_mapper.Map(persist, data);

					data.Id = null; // Ensure new ID is generated
					data.FileSaveStatus = FileSaveStatus.Temporary;
					data.CreatedAt = DateTime.UtcNow;
					data.UpdatedAt = DateTime.UtcNow;

					persistData.Add(data);
				}
			}

			List<String> persistedIds = null;
			if (isUpdate) persistedIds = await _fileRepository.UpdateManyAsync(persistData);
			else persistedIds = await _fileRepository.AddManyAsync(persistData);

			if (persistedIds == null || (persistedIds.Count != persistData.Count))
			{
				throw new InvalidOperationException("Failed to persist all files");
			}

			// Return dto model
			FileLookup lookup = new FileLookup(_fileQuery);
			lookup.Ids = persistedIds;
			lookup.Fields = fields;
			lookup.Offset = 0;
			lookup.PageSize = 1;

			return await _fileBuilder.SetLookup(lookup).BuildDto(await lookup.EnrichLookup().CollectAsync(), lookup.Fields.ToList());
		}

		public async Task<FileDto> Get(String id, List<String> fields)
		{
			Data.Entities.File file = await _fileRepository.FindAsync(f => f.Id == id, fields);
			if (file == null)
			{
				throw new InvalidOperationException("File not found");
			}

			return (await _fileBuilder.BuildDto(new List<Data.Entities.File> { file }, fields)).FirstOrDefault();
		}

		public async Task<FileDto> SaveTemporarily(TempMediaFile tempMediaFile)
		{
			return (await this.SaveTemporarily(new List<TempMediaFile>() { tempMediaFile })).FirstOrDefault();
		}

		public async Task<IEnumerable<FileDto>> SaveTemporarily(List<TempMediaFile> tempMediaFiles)
		{
			if (tempMediaFiles == null || !tempMediaFiles.Any())
				throw new ArgumentException("No files provided for upload.");

			// Step 1: Pre-generate unique IDs and validate files
			List<Data.Entities.FileInfo> fileInfos = tempMediaFiles.Select(tmf =>
			{
				String fileId = ObjectId.GenerateNewId().ToString();
				String key = _awsService.ConstructAwsKey(fileId, tmf.OwnerId);
				(Boolean IsValid, String ErrorMessage) validationResult = _conventionService.IsValidFile(tmf.File, _filesConfig);
				return new Data.Entities.FileInfo
				{
					FileId = fileId,
					Key = key,
					TempMediaFile = tmf,
					IsValid = validationResult.IsValid,
					ErrorMessage = validationResult.ErrorMessage
				};
			}).ToList();

			// Step 2: Process files in batches
			List<FileSaveResult> saveResults = new List<FileSaveResult>();
			IEnumerable<FileDto> dtos = null;

			List<Data.Entities.FileInfo> validFiles = fileInfos.Where(fi => fi.IsValid).ToList();
			for (int i = 0; i < validFiles.Count; i += _filesConfig.BatchSize)
			{
				List<Data.Entities.FileInfo> batch = validFiles.Skip(i).Take(_filesConfig.BatchSize).ToList();
				IEnumerable<Task<UploadResult>> uploadTasks = batch.Select(async (Data.Entities.FileInfo info) =>
				{
					int retryDelay = _filesConfig.InitialRetryDelayMs;
					for (int attempt = 0; attempt < _filesConfig.MaxRetryCount; attempt++)
					{
						try
						{
							String url = await _awsService.UploadAsync(info.TempMediaFile.File, info.Key);
							FilePersist persist = new FilePersist
							{
								Id = info.FileId,
								Filename = info.TempMediaFile.File.FileName,
								FileType = _conventionService.ToFileType(Path.GetExtension(info.TempMediaFile.File.FileName)),
								MimeType = info.TempMediaFile.File.ContentType,
								Size = info.TempMediaFile.File.Length,
								FileSaveStatus = FileSaveStatus.Temporary,
								SourceUrl = url,
								OwnerId = info.TempMediaFile.OwnerId
							};
							return new UploadResult { Persist = persist, FileName = info.TempMediaFile.File.FileName, Error = (String)null };
						}
						catch (Exception ex)
						{
							if (attempt == _filesConfig.MaxRetryCount - 1)
								return new UploadResult { Persist = (FilePersist)null, FileName = info.TempMediaFile.File.FileName, Error = ex.Message };
							await Task.Delay(retryDelay);
							retryDelay *= 2; // Exponential backoff
						}
					}
					return new UploadResult { Persist = (FilePersist)null, FileName = info.TempMediaFile.File.FileName, Error = "Unexpected retry failure" };
				});

				List<UploadResult> failedUploads = (await Task.WhenAll(uploadTasks)).Where(r => r.Persist == null).ToList();
				
				// -- End Proccess Of Files -- //


				// Step 3: Bulk insert metadata for successful uploads
				if ((await Task.WhenAll(uploadTasks)).Where(r => r.Persist != null).ToList().Count != 0)
				{
					List<Data.Entities.File> files = (await Task.WhenAll(uploadTasks)).Where(r => r.Persist != null).ToList().Select(r => _mapper.Map<Data.Entities.File>(r.Persist)).ToList();
					DateTime now = DateTime.UtcNow;
					foreach (Data.Entities.File file in files)
					{
						file.CreatedAt = now;
						file.UpdatedAt = now;
					}

					await _fileRepository.AddManyAsync(files);

					// Step 4: Build DTOs for successful uploads
					List<String> fileIds = files.Select(f => f.Id).ToList();
					FileLookup lookup = new FileLookup(_fileQuery)
					{
						Ids = fileIds,
						Fields = new List<String> { "*" },
						Offset = 0,
						PageSize = fileIds.Count
					};

					dtos = await this.QueryFilesAsync(lookup);

					saveResults.AddRange((await Task.WhenAll(uploadTasks)).Where(r => r.Persist != null).ToList().Select((su, index) => new FileSaveResult
					{
						FileName = su.FileName,
						File = dtos.ElementAt(index),
						Success = true,
						ErrorMessage = null
					}));
				}

				saveResults.AddRange(failedUploads.Select(fu => new FileSaveResult
				{
					FileName = fu.FileName,
					File = null,
					Success = false,
					ErrorMessage = fu.Error
				}));
			}

			// Step 5: Add invalid files to results
			saveResults.AddRange(fileInfos.Where(fi => !fi.IsValid).Select(fi => new FileSaveResult
			{
				FileName = fi.TempMediaFile.File.FileName,
				File = null,
				Success = false,
				ErrorMessage = fi.ErrorMessage
			}));

			// Step 6: Log performance metrics
			LogUploadPerformance(saveResults, tempMediaFiles.Count);

			return dtos;
		}
		private void LogUploadPerformance(List<FileSaveResult> results, int totalFiles)
		{
			// Placeholder for logging logic, e.g., using ILogger
			_logger.LogInformation($"Uploaded {results.Count(r => r.Success)}/{totalFiles} files successfully.");
		}


		public async Task Delete(String id) { await this.Delete(new List<String>() { id }); }

		public async Task Delete(List<String> ids)
		{
			// TODO : Authorization
			FileLookup lookup = new FileLookup(_fileQuery);
			lookup.Ids = ids;
			lookup.Fields = new List<String> { nameof(FileDto.Id), nameof(FileDto.Owner) };
			lookup.Offset = 0;
			lookup.PageSize = 1000;

			List<Data.Entities.File> files = await lookup.EnrichLookup().CollectAsync();

			List<String> keys = files.Select(file => { return _awsService.ConstructAwsKey(file.Id, file.OwnerId); } ).ToList();

			Dictionary<String, Boolean> results = await _awsService.DeleteAsync(keys);

			List<String> failedIds = results.Where(r => !r.Value).Select(r => r.Key.Split('-')[0]).ToList();

			if (failedIds.Any())
			{
				_logger.LogError("Not all objects where deleted from AWS. Removing them from file deleting pipeline");

				// Remove failed file IDs from the original ids list
				ids = ids.Except(failedIds).ToList();
			}

			await _fileRepository.DeleteAsync(ids);
		}
	}
}
