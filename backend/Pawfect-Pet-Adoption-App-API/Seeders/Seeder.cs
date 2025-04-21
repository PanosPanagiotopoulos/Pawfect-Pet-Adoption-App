using MongoDB.Driver;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;

public class Seeder
{
	private readonly MongoDbService dbService;

	public Seeder(MongoDbService dbService)
	{
		this.dbService = dbService;
	}

	public void Seed()
	{
		
		this.dbService.DropAllCollections();

		SeedAnimalTypes();
		SeedBreeds();
        SeedUsersAndShelters();
		SeedAnimals();
		SeedAdoptionApplications();
		SeedConversations();
		SeedMessages();
		SeedReports();
		SeedNotifications();
		return;


	}

	private void SeedAnimalTypes()
	{
		var animalTypesCollection = this.dbService.GetCollection<AnimalType>();
		if (animalTypesCollection.CountDocuments(FilterDefinition<AnimalType>.Empty) == 0)
		{
			var animalTypes = new List<AnimalType>
			{
				new AnimalType { Name = "Dog", Description = "Canine species", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
				new AnimalType { Name = "Cat", Description = "Feline species", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
				new AnimalType { Name = "Rabbit", Description = "Lagomorph species", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
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
				new Breed { Name = "Labrador", TypeId = animalTypes[0].Id, Description = "Friendly and outgoing", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
				new Breed { Name = "Persian", TypeId = animalTypes[1].Id, Description = "Long-haired breed", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
				new Breed { Name = "Holland Lop", TypeId = animalTypes[2].Id, Description = "Popular rabbit breed", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
			};
			breedsCollection.InsertMany(breeds);
		}
	}

	private void SeedUsersAndShelters()
	{
		var usersCollection = this.dbService.GetCollection<User>();
		var sheltersCollection = this.dbService.GetCollection<Shelter>();

		if (usersCollection.CountDocuments(FilterDefinition<User>.Empty) == 0)
		{
			var users = new List<User>
		{
			new User { Email = "user1@example.com", Password = Security.HashValue("password1"), FullName = "User 1", Role = UserRole.User, Phone = "1234567890", Location = new Location { Address = "123 Main St", City = "CityA", Number = "1", ZipCode = "12345" }, AuthProvider = AuthProvider.Local, IsVerified = true, HasPhoneVerified = true, HasEmailVerified = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
			new User { Email = "user2@example.com", Password = Security.HashValue("password2"), FullName = "User 2", Role = UserRole.User, Phone = "2345678901", Location = new Location { Address = "123 Main St", City = "CityA", Number = "1", ZipCode = "12345" }, AuthProvider = AuthProvider.Local, IsVerified = true, HasPhoneVerified = true, HasEmailVerified = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
			new User { Email = "user3@example.com", Password = Security.HashValue("password3"), FullName = "User 3", Role = UserRole.User, Phone = "3456789012", Location = new Location { Address = "123 Main St", City = "CityA", Number = "1", ZipCode = "12345" }, AuthProvider = AuthProvider.Local, IsVerified = true, HasPhoneVerified = true, HasEmailVerified = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
			new User { Email = "user4@example.com", Password = Security.HashValue("password4"), FullName = "User 4", Role = UserRole.User, Phone = "4567890123", Location = new Location { Address = "123 Main St", City = "CityA", Number = "1", ZipCode = "12345" }, AuthProvider = AuthProvider.Local, IsVerified = true, HasPhoneVerified = true, HasEmailVerified = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
			new User { Email = "user5@example.com", Password = Security.HashValue("password5"), FullName = "User 5", Role = UserRole.User, Phone = "5678901234", Location = new Location { Address = "123 Main St", City = "CityA", Number = "1", ZipCode = "12345" }, AuthProvider = AuthProvider.Local, IsVerified = true, HasPhoneVerified = true, HasEmailVerified = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
		};
			usersCollection.InsertMany(users);

			var shelters = new List<Shelter>
		{
			new Shelter { UserId = users[1].Id, ShelterName = "Shelter 2", Description = "Shelter 2 description", VerificationStatus = VerificationStatus.Verified },
			new Shelter { UserId = users[3].Id, ShelterName = "Shelter 4", Description = "Shelter 4 description", VerificationStatus = VerificationStatus.Verified }
		};
			sheltersCollection.InsertMany(shelters);

			// Update users to have the role of Shelter and know their ShelterId
			for (int i = 0; i < users.Count; i++)
			{
				if (i % 2 != 0) // Odd position
				{
					var shelter = shelters.First(s => s.UserId == users[i].Id);
					var update = Builders<User>.Update
						.Set(u => u.Role, UserRole.Shelter)
						.Set(u => u.ShelterId, shelter.Id);
					usersCollection.UpdateOne(u => u.Id == users[i].Id, update);
				}
			}
		}
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
            new Animal {
                Name = "Max",
                Age = 4,
                Gender = Gender.Male,
                Description = "Loyal and energetic companion",
                Weight = 30,
                HealthStatus = "Good",
                ShelterId = shelters[0].Id,
                BreedId = breeds[0].Id,
                AnimalTypeId = animalTypes[0].Id,
                AdoptionStatus = AdoptionStatus.Available,
                PhotosIds = new List<string> {
                    "https://images.unsplash.com/photo-1558788353-f76d92427f16?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=60",
                    "https://cdn.pixabay.com/photo/2017/09/25/13/12/dog-2785074_1280.jpg",
                    "https://images.unsplash.com/photo-1518717758536-85ae29035b6d?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=60"
                }, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
			},
            new Animal {
                Name = "Bella",
                Age = 3,
                Gender = Gender.Female,
                Description = "Affectionate and playful, loves cuddles",
                Weight = 20,
                HealthStatus = "Good",
                ShelterId = shelters[0].Id,
                BreedId = breeds[1].Id,
                AnimalTypeId = animalTypes[1].Id,
                AdoptionStatus = AdoptionStatus.Available,
                PhotosIds = new List<string> {
                    "https://images.unsplash.com/photo-1601758123927-39d86b14d886?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=60",
                    "https://cdn.pixabay.com/photo/2018/01/15/07/51/dog-3081774_1280.jpg",
                    "https://images.unsplash.com/photo-1568572933382-74d440642117?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=60"
                }, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
			},
            new Animal {
                Name = "Charlie",
                Age = 5,
                Gender = Gender.Male,
                Description = "Friendly and intelligent, loves outdoor adventures",
                Weight = 28,
                HealthStatus = "Good",
                ShelterId = shelters[0].Id,
                BreedId = breeds[2].Id,
                AnimalTypeId = animalTypes[2].Id,
                AdoptionStatus = AdoptionStatus.Available,
                PhotosIds = new List<string> {
                    "https://images.unsplash.com/photo-1574158622681-2d41c4939f82?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=60",
                    "https://cdn.pixabay.com/photo/2019/06/18/16/05/dog-4284419_1280.jpg",
                    "https://images.unsplash.com/photo-1558788353-f76d92427f16?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=60"
                }, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
			},
            new Animal {
                Name = "Luna",
                Age = 2,
                Gender = Gender.Female,
                Description = "Gentle and graceful with a mysterious charm",
                Weight = 15,
                HealthStatus = "Excellent",
                ShelterId = shelters[0].Id,
                BreedId = breeds[0].Id,
                AnimalTypeId = animalTypes[1].Id,
                AdoptionStatus = AdoptionStatus.Available,
                PhotosIds = new List<string> {
                    "https://cdn.pixabay.com/photo/2016/02/19/10/00/animal-1209728_1280.jpg",
                    "https://images.unsplash.com/photo-1570018145413-1f6206eeabdb?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=60",
                    "https://cdn.pixabay.com/photo/2017/11/09/21/41/cat-2934720_1280.jpg"
                }, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
			},
            new Animal {
                Name = "Rocky",
                Age = 6,
                Gender = Gender.Male,
                Description = "Tough and resilient, yet full of heart",
                Weight = 35,
                HealthStatus = "Fair",
                ShelterId = shelters[0].Id,
                BreedId = breeds[2].Id,
                AnimalTypeId = animalTypes[1].Id,
                AdoptionStatus = AdoptionStatus.Available,
                PhotosIds = new List<string> {
                    "https://cdn.pixabay.com/photo/2018/05/07/22/09/dog-3383863_1280.jpg",
                    "https://images.unsplash.com/photo-1583511655623-caa2e58d0d69?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=60",
                    "https://cdn.pixabay.com/photo/2018/05/07/22/09/dog-3383863_1280.jpg"
                }, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
			},
            new Animal {
                Name = "Milo",
                Age = 3,
                Gender = Gender.Male,
                Description = "Curious and playful, always up for an adventure",
                Weight = 22,
                HealthStatus = "Good",
                ShelterId = shelters[0].Id,
                BreedId = breeds[2].Id,
                AnimalTypeId = animalTypes[0].Id,
                AdoptionStatus = AdoptionStatus.Available,
                PhotosIds = new List<string> {
                    "https://cdn.pixabay.com/photo/2017/11/11/21/46/dog-2944273_1280.jpg",
                    "https://images.unsplash.com/photo-1560807707-8cc77767d783?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=60",
                    "https://cdn.pixabay.com/photo/2017/11/11/21/46/dog-2944273_1280.jpg"
                }, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
			},
            new Animal {
                Name = "Lucy",
                Age = 4,
                Gender = Gender.Female,
                Description = "Sweet and loving with a playful spirit",
                Weight = 18,
                HealthStatus = "Excellent",
                ShelterId = shelters[0].Id,
                BreedId = breeds[1].Id,
                AnimalTypeId = animalTypes[2].Id,
                AdoptionStatus = AdoptionStatus.Available,
                PhotosIds = new List<string> {
                    "https://cdn.pixabay.com/photo/2015/03/26/09/54/dog-690159_1280.jpg",
                    "https://cdn.pixabay.com/photo/2016/07/27/07/26/dog-1543162_1280.jpg",
                    "https://cdn.pixabay.com/photo/2015/03/26/09/54/dog-690159_1280.jpg"
                }, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
			},
            new Animal {
                Name = "Buddy",
                Age = 5,
                Gender = Gender.Male,
                Description = "Friendly and dependable, always ready for fun",
                Weight = 32,
                HealthStatus = "Good",
                ShelterId = shelters[0].Id,
                BreedId = breeds[1].Id,
                AnimalTypeId = animalTypes[1].Id,
                AdoptionStatus = AdoptionStatus.Available,
                PhotosIds = new List<string> {
                    "https://cdn.pixabay.com/photo/2017/08/30/12/45/dog-2690939_1280.jpg",
                    "https://cdn.pixabay.com/photo/2017/08/30/12/45/dog-2690939_1280.jpg",
                    "https://cdn.pixabay.com/photo/2017/08/30/12/45/dog-2690939_1280.jpg"
                }, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
			},
            new Animal {
                Name = "Daisy",
                Age = 2,
                Gender = Gender.Female,
                Description = "Cheerful and gentle – a burst of sunshine",
                Weight = 12,
                HealthStatus = "Good",
                ShelterId = shelters[0].Id,
                BreedId = breeds[0].Id,
                AnimalTypeId = animalTypes[0].Id,
                AdoptionStatus = AdoptionStatus.Available,
                PhotosIds = new List<string> {
                    "https://images.unsplash.com/photo-1517423440428-a5a00ad493e8?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=60",
                    "https://cdn.pixabay.com/photo/2016/03/27/21/06/dog-1280185_1280.jpg",
                    "https://images.unsplash.com/photo-1543466835-00a7907e3c8b?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=60"
                }, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
			},
            new Animal {
                Name = "Cooper",
                Age = 3,
                Gender = Gender.Male,
                Description = "Smart and curious, loves to explore",
                Weight = 25,
                HealthStatus = "Excellent",
                ShelterId = shelters[0].Id,
                BreedId = breeds[0].Id,
                AnimalTypeId = animalTypes[1].Id,
                AdoptionStatus = AdoptionStatus.Available,
                PhotosIds = new List<string> {
                    "https://images.unsplash.com/photo-1575691554672-168dd42b0a60?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=60",
                    "https://images.unsplash.com/photo-1559599189-396b4a8c81d1?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=60",
                    "https://images.unsplash.com/photo-1575691554672-168dd42b0a60?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=60"
                }, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
			}
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
				new AdoptionApplication { UserId = users[0].Id, AnimalId = animals[0].Id, ShelterId = animals[0].ShelterId, ApplicationDetails = "Loves pets and has a spacious home.", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
				new AdoptionApplication { UserId = users[0].Id, AnimalId = animals[0].Id, ShelterId = animals[1].ShelterId, ApplicationDetails = "Experienced with pets, lives in a quiet neighborhood.", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
				new AdoptionApplication { UserId = users[0].Id, AnimalId = animals[0].Id, ShelterId = animals[2].ShelterId, ApplicationDetails = "Works from home, has time to care for pets.", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
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
				new Conversation { UserIds = new List<String> { users[0].Id, users[1].Id }, AnimalId = animals[0].Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
				new Conversation { UserIds = new List<String> { users[1].Id, users[2].Id }, AnimalId = animals[1].Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
				new Conversation { UserIds = new List<String> { users[2].Id, users[0].Id }, AnimalId = animals[2].Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
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
				new Message { ConversationId = conversations[0].Id, SenderId = conversations[0].UserIds[0], RecipientId = conversations[0].UserIds[1], Content = "Hello, I'm interested in adopting Buddy." },
				new Message { ConversationId = conversations[1].Id, SenderId = conversations[1].UserIds[0], RecipientId = conversations[1].UserIds[1], Content = "Is Mittens still available?" },
				new Message { ConversationId = conversations[2].Id, SenderId = conversations[2].UserIds[0], RecipientId = conversations[2].UserIds[1], Content = "How is Thumper's health?" }
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
				new Report { ReporterId = users[0].Id, ReportedId = users[1].Id, Type = ReportType.InAppropriateMessage, Reason = "Inappropriate content in message", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
				new Report { ReporterId = users[1].Id, ReportedId = users[2].Id, Type = ReportType.Other, Reason = "Suspicious activity", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
				new Report { ReporterId = users[2].Id, ReportedId = users[0].Id, Type = ReportType.InAppropriateMessage, Reason = "Offensive language", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
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
                new Notification { UserId = users[0].Id, Type = NotificationType.IncomingMessage, Content = "You have a new message about Buddy.", CreatedAt = DateTime.UtcNow },
                new Notification { UserId = users[1].Id, Type = NotificationType.AdoptionApplication, Content = "Your application for Mittens is under review.", CreatedAt = DateTime.UtcNow },
                new Notification { UserId = users[2].Id, Type = NotificationType.Report, Content = "You have been reported for inappropriate content.", CreatedAt = DateTime.UtcNow }
            };
			notificationsCollection.InsertMany(notifications);
		}
	}
}