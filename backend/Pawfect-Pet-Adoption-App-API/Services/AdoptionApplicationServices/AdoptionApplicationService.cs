using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.File;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services.Convention;
using Pawfect_Pet_Adoption_App_API.Services.FileServices;
using ZstdSharp.Unsafe;

namespace Pawfect_Pet_Adoption_App_API.Services.AdoptionApplicationServices
{
	public class AdoptionApplicationService : IAdoptionApplicationService
	{
		private readonly ILogger<AdoptionApplicationService> _logger;
		private readonly AdoptionApplicationQuery _adoptionApplicationQuery;
		private readonly AdoptionApplicationBuilder _adoptionApplicationBuilder;
		private readonly IAdoptionApplicationRepository _adoptionApplicationRepository;
		private readonly IMapper _mapper;
		private readonly IConventionService _conventionService;
		private readonly Lazy<IFileService> _fileService;
		private readonly FileQuery _fileQuery;

		public AdoptionApplicationService(
			ILogger<AdoptionApplicationService> logger,
			AdoptionApplicationQuery adoptionApplicationQuery,
			AdoptionApplicationBuilder adoptionApplicationBuilder,
			IAdoptionApplicationRepository adoptionApplicationRepository,
			IMapper mapper,
			IConventionService conventionService,
			Lazy<IFileService> fileService,
			FileQuery fileQuery
		)
		{
			_logger = logger;
			_adoptionApplicationQuery = adoptionApplicationQuery;
			_adoptionApplicationBuilder = adoptionApplicationBuilder;
			_adoptionApplicationRepository = adoptionApplicationRepository;
			_mapper = mapper;
			_conventionService = conventionService;
			_fileService = fileService;
			_fileQuery = fileQuery;
		}
		public async Task<IEnumerable<AdoptionApplicationDto>> QueryAdoptionApplicationsAsync(AdoptionApplicationLookup adoptionApplicationLookup)
		{
			List<AdoptionApplication> queriedAdoptionApplications = await adoptionApplicationLookup.EnrichLookup(_adoptionApplicationQuery).CollectAsync();
			return await _adoptionApplicationBuilder.SetLookup(adoptionApplicationLookup).BuildDto(queriedAdoptionApplications, adoptionApplicationLookup.Fields.ToList());
		}

		public async Task<AdoptionApplicationDto?> Get(String id, List<String> fields)
		{
			//*TODO* Add authorization service with user roles and permissions

			AdoptionApplicationLookup lookup = new AdoptionApplicationLookup(_adoptionApplicationQuery);
			lookup.Ids = new List<String> { id };
			lookup.Fields = fields;
			lookup.PageSize = 1;
			lookup.Offset = 0;

			List<AdoptionApplication> adoptionApplication = await lookup.EnrichLookup().CollectAsync();

			if (adoptionApplication == null)
			{
				throw new InvalidDataException("Δεν βρέθηκε αίτηση  υιοθεσίας με αυτό το ID");
			}

			return (await _adoptionApplicationBuilder.SetLookup(lookup).BuildDto(adoptionApplication, fields)).FirstOrDefault();
		}

		public async Task<AdoptionApplicationDto?> Persist(AdoptionApplicationPersist persist)
		{
			Boolean isUpdate =  _conventionService.IsValidId(persist.Id);
			AdoptionApplication data = new AdoptionApplication();
			String dataId = String.Empty;
			if (isUpdate)
			{
				_mapper.Map(persist, data);
				data.UpdatedAt = DateTime.UtcNow;
			}
			else
			{
				_mapper.Map(persist, data);
				data.Id = null;
				data.CreatedAt = DateTime.UtcNow;
				data.UpdatedAt = DateTime.UtcNow;
			}

			// Set files to permanent
			await this.PersistFiles(persist.AttachedFilesIds, data.AttachedFilesIds);

			if (isUpdate) dataId = await _adoptionApplicationRepository.UpdateAsync(data);
			else dataId = await _adoptionApplicationRepository.AddAsync(data);


			if (String.IsNullOrEmpty(dataId))
			{
				throw new InvalidOperationException("Αποτυχία κατα το Persisting");
			}

			// Return dto model
			AdoptionApplicationLookup lookup = new AdoptionApplicationLookup(_adoptionApplicationQuery);
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = new List<String> { "*", nameof(User) + ".*", nameof(Shelter) + ".*", nameof(Animal) + ".*", nameof(Data.Entities.File) + ".*" };
			lookup.Offset = 0;
			lookup.PageSize = 1;

			return (
					 await _adoptionApplicationBuilder.SetLookup(lookup)
					.BuildDto(await lookup.EnrichLookup().CollectAsync(), lookup.Fields.ToList())
					).FirstOrDefault();
		}

		private async Task PersistFiles(List<String> attachedFilesIds, List<String> currentFileIds)
		{
			// Make nul lto an empty list so that we can delete all current file Ids
			if (attachedFilesIds == null) { attachedFilesIds = new List<String>(); }

			if (currentFileIds != null)
			{
				List<String> diff = currentFileIds.Except(attachedFilesIds).ToList();

				// If no difference with current , return
				if (diff.Count == 0 && currentFileIds.Count == attachedFilesIds.Count) { return; }

				// Else delete the ones that remains since they where deleted from the file id list
				if (diff.Count != 0) await _fileService.Value.Delete(diff);
			}

			// Is empty, means it got deleted, so no need to query for persisting
			if (attachedFilesIds.Count == 0) return;

			FileLookup lookup = new FileLookup(_fileQuery);
			lookup.Ids = attachedFilesIds;
			lookup.Fields = new List<String> { "*" };
			lookup.Offset = 0;
			lookup.PageSize = attachedFilesIds.Count;
			
			List<Data.Entities.File> attachedFiles = await lookup.EnrichLookup().CollectAsync();
			if (attachedFiles == null || !attachedFiles.Any())
			{
				_logger.LogError("Failed to saved attached files. No return from query");
				return;
			}

			List<FilePersist> persistModels = new List<FilePersist>();
			foreach (Data.Entities.File file in attachedFiles)
			{
				file.FileSaveStatus = FileSaveStatus.Permanent;
				persistModels.Add(_mapper.Map<FilePersist>(file));
			}

			await _fileService.Value.Persist(persistModels);
		}
	}
}
