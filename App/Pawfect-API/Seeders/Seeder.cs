using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Pawfect_API.Data.Entities;
using Pawfect_API.Data.Entities.EnumTypes;
using Pawfect_API.DevTools;
using Pawfect_API.Services.AwsServices;
using Pawfect_API.Services.Convention;
using Pawfect_API.Services.MongoServices;
using Pawfect_API.Services.EmbeddingServices;
using Microsoft.Extensions.Options;
using Pawfect_API.Data.Entities.Types.Mongo;
using Pawfect_API.Services.EmbeddingServices.AnimalEmbeddingValueExtractors;
using Pawfect_API.Data.Entities.Types.Embedding;

public class Seeder
{
	private readonly MongoDbService _dbService;
    private readonly IAwsService _awsService;
    private readonly IConventionService _conventionService;
    private readonly IEmbeddingService _embeddingService;
    private readonly EmbeddingValueExtractorFactory _embeddingValueExtractorFactory;
    private readonly MongoDbConfig _config;

    public Seeder
    (
        MongoDbService dbService, 
        IAwsService awsService,
        IConventionService conventionService,
        IEmbeddingService embeddingService,
        EmbeddingValueExtractorFactory embeddingValueExtractorFactory,
        IOptions<MongoDbConfig> options
    )
	{
		this._dbService = dbService;
        this._awsService = awsService;
        this._conventionService = conventionService;
        this._embeddingService = embeddingService;
        this._embeddingValueExtractorFactory = embeddingValueExtractorFactory;
        this._config = options.Value;
    }

	public async Task Seed()
	{
        // Delete all
        await UnseedFiles();
        await this._dbService.DropAll();

        // Re seed
		SeedAnimalTypes();
		SeedBreeds();
        SeedUsersAndShelters();
        await SeedAnimals();
        await SeedFiles();
		return;
	}

    private List<T> ReadFromJson<T>(String fileName)
    {
        String path = Path.Combine("Seeders/TestData/Json", fileName);
        String json = System.IO.File.ReadAllText(path);
        return JsonConvert.DeserializeObject<List<T>>(json);
    }

    private void SeedAnimalTypes()
    {
        IMongoCollection<AnimalType> animalTypesCollection = this._dbService.GetCollection<AnimalType>();
        if (animalTypesCollection.CountDocuments(FilterDefinition<AnimalType>.Empty) == 0)
        {
            List<AnimalType> animalTypes = ReadFromJson<AnimalType>("animal-types.json");
            animalTypesCollection.InsertMany(animalTypes);
        }
    }

    private void SeedBreeds()
    {
        IMongoCollection<Breed> breedsCollection = this._dbService.GetCollection<Breed>();
        if (breedsCollection.CountDocuments(FilterDefinition<Breed>.Empty) == 0)
        {
            List<Breed> breeds = ReadFromJson<Breed>("breeds.json");
            breedsCollection.InsertMany(breeds);
        }
    }

    private void SeedUsersAndShelters()
    {
        IMongoCollection<User> usersCollection = this._dbService.GetCollection<User>();
        IMongoCollection<Shelter> sheltersCollection = this._dbService.GetCollection<Shelter>();

        if (usersCollection.CountDocuments(FilterDefinition<User>.Empty) == 0)
        {
            // Read users and shelters from JSON
            List<User> users = ReadFromJson<User>("users.json");
            foreach (User user in users)
                if (!String.IsNullOrEmpty(user.Password))
                    user.Password = Security.HashValue(user.Password);

            List<Shelter> shelters = ReadFromJson<Shelter>("shelters.json");

            // Insert all users and shelters into collections
            usersCollection.InsertMany(users);
            sheltersCollection.InsertMany(shelters);
        }
    }

