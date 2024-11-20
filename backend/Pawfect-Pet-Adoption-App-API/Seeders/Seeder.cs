using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Services;

public class Seeder
{
    private readonly MongoDbService dbService;

    public Seeder(MongoDbService dbService)
    {
        this.dbService = dbService;
    }

    public void Seed()
    {
        // Κάνε "Seed" τη βάση μόνο άν δεν΄έχει ήδη γίνει
        if (this.dbService.GetCollection<User>().CountDocuments(FilterDefinition<User>.Empty) == 0)
        {
            SeedAnimalTypes();
            SeedBreeds();
            SeedUsers();
            SeedShelters();
            SeedAnimals();
            SeedAdoptionApplications();
            SeedConversations();
            SeedMessages();
            SeedReports();
            SeedNotifications();
            return;
        }

        // Διέγραψε τη βάση μόνο άν έχει ήδη γίνει "Seed" ( Μόνο για Testing )
        this.dbService.DropAllCollections();

    }

    private void SeedAnimalTypes()
    {
        var animalTypesCollection = this.dbService.GetCollection<AnimalType>();
        if (animalTypesCollection.CountDocuments(FilterDefinition<AnimalType>.Empty) == 0)
        {
            var animalTypes = new List<AnimalType>
            {
                new AnimalType { Name = "Dog", Description = "Canine species" },
                new AnimalType { Name = "Cat", Description = "Feline species" },
                new AnimalType { Name = "Rabbit", Description = "Lagomorph species" }
            };
            animalTypesCollection.InsertMany(animalTypes);
        }
    }

    private void SeedBreeds()
    {
        var breedsCollection = this.dbService.GetCollection<Breed>();
        var animalTypesCollection = this.dbService.GetCollection<AnimalType>();
        var animalTypes = animalTypesCollection.Find(FilterDefinition<AnimalType>.Empty).ToList();

        if (breedsCollection.CountDocuments(FilterDefinition<Breed>.Empty) == 0)
        {
            var breeds = new List<Breed>
            {
                new Breed { Name = "Labrador", TypeId = animalTypes[0].Id, Description = "Friendly and outgoing" },
                new Breed { Name = "Persian", TypeId = animalTypes[1].Id, Description = "Long-haired breed" },
                new Breed { Name = "Holland Lop", TypeId = animalTypes[2].Id, Description = "Popular rabbit breed" }
            };
            breedsCollection.InsertMany(breeds);
        }
    }

    private void SeedUsers()
    {
        var usersCollection = this.dbService.GetCollection<User>();
        if (usersCollection.CountDocuments(FilterDefinition<User>.Empty) == 0)
        {
            var users = new List<User>
            {
                new User { Email = "user1@example.com", Password = "PanosPan7!", FullName = "John Doe", Role = UserRole.User, Phone = "1234567890", Location = new Location { Address = "123 Main St", City = "CityA", Number = "1", ZipCode = "12345" }, AuthProvider = AuthProvider.Local },
                new User { Email = "user2@example.com", FullName = "Jane Smith", Role = UserRole.Admin, Phone = "0987654321", Location = new Location { Address = "456 Market St", City = "CityB", Number = "2", ZipCode = "67890" }, AuthProvider = AuthProvider.Google, AuthProviderId = "google_id123" },
                new User { Email = "user3@example.com", Password = "PanosPan7!",  FullName = "Alice Brown", Role = UserRole.Shelter, Phone = "1122334455", Location = new Location { Address = "789 Broad St", City = "CityC", Number = "3", ZipCode = "54321" }, AuthProvider = AuthProvider.Local }
            };
            usersCollection.InsertMany(users);
        }
    }

    private void SeedShelters()
    {
        var sheltersCollection = this.dbService.GetCollection<Shelter>();
        var usersCollection = this.dbService.GetCollection<User>();
        var shelterUsers = usersCollection.Find(u => u.Role == UserRole.Shelter).ToList();

        if (sheltersCollection.CountDocuments(FilterDefinition<Shelter>.Empty) == 0)
        {
            var shelters = new List<Shelter>
            {
                new Shelter { UserId = shelterUsers[0].Id, ShelterName = "Happy Paws"
                , Description = "Caring for animals in need"
                , VerificationStatus = VerificationStatus.Verified },
            };
            sheltersCollection.InsertMany(shelters);
        }

        Shelter newShelter = sheltersCollection.Find(sh => sh.UserId == shelterUsers[0].Id).FirstOrDefault();

        // Define the filter to find the user by ID
        var filter = Builders<User>.Filter.Eq(u => u.Id, newShelter.UserId);

        // Define the update operation to set the new email
        var update = Builders<User>.Update.Set(u => u.ShelterId, newShelter.Id);

        // Execute the update operation
        usersCollection.UpdateOne(filter, update);
    }

