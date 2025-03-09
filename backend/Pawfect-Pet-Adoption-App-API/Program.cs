using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using MongoDB.Driver;

using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authentication;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Cache;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Middleware;
using Pawfect_Pet_Adoption_App_API.Models;
using Pawfect_Pet_Adoption_App_API.Services.AdoptionApplicationServices.Extention;
using Pawfect_Pet_Adoption_App_API.Services.AnimalServices.Extention;
using Pawfect_Pet_Adoption_App_API.Services.AnimalTypeServices.Extentions;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices.Extentions;
using Pawfect_Pet_Adoption_App_API.Services.BreedServices.Extention;
using Pawfect_Pet_Adoption_App_API.Services.ConversationServices.Extention;
using Pawfect_Pet_Adoption_App_API.Services.EmailServices.Extention;
using Pawfect_Pet_Adoption_App_API.Services.HttpServices.Extentions;
using Pawfect_Pet_Adoption_App_API.Services.MessageServices.Extentions;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices.Extentions;
using Pawfect_Pet_Adoption_App_API.Services.NotificationServices.Extention;
using Pawfect_Pet_Adoption_App_API.Services.QueryServices.Extentions;
using Pawfect_Pet_Adoption_App_API.Services.ReportServices.Extentions;
using Pawfect_Pet_Adoption_App_API.Services.SearchServices.Extentions;
using Pawfect_Pet_Adoption_App_API.Services.ShelterServices.Extentions;
using Pawfect_Pet_Adoption_App_API.Services.SmsServices.Extentions;
using Pawfect_Pet_Adoption_App_API.Services.UserServices.Extentions;
using Pawfect_Pet_Adoption_App_API.Services.ValidationServices.Extentions;

using Serilog;

using System.Text;

public class Program
{
	public static async Task Main(String[] args)
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

		ConfigureAppConfiguration(builder);
		builder.Host.UseSerilog((context, configuration) => configuration
			.ReadFrom.Configuration(context.Configuration)
			.Enrich.FromLogContext());

		ConfigureServices(builder);

		WebApplication app = builder.Build();

		if (args.Length == 1 && args[0].ToLower() == "seeddata")
			SeedData(app);

		Configure(app);

