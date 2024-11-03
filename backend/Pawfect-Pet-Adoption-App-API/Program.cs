using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Database;
using Pawfect_Pet_Adoption_App_API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure MongoDB services
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddSingleton<IMongoClient>(s =>
    new MongoClient(builder.Configuration.GetValue<string>("MongoDbSettings:ConnectionString")));
builder.Services.AddScoped(s => s.GetRequiredService<IMongoClient>().GetDatabase(
    builder.Configuration.GetValue<string>("MongoDbSettings:DatabaseName")));

// Add Seeder service for database seeding
builder.Services.AddTransient<Seeder>();

// Register additional services
builder.Services.AddScoped<MongoDbService>();

var app = builder.Build();

// Check for seed data argument and run seeding if present
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
        Console.WriteLine("Data seeding completed successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Data seeding failed: {ex}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