    private void SeedAnimals()
    {
        var animalsCollection = this.dbService.GetCollection<Animal>();
        var breedsCollection = this.dbService.GetCollection<Breed>();
        var sheltersCollection = this.dbService.GetCollection<Shelter>();
        var animalTypesCollection = this.dbService.GetCollection<AnimalType>();

        var breeds = breedsCollection.Find(FilterDefinition<Breed>.Empty).ToList();
        var shelters = sheltersCollection.Find(FilterDefinition<Shelter>.Empty).ToList();
        var animalTypes = animalTypesCollection.Find(FilterDefinition<AnimalType>.Empty).ToList();

        if (animalsCollection.CountDocuments(FilterDefinition<Animal>.Empty) == 0)
        {
            var animals = new List<Animal>
            {
                new Animal { Name = "Buddy", Age = 3, Gender = Gender.Male, Description = "Friendly and playful", Weight = 25, HealthStatus = "Good"
                , ShelterId = shelters[0].Id, BreedId = breeds[0].Id, TypeId = animalTypes[0].Id, AdoptionStatus = AdoptionStatus.Available
                , Photos = new[] { "https://cdn.pixabay.com/photo/2016/03/27/21/06/dog-1280185_1280.jpg",
                                    "https://images.unsplash.com/photo-1574158622681-2d41c4939f82",
                                    "https://cdn.pixabay.com/photo/2020/06/10/16/28/dog-5360330_1280.jpg" } },
                new Animal { Name = "Mittens", Age = 2, Gender = Gender.Female, Description = "Calm and affectionate", Weight = 10, HealthStatus = "Good",
                    ShelterId = shelters[0].Id, BreedId = breeds[1].Id, TypeId = animalTypes[1].Id, AdoptionStatus = AdoptionStatus.Available
                    , Photos = new[] { "https://cdn.pixabay.com/photo/2016/03/27/21/06/dog-1280185_1280.jpg",
                                    "https://images.unsplash.com/photo-1574158622681-2d41c4939f82",
                                    "https://cdn.pixabay.com/photo/2020/06/10/16/28/dog-5360330_1280.jpg" } },
                new Animal { Name = "Thumper", Age = 1, Gender = Gender.Male, Description = "Energetic and curious", Weight = 3, HealthStatus = "Excellent",
                    ShelterId = shelters[0].Id, BreedId = breeds[2].Id, TypeId = animalTypes[2].Id, AdoptionStatus = AdoptionStatus.Available,
                    Photos = new[] { "https://cdn.pixabay.com/photo/2016/03/27/21/06/dog-1280185_1280.jpg",
                                    "https://images.unsplash.com/photo-1574158622681-2d41c4939f82",
                                    "https://cdn.pixabay.com/photo/2020/06/10/16/28/dog-5360330_1280.jpg" } }
            };
            animalsCollection.InsertMany(animals);
        }
    }

    private void SeedAdoptionApplications()
    {
        var applicationsCollection = this.dbService.GetCollection<AdoptionApplication>();
        var usersCollection = this.dbService.GetCollection<User>();
        var animalsCollection = this.dbService.GetCollection<Animal>();

        var users = usersCollection.Find(u => u.Role == UserRole.User).ToList();
        var animals = animalsCollection.Find(FilterDefinition<Animal>.Empty).ToList();

        if (applicationsCollection.CountDocuments(FilterDefinition<AdoptionApplication>.Empty) == 0)
        {
            var applications = new List<AdoptionApplication>
            {
                new AdoptionApplication { UserId = users[0].Id, AnimalId = animals[0].Id, ShelterId = animals[0].ShelterId, ApplicationDetails = "Loves pets and has a spacious home." },
                new AdoptionApplication { UserId = users[0].Id, AnimalId = animals[0].Id, ShelterId = animals[1].ShelterId, ApplicationDetails = "Experienced with pets, lives in a quiet neighborhood." },
                new AdoptionApplication { UserId = users[0].Id, AnimalId = animals[0].Id, ShelterId = animals[2].ShelterId, ApplicationDetails = "Works from home, has time to care for pets." }
            };
            applicationsCollection.InsertMany(applications);
        }
    }

