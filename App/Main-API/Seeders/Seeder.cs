using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Main_API.Data.Entities;
using Main_API.Data.Entities.EnumTypes;
using Main_API.DevTools;
using Main_API.Services.AwsServices;
using Main_API.Services.Convention;
using Main_API.Services.MongoServices;
using System.Threading.Tasks;

public class Seeder
{
	private readonly MongoDbService _dbService;
    private readonly IAwsService _awsService;
    private readonly IConventionService _conventionService;

    public Seeder
    (
        MongoDbService dbService, 
        IAwsService awsService,
        IConventionService conventionService
    )
	{
		this._dbService = dbService;
        this._awsService = awsService;
        this._conventionService = conventionService;
    }

	public async Task Seed()
	{
        // Delete all
        await UnseedFiles();
        this._dbService.DropAllCollections();

        // Re seed
		SeedAnimalTypes();
		SeedBreeds();
        SeedUsersAndShelters();
        SeedAnimals();
        await SeedFiles();
        SeedAdoptionApplications();
		SeedConversations();
		SeedMessages();
		SeedReports();
		SeedNotifications();
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
        // Get all animals, shelters, and users
        IMongoCollection<Animal> animalsCollection = this._dbService.GetCollection<Animal>();
        IMongoCollection<Shelter> sheltersCollection = this._dbService.GetCollection<Shelter>();
        IMongoCollection<User> usersCollection = this._dbService.GetCollection<User>();
        IMongoCollection<Main_API.Data.Entities.File> filesCollection = this._dbService.GetCollection<Main_API.Data.Entities.File>();

        // Seed animal photos
        List<Animal> animals = animalsCollection.Find(FilterDefinition<Animal>.Empty).ToList();
        if (animals.Count == 0)
            throw new Exception("No animals found to assign photos.");

        // Get shelters and create a lookup for UserId by ShelterId
        List<Shelter> shelters = sheltersCollection.Find(FilterDefinition<Shelter>.Empty).ToList();
        Dictionary<String, String> shelterUserIds = shelters.ToDictionary(s => s.Id, s => s.UserId);

        // Get animal files from the directory
        String animalFilesDirectory = Path.Combine("Seeders/TestData", "Files", "Animals");
        String[] animalFilePaths = Directory.GetFiles(animalFilesDirectory);
        if (animalFilePaths.Length < 2 * animals.Count)
            throw new Exception($"Not enough photos in {animalFilesDirectory}. Need {2 * animals.Count}, found {animalFilePaths.Length}.");

        List<Main_API.Data.Entities.File> fileEntities = new List<Main_API.Data.Entities.File>();
        Dictionary<String, List<String>> animalPhotoIds = new Dictionary<String, List<String>>();
        int animalFileIndex = 0;
        foreach (Animal animal in animals)
        {
            if (!shelterUserIds.TryGetValue(animal.ShelterId, out String ownerId))
                throw new Exception($"No shelter found for animal {animal.Id} with ShelterId {animal.ShelterId}.");

            List<String> photoIds = new List<String>();
            for (int i = 0; i < 2; i++)
            {
                // Cycle through files if we run out
                String filePath = animalFilePaths[animalFileIndex % animalFilePaths.Length];
                animalFileIndex++;

                String fileName = Path.GetFileName(filePath);
                String fileId = ObjectId.GenerateNewId().ToString();
                String extension = Path.GetExtension(fileName).ToLowerInvariant();
                String mimeType = _conventionService.ToMimeType(extension);
                String fileType = _conventionService.ToFileType(extension).ToString();
                // Read the file into a stream and create an IFormFile
                using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                FormFile formFile = new FormFile(stream, 0, stream.Length, null, fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = mimeType
                };

                // Construct the AWS key and upload the file
                String key = _awsService.ConstructAwsKey(fileName, Guid.NewGuid().ToString());
                String sourceUrl = await _awsService.UploadAsync(formFile, key);

                // Create the File entity
                Main_API.Data.Entities.File fileEntity = new Main_API.Data.Entities.File
                {
                    Id = fileId,
                    Filename = fileName,
                    Size = (double)stream.Length,
                    OwnerId = ownerId,
                    MimeType = mimeType,
                    FileType = fileType,
                    SourceUrl = sourceUrl,
                    AwsKey = key,
                    FileSaveStatus = FileSaveStatus.Permanent,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                fileEntities.Add(fileEntity);
                photoIds.Add(fileId);
            }

            animalPhotoIds[animal.Id] = photoIds;
        }

        // Seed user profile pictures
        List<User> users = usersCollection.Find(FilterDefinition<User>.Empty).ToList();
        if (users.Count < 5)
            throw new Exception($"Not enough users in the database. Need at least 5, found {users.Count}.");

        // Get user profile picture files from the directory
        String userFilesDirectory = Path.Combine("Seeders/TestData", "Files", "UserProfilePictures");
        String[] userFilePaths = Directory.GetFiles(userFilesDirectory);
        if (userFilePaths.Length < 5)
            throw new Exception($"Not enough profile pictures in {userFilesDirectory}. Need at least 5, found {userFilePaths.Length}.");

        // Select 5 random users
        Random random = new Random();
        List<User> selectedUsers = [.. users.OrderBy(x => random.Next()).Take(5)];
        int userFileIndex = 0;

        foreach (User user in selectedUsers)
        {
            // Cycle through files if we run out
            String filePath = userFilePaths[userFileIndex % userFilePaths.Length];
            userFileIndex++;

            String fileName = Path.GetFileName(filePath);
            String fileId = ObjectId.GenerateNewId().ToString();
            String extension = Path.GetExtension(fileName).ToLowerInvariant();
            String mimeType = _conventionService.ToMimeType(extension);
            String fileType = _conventionService.ToFileType(extension).ToString();
            // Read the file into a stream and create an IFormFile
            using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            FormFile formFile = new FormFile(stream, 0, stream.Length, null, fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = mimeType
            };

            // Construct the AWS key and upload the file
            String key = _awsService.ConstructAwsKey(fileName, Guid.NewGuid().ToString());
            String sourceUrl = await _awsService.UploadAsync(formFile, key);

            // Create the File entity for the user profile picture
            Main_API.Data.Entities.File fileEntity = new Main_API.Data.Entities.File
            {
                Id = fileId,
                Filename = fileName,
                Size = (double)stream.Length,
                OwnerId = user.Id,
                MimeType = mimeType,
                FileType = fileType,
                SourceUrl = sourceUrl,
                AwsKey = key,
                FileSaveStatus = FileSaveStatus.Permanent,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            fileEntities.Add(fileEntity);

            // Update the user's profile picture reference (assuming User has a ProfilePictureId field)
            UpdateDefinition<User> update = Builders<User>.Update.Set(u => u.ProfilePhotoId, fileId);
            await usersCollection.UpdateOneAsync(u => u.Id == user.Id, update);
        }

        // Insert all file entities into the database
        if (fileEntities.Any())
        {
            await filesCollection.InsertManyAsync(fileEntities);
        }

        // Update animals with their photo IDs
        foreach (KeyValuePair<String, List<String>> animal in animalPhotoIds)
        {
            UpdateDefinition<Animal> update = Builders<Animal>.Update.Set(a => a.PhotosIds, animal.Value);
            await animalsCollection.UpdateOneAsync(a => a.Id == animal.Key, update);
        }
    }
    private void SeedAnimals()
    {
        IMongoCollection<Animal> animalsCollection = this._dbService.GetCollection<Animal>();
        if (animalsCollection.CountDocuments(FilterDefinition<Animal>.Empty) == 0)
        {
            List<Animal> animals = ReadFromJson<Animal>("animals.json");
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

    private void SeedConversations()
    {
        IMongoCollection<Conversation> conversationsCollection = this._dbService.GetCollection<Conversation>();
        if (conversationsCollection.CountDocuments(FilterDefinition<Conversation>.Empty) == 0)
        {
            List<Conversation> conversations = ReadFromJson<Conversation>("conversations.json");
            conversationsCollection.InsertMany(conversations);
        }
    }

    private void SeedMessages()
    {
        IMongoCollection<Message> messagesCollection = this._dbService.GetCollection<Message>();
        if (messagesCollection.CountDocuments(FilterDefinition<Message>.Empty) == 0)
        {
            List<Message> messages = ReadFromJson<Message>("messages.json");
            messagesCollection.InsertMany(messages);
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

    private void SeedNotifications()
    {
        IMongoCollection<Notification> notificationsCollection = this._dbService.GetCollection<Notification>();
        if (notificationsCollection.CountDocuments(FilterDefinition<Notification>.Empty) == 0)
        {
            List<Notification> notifications = ReadFromJson<Notification>("notifications.json");
            notificationsCollection.InsertMany(notifications);
        }
    }

    private async Task UnseedFiles()
    {
        IMongoCollection<Main_API.Data.Entities.File> filesCollection = this._dbService.GetCollection<Main_API.Data.Entities.File>();

        // Retrieve all files
        List<Main_API.Data.Entities.File> files = filesCollection.Find(FilterDefinition<Main_API.Data.Entities.File>.Empty).ToList();
        if (files.Count == 0)  return;

        // Construct AWS keys for deletion
        List<String> keys = [..files.Select(file => file.AwsKey)];

        // Delete files from AWS
        Dictionary<String, Boolean> deletionResults = await _awsService.DeleteAsync(keys);

        // Log any failed deletions (optional, for debugging)
        List<KeyValuePair<String, Boolean>> failedDeletions = [.. deletionResults.Where(r => !r.Value)];
        if (failedDeletions.Any())
            Console.WriteLine($"Failed to delete {failedDeletions.Count} files from AWS: {String.Join(", ", failedDeletions.Select(r => r.Key))}");

        // Delete all files from MongoDB
        await filesCollection.DeleteManyAsync(FilterDefinition<Main_API.Data.Entities.File>.Empty);
    }
}