using FluentValidation;
using FluentValidation.AspNetCore;
using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data;
using Pawfect_Pet_Adoption_App_API.Models;
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
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Ρυθμήσεις Logger //
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // Load configuration from appsettings.json
    .Enrich.FromLogContext() // Enrich logs with contextual information
    .CreateLogger();

// Use Serilog middleware to log using ILogger
builder.Host.UseSerilog();
// - Ρυθμήσεις Logger //

// Προσθήκη JSON configuration αρχείων //
builder.Configuration.AddJsonFile("Cache_Configurations.json", optional: false);
builder.Configuration.AddJsonFile("APIs_Configurations.json", optional: false);
// -- Προσθήκη JSON configuration αρχείων -- //

// Ρύθμιση της υπηρεσίας MongoDB
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddSingleton<IMongoClient>(s => new MongoClient(builder.Configuration.GetValue<string>("MongoDbSettings:MongoDb")));
builder.Services.AddScoped(s => s.GetRequiredService<IMongoClient>().GetDatabase(builder.Configuration.GetValue<string>("MongoDbSettings:DatabaseName")));
// -- Ρύθμιση της υπηρεσίας MongoDB

// Προσθέστε την υπηρεσία Seeder για το πρώτο Seeding της βάσης δεδομένων
builder.Services.AddTransient<Seeder>();


// Ρύθμιση Controllers , μαζί με δυνατότητα χρήσης Fluent Validation , χωρις auto validation, θα γίνει μικτά auto κ mannual
builder.Services.AddControllers()
    .AddFluentValidation(
        fv => fv.DisableDataAnnotationsValidation = true
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
builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<OTPVerificationValidator>();
// -- Register auto validation for persist dtos



// Configure Auto Mapper
builder.Services.AddAutoMapper(
    typeof(AutoUserBuilder),
    typeof(AutoShelterBuilder),
    typeof(AutoReportBuilder),
    typeof(AutoNotificationBuilder),
    typeof(AutoMessageBuilder),
    typeof(AutoConversationBuilder),
    typeof(AutoAnimalTypeBuilder),
    typeof(AutoBreedBuilder),
    typeof(AutoAnimalBuilder),
    typeof(AutoAdoptionApplicationBuilder)
);

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
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<RequestService>();
builder.Services.AddScoped<MongoDbService>();
// -- Services

// Προσθήκη Memory Cache services
builder.Services.AddMemoryCache();
// -- Προσθήκη Memory Cache services

// Προσθήκη HttpClient //
builder.Services.AddHttpClient();
// -- Προσθήκη HttpClient //

// Προσθήκη HttpContextAccessor για διαχείρηση των Request δεδομένων και του API //
builder.Services.AddHttpContextAccessor();
// -- Προσθήκη HttpContextAccessor για διαχείρηση των Request δεδομένων και του API //

// Προσθήκη CORS υπηρεσιών για διαχείρηση ασφάλειας των origins
builder.Services.AddCors(options =>
{
    // Προσωρινά τα αποδεχόμαστε όλα
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
// -- Προσθήκη CORS υπηρεσιών για διαχείρηση ασφάλειας των origins


// Πρόσθεση endpoints και SwaggerUI για την ανάπτυξη, doccumentation και testing του API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// -- Πρόσθεση endpoints και SwaggerUI για την ανάπτυξη, doccumentation και testing του API

WebApplication app = builder.Build();

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
app.UseCors("AllowAll");
app.MapControllers();

app.Run();