    private void SeedConversations()
    {
        var conversationsCollection = this.dbService.GetCollection<Conversation>();
        var usersCollection = this.dbService.GetCollection<User>();
        var animalsCollection = this.dbService.GetCollection<Animal>();

        var users = usersCollection.Find(FilterDefinition<User>.Empty).ToList();
        var animals = animalsCollection.Find(FilterDefinition<Animal>.Empty).ToList();

        if (conversationsCollection.CountDocuments(FilterDefinition<Conversation>.Empty) == 0)
        {
            var conversations = new List<Conversation>
            {
                new Conversation { UserIds = new List<string> { users[0].Id, users[1].Id }, AnimalId = animals[0].Id },
                new Conversation { UserIds = new List<string> { users[1].Id, users[2].Id }, AnimalId = animals[1].Id },
                new Conversation { UserIds = new List<string> { users[2].Id, users[0].Id }, AnimalId = animals[2].Id }
            };
            conversationsCollection.InsertMany(conversations);
        }
    }

    private void SeedMessages()
    {
        var messagesCollection = this.dbService.GetCollection<Message>();
        var conversationsCollection = this.dbService.GetCollection<Conversation>();
        var conversations = conversationsCollection.Find(FilterDefinition<Conversation>.Empty).ToList();

        if (messagesCollection.CountDocuments(FilterDefinition<Message>.Empty) == 0)
        {
            var messages = new List<Message>
            {
                new Message { ConversationId = conversations[0].Id, SenderId = conversations[0].UserIds[0], RecepientId = conversations[0].UserIds[1], Content = "Hello, I'm interested in adopting Buddy." },
                new Message { ConversationId = conversations[1].Id, SenderId = conversations[1].UserIds[0], RecepientId = conversations[1].UserIds[1], Content = "Is Mittens still available?" },
                new Message { ConversationId = conversations[2].Id, SenderId = conversations[2].UserIds[0], RecepientId = conversations[2].UserIds[1], Content = "How is Thumper's health?" }
            };
            messagesCollection.InsertMany(messages);
        }
    }

    private void SeedReports()
    {
        var reportsCollection = this.dbService.GetCollection<Report>();
        var usersCollection = this.dbService.GetCollection<User>();
        var users = usersCollection.Find(FilterDefinition<User>.Empty).ToList();

        if (reportsCollection.CountDocuments(FilterDefinition<Report>.Empty) == 0)
        {
            var reports = new List<Report>
            {
                new Report { ReporterId = users[0].Id, ReportedId = users[1].Id, Type = ReportType.InAppropriateMessage, Reason = "Inappropriate content in message" },
                new Report { ReporterId = users[1].Id, ReportedId = users[2].Id, Type = ReportType.Other, Reason = "Suspicious activity" },
                new Report { ReporterId = users[2].Id, ReportedId = users[0].Id, Type = ReportType.InAppropriateMessage, Reason = "Offensive language" }
            };
            reportsCollection.InsertMany(reports);
        }
    }

    private void SeedNotifications()
    {
        var notificationsCollection = this.dbService.GetCollection<Notification>();
        var usersCollection = this.dbService.GetCollection<User>();
        var users = usersCollection.Find(FilterDefinition<User>.Empty).ToList();

        if (notificationsCollection.CountDocuments(FilterDefinition<Notification>.Empty) == 0)
        {
            var notifications = new List<Notification>
            {
                new Notification { UserId = users[0].Id, Type = NotificationType.IncomingMessage, Content = "You have a new message about Buddy." },
                new Notification { UserId = users[1].Id, Type = NotificationType.AdoptionApplication, Content = "Your application for Mittens is under review." },
                new Notification { UserId = users[2].Id, Type = NotificationType.Report, Content = "You have been reported for inappropriate content." }
            };
            notificationsCollection.InsertMany(notifications);
        }
    }
}