    private async Task SeedFiles()
    {
        // Collections
        IMongoCollection<Animal> animalsCollection = _dbService.GetCollection<Animal>();
        IMongoCollection<User> usersCollection = _dbService.GetCollection<User>();
        IMongoCollection<Shelter> sheltersCollection = _dbService.GetCollection<Shelter>();
        IMongoCollection<Pawfect_API.Data.Entities.File> filesCollection = _dbService.GetCollection<Pawfect_API.Data.Entities.File>();

        List<Animal> animals = animalsCollection.Find(FilterDefinition<Animal>.Empty).ToList();
        List<User> users = usersCollection.Find(FilterDefinition<User>.Empty).ToList();
        List<Shelter> shelters = sheltersCollection.Find(FilterDefinition<Shelter>.Empty).ToList();

        if (!animals.Any())
            throw new Exception("No animals found to assign photos.");
        if (!users.Any())
            throw new Exception("No users found to assign profile pictures.");
        if (!shelters.Any())
            throw new Exception("No shelters found.");

        // Map ShelterId -> UserId
        Dictionary<String, String> shelterUserIds = shelters.ToDictionary(s => s.Id, s => s.UserId);

        List<Pawfect_API.Data.Entities.File> fileEntities = new List<Pawfect_API.Data.Entities.File>();

        // Directories
        String animalFilesDirectory = Path.Combine("Seeders/TestData", "Files", "Animals");
        String userFilesDirectory = Path.Combine("Seeders/TestData", "Files", "UserProfilePictures");

        // 1. Animals
        foreach (Animal animal in animals)
        {
            if (!shelterUserIds.TryGetValue(animal.ShelterId, out String ownerId))
            {
                Console.WriteLine($"No shelter user found for animal {animal.Name}, skipping.");
                continue;
            }

            // Look for file <animal.Name>.<ext>
            String[] matches = Directory.GetFiles(animalFilesDirectory, $"{animal.Name}.*");
            if (!matches.Any())
            {
                Console.WriteLine($"No image found for animal {animal.Name}, skipping.");
                continue;
            }

            List<String> photos = new List<String>();
            foreach (String filePath in matches)
            {
                String fileId = ObjectId.GenerateNewId().ToString();
                Pawfect_API.Data.Entities.File fileEntity = await this.UploadFile(filePath, fileId, ownerId);

                fileEntities.Add(fileEntity);
                photos.Add(fileEntity.Id);
            }

            // Update animal with photo IDs
            UpdateDefinition<Pawfect_API.Data.Entities.Animal> update = Builders<Pawfect_API.Data.Entities.Animal>.Update.Set(a => a.PhotosIds, photos);
            await animalsCollection.UpdateOneAsync(a => a.Id == animal.Id, update);
        }

        // 2. Users
        foreach (User user in users)
        {
            // Look for file <user.FullName>.<ext>
            String[] matches = Directory.GetFiles(userFilesDirectory, $"{user.FullName}.*");
            if (!matches.Any())
            {
                Console.WriteLine($"No image found for user {user.FullName}, skipping.");
                continue;
            }

            // Use first match (1 profile photo per user)
            String filePath = matches.First();
            String fileId = ObjectId.GenerateNewId().ToString();

            Pawfect_API.Data.Entities.File fileEntity = await this.UploadFile(filePath, fileId, ownerId: user.Id);
            fileEntities.Add(fileEntity);

            // Update user with profile photo ID
            UpdateDefinition<User> update = Builders<User>.Update.Set(u => u.ProfilePhotoId, fileId);
            await usersCollection.UpdateOneAsync(u => u.Id == user.Id, update);
        }

        // 3. Insert all files
        if (fileEntities.Any())
            await filesCollection.InsertManyAsync(fileEntities);
    }

