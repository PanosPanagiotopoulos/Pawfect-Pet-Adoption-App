using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data;
using Pawfect_Pet_Adoption_App_API.Models;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Models.AnimalType;
using Pawfect_Pet_Adoption_App_API.Models.Breed;
using Pawfect_Pet_Adoption_App_API.Models.Conversation;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Message;
using Pawfect_Pet_Adoption_App_API.Models.Notification;
using Pawfect_Pet_Adoption_App_API.Models.Report;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Repositories.Implementations;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services;
using Serilog;
using System.Text;


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
builder.Configuration.AddJsonFile("Authentication.json", optional: false);
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
    .AddFluentValidation(fv => fv.DisableDataAnnotationsValidation = true);

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
builder.Services.AddScoped<IShelterService, ShelterService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IBreedService, BreedService>();
builder.Services.AddScoped<IAnimalTypeService, AnimalTypeService>();
builder.Services.AddScoped<IAnimalService, AnimalService>();
builder.Services.AddScoped<IAdoptionApplicationService, AdoptionApplicationService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<RequestService>();
builder.Services.AddScoped<MongoDbService>();

// Lazy Services
builder.Services.AddScoped(provider => new Lazy<IUserService>(() => provider.GetRequiredService<IUserService>()));
builder.Services.AddScoped(provider => new Lazy<IShelterService>(() => provider.GetRequiredService<IShelterService>()));
// -- Lazy Services

// Search Service ως HttpClient για καλύτερο management στα requests στον Search Server
builder.Services.AddHttpClient<SearchService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5000"); // Search server URL
});

// * Το JWT Service θα είναι Singleton γιατί είναι stateless και απλά παρέχει υπηρεσίες Authentication
builder.Services.AddSingleton<JwtService>();

// -- Services

// Querying Services
builder.Services.AddScoped<UserQuery>();
builder.Services.AddScoped<ShelterQuery>();
builder.Services.AddScoped<ReportQuery>();
builder.Services.AddScoped<NotificationQuery>();
builder.Services.AddScoped<MessageQuery>();
builder.Services.AddScoped<ConversationQuery>();
builder.Services.AddScoped<BreedQuery>();
builder.Services.AddScoped<AnimalQuery>();
builder.Services.AddScoped<AnimalTypeQuery>();
builder.Services.AddScoped<AdoptionApplicationQuery>();
// -- Querying Services

// Lookup Objects Services
builder.Services.AddScoped<AnimalLookup>();
builder.Services.AddScoped<UserLookup>();
builder.Services.AddScoped<ShelterLookup>();
builder.Services.AddScoped<NotificationLookup>();
builder.Services.AddScoped<ReportLookup>();
builder.Services.AddScoped<ConversationLookup>();
builder.Services.AddScoped<MessageLookup>();
builder.Services.AddScoped<AdoptionApplicationLookup>();
builder.Services.AddScoped<BreedLookup>();
builder.Services.AddScoped<AnimalTypeLookup>();
// -- Lookup Objects Services

// Προσθήκη HttpContextAccessor για διαχείρηση των Request δεδομένων και του API //
builder.Services.AddHttpContextAccessor();
// -- Προσθήκη HttpContextAccessor για διαχείρηση των Request δεδομένων και του API //

// Προσθήκη HttpClient //
builder.Services.AddHttpClient();
// -- Προσθήκη HttpClient //

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

// Προσθήκη JWT Token Αυθεντικοποίησης
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            string tokenId = context.SecurityToken.Id;
            JwtService revokedTokensService = context.HttpContext.RequestServices.GetRequiredService<JwtService>();

            if (revokedTokensService.IsTokenRevoked(tokenId))
            {
                context.Fail("Αυτό το token είναι πλέον revoked.");
            }
            return Task.CompletedTask;
        }
    };
});
// -- Προσθήκη JWT Token Αυθεντικοποίησης

// Προσθήκη Memory Cache services
builder.Services.AddMemoryCache();
// -- Προσθήκη Memory Cache services

// Πρόσθεση endpoints και SwaggerUI για την ανάπτυξη, doccumentation και testing του API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Pawfect Pet Adoption API", Version = "v1" });
    options.CustomSchemaIds(type => type.FullName); // Avoid duplicate schema names
});
// -- Πρόσθεση endpoints και SwaggerUI για την ανάπτυξη, doccumentation και testing του API

WebApplication app = builder.Build();

// Καθορισμός Seeding της βάσης
if (args.Length == 1 && args[0].ToLower() == "seeddata")
    SeedData(app);

async void SeedData(IHost app)
{
    try
    {
        IServiceScopeFactory scopedFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
        using IServiceScope scope = scopedFactory.CreateScope();
        Seeder seeder = scope.ServiceProvider.GetRequiredService<Seeder>();
        seeder.Seed();
        Console.WriteLine("Η φόρτωση temporary δεδομένων ολοκληρώθηκε με επιτυχία.");
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

app.UseDeveloperExceptionPage();

// Προσθήκη HTTPS πρωτοκόλλου επικοινωνίας
app.UseHttpsRedirection();

// Προσθήκη Αυθεντικοποίησης
app.UseAuthentication();
// Προσθήκη Authorisation
app.UseAuthorization();

// Επιβολή των CORS κανόνων
app.UseCors("AllowAll");

// Προσθήκη Controlller ROutes
app.MapControllers();

app.Run();
