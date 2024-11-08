using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Database;
using Pawfect_Pet_Adoption_App_API.DevTools.Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Repositories.Implementations;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Ρύθμιση Controllers , να δέχονται και custom annotations για Enum
builder.Services.AddControllers(options =>
{
    options.Filters.Add<JsonExceptionFilter>();
});

// Ρύθμιση της υπηρεσίας MongoDB
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddSingleton<IMongoClient>(s =>
    new MongoClient(builder.Configuration.GetValue<string>("MongoDbSettings:ConnectionString")));
builder.Services.AddScoped(s => s.GetRequiredService<IMongoClient>().GetDatabase(
    builder.Configuration.GetValue<string>("MongoDbSettings:DatabaseName")));


// Προσθέστε την υπηρεσία Seeder για το πρώτο Seeding της βάσης δεδομένων
builder.Services.AddTransient<Seeder>();

// AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Repositories
builder.Services.AddScoped(typeof(IGeneralRepo<>), typeof(GeneralRepo<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAnimalRepository, AnimalRepository>();
builder.Services.AddScoped<IAnimalTypeRepository, AnimalTypeRepository>();
builder.Services.AddScoped<IBreedRepository, BreedRepository>();
builder.Services.AddScoped<IShelterRepository, ShelterRepository>();
builder.Services.AddScoped<IAdoptionApplicationRepository, AdoptionApplicationRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();




// Services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<MongoDbService>();

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