		await app.RunAsync();
	}

	private const String Configuration = "Configuration";
	public static void ConfigureAppConfiguration(WebApplicationBuilder builder)
	{
		IWebHostEnvironment env = builder.Environment;
		IConfigurationBuilder configBuilder = new ConfigurationBuilder();

		String[] configurationPaths = new String[]
		{
			Path.Combine(env.ContentRootPath, Configuration),
		};

		// Add configuration files for each section
		AddConfigurationFiles(configBuilder, configurationPaths, "cache", env);
		AddConfigurationFiles(configBuilder, configurationPaths, "apis", env);
		AddConfigurationFiles(configBuilder, configurationPaths, "auth", env);
		AddConfigurationFiles(configBuilder, configurationPaths, "cors", env);
		AddConfigurationFiles(configBuilder, configurationPaths, "db", env);
		AddConfigurationFiles(configBuilder, configurationPaths, "logging", env);

		// Load environment variables from 'environment.json'
		foreach (String path in configurationPaths)
		{
			String envFilePath = Path.Combine(path, "environment.json");
			if (File.Exists(envFilePath))
			{
				configBuilder.AddJsonFile(envFilePath, optional: false, reloadOnChange: true);
				break;
			}
		}

		// Build the configuration
		IConfiguration configuration = configBuilder.Build();

		// Replace placeholders with actual values
		configuration = ConfigurationHandler.ReplacePlaceholders(configuration);

		// Set the configuration to the builder
		builder.Configuration.AddConfiguration(configuration);
	}

	private static void AddConfigurationFiles(IConfigurationBuilder configBuilder, String[] paths, String baseFileName, IWebHostEnvironment env)
	{
		// List of configuration file patterns
		(String FileName, Boolean Optional)[] configFiles = new[]
		{
			(FileName: $"{baseFileName}.json", Optional: false),
			(FileName: $"{baseFileName}.override.json", Optional: true),
			(FileName: $"{baseFileName}.{env.EnvironmentName}.json", Optional: true),
			(FileName: $"{baseFileName}.override.{env.EnvironmentName}.json", Optional: true),
		};

		foreach ((String fileName, Boolean optional) in configFiles)
		{
			foreach (String path in paths)
			{
				String fullPath = Path.Combine(path, fileName);
				configBuilder.AddJsonFile(fullPath, optional: optional, reloadOnChange: true);
			}
		}
	}

	public static void ConfigureServices(WebApplicationBuilder builder)
	{
		// Logger configuration
		builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

		// MongoDB configuration
		builder.Services.Configure<MongoDbConfig>(builder.Configuration.GetSection("MongoDbConfig"));
		builder.Services.AddSingleton<IMongoClient>(s =>
			new MongoClient(builder.Configuration.GetValue<String>("MongoDbConfig:MongoDb")));
		builder.Services.AddScoped(s =>
			s.GetRequiredService<IMongoClient>().GetDatabase(builder.Configuration.GetValue<String>("MongoDbConfig:DatabaseName")));

		// Cache Configuration
		builder.Services.Configure<CacheConfig>(builder.Configuration.GetSection("Cache"));
		// Cors Configuration
		builder.Services.Configure<CorsConfig>(builder.Configuration.GetSection("Cors"));

		// Services
		builder.Services
		.AddQueryAndBuilderServices()
		.AddValidationServices()
		.AddAdoptionApplicationServices()
		.AddAnimalServices()
		.AddAnimalTypeServices()
		.AddBreedServices()
		.AddAuthenticationServices(builder.Configuration.GetSection("Authentication"))
		.AddBreedServices()
		.AddConversationServices()
		.AddEmailServices(builder.Configuration.GetSection("SendGrid"))
		.AddHttpServices()
		.AddMessageServices()
		.AddMongoServices()
		.AddNotificationServices()
		.AddReportServices()
		.AddSearchServices()
		.AddShelterServices()
		.AddSmsServices(builder.Configuration.GetSection("SmsService"))
		.AddUserServices();

		// HttpContextAccessor
		builder.Services.AddHttpContextAccessor();


		// CORS
		List<String> allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<List<String>>() ?? new List<String>();
		builder.Services.AddCors(options =>
		{
			options.AddPolicy("Cors", policyBuilder =>
			{
				policyBuilder.WithOrigins(allowedOrigins.ToArray())
							 .WithMethods("GET", "POST", "PUT", "DELETE")
							 .AllowAnyHeader();
			});
		});

		JwtConfig jwtConfig = JwtService.GetJwtSettings(builder.Configuration.GetSection("Authentication"));
		// Authentication and JWT
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
				ValidIssuer = jwtConfig.Issuer,
				ValidAudiences = jwtConfig.Audiences,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key)),
			};
			options.Events = new JwtBearerEvents
			{
				OnTokenValidated = context =>
				{
					String tokenId = context.SecurityToken.Id;
					JwtService revokedTokensService = context.HttpContext.RequestServices.GetRequiredService<JwtService>();

					if (revokedTokensService.IsTokenRevoked(tokenId))
					{
						context.Fail("This token has been revoked.");
					}
					return Task.CompletedTask;
				}
			};
		});

		// Memory Cache
		builder.Services.AddMemoryCache();

		// Swagger
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen(options =>
		{
			options.SwaggerDoc("v1", new OpenApiInfo { Title = "Pawfect Pet Adoption API", Version = "v1" });
			options.CustomSchemaIds(type => type.FullName);
		});
	}

	public static void Configure(WebApplication app)
	{
		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
			app.UseDeveloperExceptionPage();
		}

		app.UseHttpsRedirection();

		app.UseCors("Cors");

		app.UseAuthentication();

		app.UseJwtRevocation();

		app.UseAuthorization();

		app.MapControllers();
	}

	public static void SeedData(IHost app)
	{
		try
		{
			using IServiceScope scope = app.Services.CreateScope();
			Seeder seeder = scope.ServiceProvider.GetRequiredService<Seeder>();
			seeder.Seed();
			Console.WriteLine("Data seeding completed successfully.");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Data seeding failed: {ex}");
		}
	}
}