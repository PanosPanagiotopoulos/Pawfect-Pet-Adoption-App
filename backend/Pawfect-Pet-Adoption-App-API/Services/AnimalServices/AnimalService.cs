using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Models.File;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Repositories.Implementations;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services.Convention;
using Pawfect_Pet_Adoption_App_API.Services.FileServices;

namespace Pawfect_Pet_Adoption_App_API.Services.AnimalServices
{
	public class AnimalService : IAnimalService
	{
		private readonly ILogger<AnimalService> _logger;
		private readonly AnimalQuery _animalQuery;
		private readonly AnimalBuilder _animalBuilder;
		private readonly IAnimalRepository _animalRepository;
		private readonly IMapper _mapper;
		private readonly IConventionService _conventionService;
		private readonly Lazy<IFileService> _fileService;
		private readonly FileQuery _fileQuery;

		public AnimalService
			(
				ILogger<AnimalService> logger,
				AnimalQuery animalQuery,
				AnimalBuilder animalBuilder,
				IAnimalRepository animalRepository,
				IMapper mapper,
				IConventionService conventionService,
				Lazy<IFileService> fileService,
				FileQuery fileQuery
			)
		{
			_logger = logger;
			_animalQuery = animalQuery;
			_animalBuilder = animalBuilder;
			_animalRepository = animalRepository;
			_mapper = mapper;
			_conventionService = conventionService;
			_fileService = fileService;
			_fileQuery = fileQuery;
		}

		public async Task<IEnumerable<AnimalDto>> QueryAnimalsAsync(AnimalLookup animalLookup)
		{
			//*TODO* Add authorization service with user roles and permissions

			List<Animal> queriedAnimals = await animalLookup.EnrichLookup(_animalQuery).CollectAsync();
			return await _animalBuilder.SetLookup(animalLookup).BuildDto(queriedAnimals, animalLookup.Fields.ToList());
		}

		public async Task<AnimalDto?> Get(String id, List<String> fields)
		{
			//*TODO* Add authorization service with user roles and permissions

			AnimalLookup lookup = new AnimalLookup(_animalQuery);
			lookup.Ids = new List<String> { id };
			lookup.Fields = fields;
			lookup.PageSize = 1;
			lookup.Offset = 0;

			List<Animal> animal = await lookup.EnrichLookup().CollectAsync();

			if (animal == null)
			{
				throw new InvalidDataException("Δεν βρέθηκε ζώο με αυτό το ID");
			}

			return (await _animalBuilder.SetLookup(lookup).BuildDto(animal, fields)).FirstOrDefault();
		}

		public async Task<AnimalDto?> Persist(AnimalPersist persist, List<String> fields)
		{
			Boolean isUpdate = _conventionService.IsValidId(persist.Id);
			Animal data = new Animal();
			String dataId = String.Empty;

			//*TODO* Add authorization service with user roles and permissions

			if (isUpdate)
			{
				data = await _animalRepository.FindAsync(x => x.Id == persist.Id);

				if (data == null) throw new InvalidDataException("No entity found with id given");

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

			await this.PersistFiles(persist.AttachedPhotosIds, data.PhotosIds);

			if (isUpdate) dataId = await _animalRepository.UpdateAsync(data);
			else dataId = await _animalRepository.AddAsync(data);

			if (String.IsNullOrEmpty(dataId))
			{
				throw new InvalidOperationException("Αποτυχία κατα το Persisting");
			}

			// Return dto model
			AnimalLookup lookup = new AnimalLookup(_animalQuery);
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = fields;
			lookup.Offset = 0;
			lookup.PageSize = 1;

			return (
					 await _animalBuilder.SetLookup(lookup)
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

			await _fileService.Value.Persist
			(
				persistModels,
				new List<String>() { nameof(FileDto.Id) }
			);
		}

		public async Task Delete(String id) { await this.Delete(new List<String>() { id }); }

		public async Task Delete(List<String> ids)
		{
			// TODO : Authorization

			FileLookup lookup = new FileLookup(_fileQuery);
			lookup.OwnerIds = ids;
			lookup.Fields = new List<String> { nameof(AnimalDto.Id) };
			lookup.Offset = 0;
			lookup.PageSize = 50;

			List<Data.Entities.File> attachedFiles = await lookup.EnrichLookup().CollectAsync();
			await _fileService.Value.Delete(attachedFiles?.Select(x => x.Id).ToList());

			await _animalRepository.DeleteAsync(ids);
		}
	}
}