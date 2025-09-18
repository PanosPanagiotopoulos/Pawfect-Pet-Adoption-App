using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Pawfect_API.Data.Entities;
using Pawfect_API.Data.Entities.EnumTypes;
using Pawfect_API.DevTools;
using Pawfect_API.Services.AwsServices;
using Pawfect_API.Services.Convention;
using Pawfect_API.Services.MongoServices;
using Pawfect_Pet_Adoption_App_API.Services.EmbeddingServices;
using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Mongo;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Services.EmbeddingServices.AnimalEmbeddingValueExtractors;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Embedding;
using Amazon.Runtime.Internal.Util;
using System.IO.Abstractions;

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
        SeedAdoptionApplications();
		SeedReports();
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

            // Get users with UserRole.Shelter
            List<User> shelterUsers = users.Where(u => u.Roles.Contains(UserRole.Shelter)).ToList();

            // Pair each shelter with a shelter-role user
            for (int i = 0; i < shelters.Count && i < shelterUsers.Count; i++)
            {
                Shelter shelter = shelters[i];
                User user = shelterUsers[i];

                // Update shelter's UserId to match the user's _id
                shelter.UserId = user.Id;

                // Update user's ShelterId to match the shelter's _id
                user.ShelterId = shelter.Id;
            }

            // Insert all users and shelters into collections
            usersCollection.InsertMany(users);
            sheltersCollection.InsertMany(shelters);
        }
    }

    private async Task SeedFiles()
    {
        // Collections
        var animalsCollection = _dbService.GetCollection<Animal>();
        var sheltersCollection = _dbService.GetCollection<Shelter>();
        var usersCollection = _dbService.GetCollection<User>();
        var filesCollection = _dbService.GetCollection<Pawfect_API.Data.Entities.File>();

        var animals = animalsCollection.Find(FilterDefinition<Animal>.Empty).ToList();
        if (animals.Count == 0)
            throw new Exception("No animals found to assign photos.");

        var shelters = sheltersCollection.Find(FilterDefinition<Shelter>.Empty).ToList();
        var shelterUserIds = shelters.ToDictionary(s => s.Id, s => s.UserId);

        var random = new Random();
        var fileEntities = new List<Pawfect_API.Data.Entities.File>();

        string animalFilesDirectory = Path.Combine("Seeders/TestData", "Files", "Animals");
        string[] animalFilePaths = Directory.GetFiles(animalFilesDirectory);

        var uploadedAnimalFiles = new List<Pawfect_API.Data.Entities.File>();

        foreach (string filePath in animalFilePaths)
        {
            string fileName = Path.GetFileName(filePath);
            string fileId = ObjectId.GenerateNewId().ToString();
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            string mimeType = _conventionService.ToMimeType(extension);
            string fileType = _conventionService.ToFileType(extension).ToString();

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

            using MemoryStream memoryStream = new MemoryStream(fileBytes);
            FormFile formFile = new FormFile(memoryStream, 0, fileBytes.Length, null, fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = mimeType
            };

            string key = _awsService.ConstructAwsKey(fileName, Guid.NewGuid().ToString());
            string sourceUrl = await _awsService.UploadAsync(formFile, key);

            var fileEntity = new Pawfect_API.Data.Entities.File
            {
                Id = fileId,
                Filename = fileName,
                Size = fileBytes.Length,
                OwnerId = null, 
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

            uploadedAnimalFiles.Add(fileEntity);
        }

        fileEntities.AddRange(uploadedAnimalFiles);

        var animalPhotoIds = new Dictionary<string, List<string>>();

        foreach (var animal in animals)
        {
            if (!shelterUserIds.TryGetValue(animal.ShelterId, out string ownerId))
                throw new Exception($"No shelter found for animal {animal.Id} with ShelterId {animal.ShelterId}.");

            // Pick 2 random photos per animal
            var photos = uploadedAnimalFiles.OrderBy(_ => random.Next()).Take(2).ToList();

            foreach (var photo in photos)
            {
                // update OwnerId only when assigned
                photo.OwnerId ??= ownerId;
            }

            animalPhotoIds[animal.Id] = photos.Select(p => p.Id).ToList();
        }

        var users = usersCollection.Find(FilterDefinition<User>.Empty).ToList();
        if (users.Count < 5)
            throw new Exception($"Not enough users in the database. Need at least 5, found {users.Count}.");

        string userFilesDirectory = Path.Combine("Seeders/TestData", "Files", "UserProfilePictures");
        string[] userFilePaths = Directory.GetFiles(userFilesDirectory);
        if (userFilePaths.Length < 5)
            throw new Exception($"Not enough profile pictures in {userFilesDirectory}. Need at least 5, found {userFilePaths.Length}.");

        var selectedUsers = users.OrderBy(x => random.Next()).Take(5).ToList();
        int userFileIndex = 0;

        foreach (var user in selectedUsers)
        {
            string filePath = userFilePaths[userFileIndex % userFilePaths.Length];
            userFileIndex++;

            string fileName = Path.GetFileName(filePath);
            string fileId = ObjectId.GenerateNewId().ToString();
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            string mimeType = _conventionService.ToMimeType(extension);
            string fileType = _conventionService.ToFileType(extension).ToString();

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

            using MemoryStream memoryStream = new MemoryStream(fileBytes);
            FormFile formFile = new FormFile(memoryStream, 0, fileBytes.Length, null, fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = mimeType
            };

            string key = _awsService.ConstructAwsKey(fileName, Guid.NewGuid().ToString());
            string sourceUrl = await _awsService.UploadAsync(formFile, key);

            var fileEntity = new Pawfect_API.Data.Entities.File
            {
                Id = fileId,
                Filename = fileName,
                Size = fileBytes.Length,
                OwnerId = user.Id,
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

            fileEntities.Add(fileEntity);

            var update = Builders<User>.Update.Set(u => u.ProfilePhotoId, fileId);
            await usersCollection.UpdateOneAsync(u => u.Id == user.Id, update);
        }

        // 🔹 Step 4: Save everything
        if (fileEntities.Any())
            await filesCollection.InsertManyAsync(fileEntities);

        foreach (var animal in animalPhotoIds)
        {
            var update = Builders<Animal>.Update.Set(a => a.PhotosIds, animal.Value);
            await animalsCollection.UpdateOneAsync(a => a.Id == animal.Key, update);
        }
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
                AnimalSearchDataModel animalSearchingModel = new AnimalSearchDataModel(
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
                if (!(embeddingValueExtractorAbstracted is IEmbeddingValueExtractor<AnimalSearchDataModel, String, String> &&
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

    private void SeedAdoptionApplications()
	{
        IMongoCollection<AdoptionApplication> adoptionApplicationCollection = this._dbService.GetCollection<AdoptionApplication>();
        if (adoptionApplicationCollection.CountDocuments(FilterDefinition<AdoptionApplication>.Empty) == 0)
        {
            List<AdoptionApplication> adoptionApplications = ReadFromJson<AdoptionApplication>("adoption-applications.json");
            adoptionApplicationCollection.InsertMany(adoptionApplications);
        }
    }
    private void SeedReports()
    {
        IMongoCollection<Report> reportsCollection = this._dbService.GetCollection<Report>();
        if (reportsCollection.CountDocuments(FilterDefinition<Report>.Empty) == 0)
        {
            List<Report> reports = ReadFromJson<Report>("reports.json");
            reportsCollection.InsertMany(reports);
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