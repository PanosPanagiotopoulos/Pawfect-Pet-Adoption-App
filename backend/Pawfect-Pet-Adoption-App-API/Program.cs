using FluentValidation;
using FluentValidation.AspNetCore;
using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Models.AnimalType;
using Pawfect_Pet_Adoption_App_API.Models.Breed;
using Pawfect_Pet_Adoption_App_API.Models.Conversation;
using Pawfect_Pet_Adoption_App_API.Models.Message;
using Pawfect_Pet_Adoption_App_API.Models.Notification;
using Pawfect_Pet_Adoption_App_API.Models.Report;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Pawfect_Pet_Adoption_App_API.Repositories.Implementations;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services;

var builder = WebApplication.CreateBuilder(args);

// Ρύθμιση της υπηρεσίας MongoDB
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddSingleton<IMongoClient>(s => new MongoClient(builder.Configuration.GetValue<string>("MongoDbSettings:ConnectionString")));
builder.Services.AddScoped(s => s.GetRequiredService<IMongoClient>().GetDatabase(builder.Configuration.GetValue<string>("MongoDbSettings:DatabaseName")));
// -- Ρύθμιση της υπηρεσίας MongoDB

// Προσθέστε την υπηρεσία Seeder για το πρώτο Seeding της βάσης δεδομένων
builder.Services.AddTransient<Seeder>();


// Ρύθμιση Controllers , μαζί με δυνατότητα χρήσης Fluent Validation , χωρις auto validation, θα γίνει μικτά auto κ mannual
builder.Services.AddControllers()
    .AddFluentValidation(
        fv => fv.DisableDataAnnotationsValidation = false
    );

// Register auto validation for persist dtos
builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ShelterValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ReportValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<NotificationValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<MessageValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ConversationValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<BreedValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AnimalTypeValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AnimalValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<AdoptionApplicationValidator>();
// -- Register auto validation for persist dtos



// Builders (DTO)
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Repositories
builder.Services.AddScoped(typeof(IGeneralRepo<>), typeof(GeneralRepo<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IShelterRepository, ShelterRepository>();
builder.Services.AddScoped<IAnimalRepository, AnimalRepository>();
builder.Services.AddScoped<IAnimalTypeRepository, AnimalTypeRepository>();
builder.Services.AddScoped<IBreedRepository, BreedRepository>();
builder.Services.AddScoped<IAdoptionApplicationRepository, AdoptionApplicationRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
// -- Repositories


// Services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<MongoDbService>();
// -- Services


// Πρόσθεση endpoints και SwaggerUI για την ανάπτυξη, doccumentation και testing του API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Καθορισμός Seeding της βάσης
if (args.Length == 1 && args[0].ToLower() == "seeddata")
    SeedData(app);

async void SeedData(IHost app)
{
    try
    {
        var scopedFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopedFactory.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<Seeder>();
        seeder.Seed();
        Console.WriteLine("Ο σποράς των δεδομένων ολοκληρώθηκε με επιτυχία.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Αποτυχία σποράς δεδομένων: {ex}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