    /// <summary>
    /// Upload a file to AWS and return File entity.
    /// </summary>
    private async Task<Pawfect_API.Data.Entities.File> UploadFile(String filePath, String fileId, String ownerId)
    {
        String fileName = Path.GetFileName(filePath);
        String extension = Path.GetExtension(fileName).ToLowerInvariant();
        String mimeType = _conventionService.ToMimeType(extension);
        String fileType = _conventionService.ToFileType(extension).ToString();

        Byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

        using MemoryStream memoryStream = new MemoryStream(fileBytes);
        IFormFile formFile = new FormFile(memoryStream, 0, fileBytes.Length, null, fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = mimeType,
        };

        String key = _awsService.ConstructAwsKey(fileName, Guid.NewGuid().ToString());
        String sourceUrl = await _awsService.UploadAsync(formFile, key);

        return new Pawfect_API.Data.Entities.File
        {
            Id = fileId,
            Filename = fileName,
            Size = fileBytes.Length,
            OwnerId = ownerId,
            MimeType = mimeType,
            FileType = fileType,
            SourceUrl = sourceUrl,
            AwsKey = key,
            AccessType = FileAccessType.Public,
            ContextId = null,
            ContextType = null,
            FileSaveStatus = FileSaveStatus.Permanent,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }


    private async Task SeedAnimals()
    {
        IMongoCollection<Animal> animalsCollection = this._dbService.GetCollection<Animal>();
        if (animalsCollection.CountDocuments(FilterDefinition<Animal>.Empty) == 0)
        {
            // Get collections for breed and animal type lookups
            IMongoCollection<Breed> breedsCollection = this._dbService.GetCollection<Breed>();
            IMongoCollection<AnimalType> animalTypesCollection = this._dbService.GetCollection<AnimalType>();

            // Load all breeds and animal types into memory for efficient lookup
            List<Breed> allBreeds = await breedsCollection.Find(FilterDefinition<Breed>.Empty).ToListAsync();
            List<AnimalType> allAnimalTypes = await animalTypesCollection.Find(FilterDefinition<AnimalType>.Empty).ToListAsync();

            // Create lookup dictionaries for better performance
            Dictionary<String, Breed> breedLookup = allBreeds.ToDictionary(b => b.Id, b => b);
            Dictionary<String, AnimalType> animalTypeLookup = allAnimalTypes.ToDictionary(at => at.Id, at => at);

            List<Animal> animals = ReadFromJson<Animal>("animals.json");

            foreach (Animal animal in animals)
            {
                // Get breed information
                Breed breed = breedLookup.TryGetValue(animal.BreedId, out Breed foundBreed)
                    ? foundBreed
                    : new Breed { Name = "Unknown", Description = "No breed information available" };

                // Get animal type information
                AnimalType animalType = animalTypeLookup.TryGetValue(animal.AnimalTypeId, out AnimalType foundAnimalType)
                    ? foundAnimalType
                    : new AnimalType { Name = "Unknown", Description = "No animal type information available" };

                // Create enhanced embedding with all information
                Pawfect_API.Models.Animal.AnimalSearchDataModel animalSearchingModel = new Pawfect_API.Models.Animal.AnimalSearchDataModel(
                        _config,
                        animal.Age,
                        animal.Gender,
                        animal.Description,
                        animal.Weight,
                        animal.HealthStatus,
                        breed.Name,
                        breed.Description,
                        animalType.Name,
                        animalType.Description
                );

                IEmbeddingValueExtractorAbstraction embeddingValueExtractorAbstracted = _embeddingValueExtractorFactory.ExtractorSafe(EmbeddingValueExtractorType.Animal);
                if (!(embeddingValueExtractorAbstracted is IEmbeddingValueExtractor<Pawfect_API.Models.Animal.AnimalSearchDataModel, String, String> &&
                    embeddingValueExtractorAbstracted is IAnimalEmbeddingValueExtractor animalEmbeddingValueExtractor))
                {
                    throw new InvalidOperationException("Invalid embedding value extractor for Animal type.");
                }

                animal.SemanticText = await animalEmbeddingValueExtractor.ExtractValue(animalSearchingModel);

                // Generate embedding with enhanced information
                animal.Embedding = (await _embeddingService.GenerateAggregatedEmbeddingAsyncDouble<String>(
                    new ChunkedEmbeddingInput<String>
                    {
                        Content = animal.SemanticText,
                        SourceId = animal.Id,
                        SourceType = nameof(Animal)
                    }
                )).Vector.ToArray();
            }

            animalsCollection.InsertMany(animals);
        }
    }

    private async Task UnseedFiles()
    {
        IMongoCollection<Pawfect_API.Data.Entities.File> filesCollection = this._dbService.GetCollection<Pawfect_API.Data.Entities.File>();

        // Retrieve all files
        List<Pawfect_API.Data.Entities.File> files = filesCollection.Find(FilterDefinition<Pawfect_API.Data.Entities.File>.Empty).ToList();
        if (files.Count == 0)  return;

        // Construct AWS keys for deletion
        List<String> keys = [..files.Select(file => file.AwsKey)];

        // Delete files from AWS
        Dictionary<String, Boolean> deletionResults = await _awsService.DeleteAsync(keys);

        // Log any failed deletions (optional, for debugging)
        List<KeyValuePair<String, Boolean>> failedDeletions = [.. deletionResults.Where(r => !r.Value)];
        if (failedDeletions.Any())
            throw new InvalidOperationException($"Failed to delete {failedDeletions.Count} files from AWS: {String.Join(", ", failedDeletions.Select(r => r.Key))}");

        // Delete all files from MongoDB
        await filesCollection.DeleteManyAsync(FilterDefinition<Pawfect_API.Data.Entities.File>.Empty);
    }
}