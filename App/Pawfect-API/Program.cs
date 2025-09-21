using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Pawfect_API.BackgroundTasks.TemporaryFilesCleanupTask.Extensions;
using Pawfect_API.BackgroundTasks.UnverifiedUserCleanupTask.Extensions;
using Pawfect_API.Data.Entities.Types.Authentication;
using Pawfect_API.Data.Entities.Types.Authorization;
using Pawfect_API.Data.Entities.Types.Cache;
using Pawfect_API.DevTools;
using Pawfect_API.Middleware;
using Pawfect_API.Middlewares;
using Pawfect_API.Services.AdoptionApplicationServices.Extention;
using Pawfect_API.Services.AnimalServices.Extention;
using Pawfect_API.Services.AnimalTypeServices.Extentions;
using Pawfect_API.Services.AuthenticationServices;
using Pawfect_API.Services.AuthenticationServices.Extentions;
using Pawfect_API.Services.AwsServices.Extention;
using Pawfect_API.Services.BreedServices.Extention;
using Pawfect_API.Services.Convention.Extention;
using Pawfect_API.Services.CookiesServices.Extensions;
using Pawfect_API.Services.FileServices.Extention;
using Pawfect_API.Services.FilterServices.Extensions;
using Pawfect_API.Services.HttpServices.Extentions;
using Pawfect_API.Services.MongoServices.Extentions;
using Pawfect_API.Services.NotificationServices.Extention;
using Pawfect_API.Services.QueryServices.Extentions;
using Pawfect_API.Services.ReportServices.Extentions;
using Pawfect_API.Services.ShelterServices.Extentions;
using Pawfect_API.Services.UserServices.Extentions;
using Pawfect_API.Services.ValidationServices.Extentions;

using Serilog;

using System.Text;
using Pawfect_API.BackgroundTasks.RefreshTokensCleanupTask.Extensions;
using Pawfect_API.Middlewares;
using Pawfect_API.Data.Entities.Types.Apis;
using Pawfect_API.Services.EmbeddingServices.Extentions;
using Pawfect_API.Services.MongoServices;
using Pawfect_API.Data.Entities.Types.Mongo;
using Pawfect_API.Services.TranslationServices.Extentions;
using Vonage.Voice.EventWebhooks;
using Microsoft.Extensions.Configuration;
using Pawfect_API.Data.Entities.Types.RateLimiting;
using Pawfect_API.Services.AiAssistantServices.Extentions;

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

        // Include frontend files in production
        if (builder.Environment.IsProduction())
        {
            builder.WebHost.UseWebRoot("wwwroot");
        }

        WebApplication app = builder.Build();

        // Bootsrap MongoDB Database Data & Index
        using (IServiceScope scope = app.Services.CreateScope())
		{
			//if (args.Length == 0)
			if (args.Length == 1 && args[0].Equals("seeddata", StringComparison.OrdinalIgnoreCase))
			{
                try
                {
                    Seeder seeder = scope.ServiceProvider.GetRequiredService<Seeder>();
                    await seeder.Seed();
                    Console.WriteLine("Data seeding completed successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Data seeding failed: {ex}");
                }
            }

			await Task.Delay(1000);
            MongoDbService mongoDbService = scope.ServiceProvider.GetRequiredService<MongoDbService>();
			await mongoDbService.SetupSearchIndexesAsync();
		}

		Configure(app);

		await app.RunAsync();
	}

	private const String Configuration = "Configuration";
	public static void ConfigureAppConfiguration(WebApplicationBuilder builder)
	{
		IWebHostEnvironment env = builder.Environment;
		IConfigurationBuilder configBuilder = new ConfigurationBuilder();


		String configPath = Path.Join(Configuration, env.IsDevelopment() ? "Development" : "Production");
		String[] configurationPaths = new String[]
		{
			Path.Combine(env.ContentRootPath, configPath),
		};

		// Add configuration files for each section
		AddConfigurationFiles(configBuilder, configurationPaths, "auth", env);
		AddConfigurationFiles(configBuilder, configurationPaths, "cors", env);
		AddConfigurationFiles(configBuilder, configurationPaths, "db", env);
		AddConfigurationFiles(configBuilder, configurationPaths, "logging", env);
		AddConfigurationFiles(configBuilder, configurationPaths, "aws", env);
		AddConfigurationFiles(configBuilder, configurationPaths, "files", env);
        AddConfigurationFiles(configBuilder, configurationPaths, "cache", env);
        AddConfigurationFiles(configBuilder, configurationPaths, "permissions", env);
        AddConfigurationFiles(configBuilder, configurationPaths, "background_tasks", env);
        AddConfigurationFiles(configBuilder, configurationPaths, "profile-fields", env);
        AddConfigurationFiles(configBuilder, configurationPaths, "animals", env);
        AddConfigurationFiles(configBuilder, configurationPaths, "api-keys", env);
		AddConfigurationFiles(configBuilder, configurationPaths, "embedding", env);
        AddConfigurationFiles(configBuilder, configurationPaths, "translation", env);
        AddConfigurationFiles(configBuilder, configurationPaths, "notification", env);
        AddConfigurationFiles(configBuilder, configurationPaths, "rate-limit", env);
        AddConfigurationFiles(configBuilder, configurationPaths, "ai-assistant", env);


        // Load environment variables
        String envFileName = env.IsDevelopment() ? "environment.Development.json" : "environment.json";
        foreach (String path in configurationPaths)
		{
            String envFilePath = Path.Combine(path, envFileName);
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
        String fileName = env.IsDevelopment()
            ? $"{baseFileName}.Development.json"
            : $"{baseFileName}.json";


        foreach (String path in paths)
        {
            String fullPath = Path.Combine(path, fileName);
			configBuilder.AddJsonFile(fullPath, optional: false, reloadOnChange: true);
        }
    }

    public static void ConfigureServices(WebApplicationBuilder builder)
	{
		// Logger configuration
		builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

        // MongoDB configuration
        builder.Services.Configure<MongoDbConfig>(builder.Configuration.GetSection("MongoDbConfig"));

        MongoDbConfig mongoConfig = builder.Configuration.GetSection("MongoDbConfig").Get<MongoDbConfig>();

        builder.Services.AddSingleton<IMongoClient>(s =>
			new MongoClient(mongoConfig.ConnectionString));

		builder.Services.AddScoped(s =>
			s.GetRequiredService<IMongoClient>().GetDatabase(mongoConfig.DatabaseName));

		// Cache Configuration
		builder.Services.Configure<CacheConfig>(builder.Configuration.GetSection("Cache"));
		// Cors Configuration
		builder.Services.Configure<CorsConfig>(builder.Configuration.GetSection("Cors"));
        // Api Keys Configuration
        builder.Services.Configure<ApiKeyConfig>(builder.Configuration.GetSection("ApiKeys"));
		// Rate Limiting
        builder.Services.Configure<RateLimitConfig>(builder.Configuration.GetSection("RateLimit"));

        // HttpContextAccessor
        builder.Services.AddHttpContextAccessor();

		// Services
		builder.Services
		.AddQueryAndBuilderServices()
		.AddValidationServices()
		.AddAdoptionApplicationServices()
		.AddAnimalServices(builder.Configuration.GetSection("Animals"))
		.AddAnimalTypeServices()
		.AddBreedServices()
		.AddAuthenticationServices(builder.Configuration.GetSection("Authentication"), builder.Configuration.GetSection("PermissionConfig"))
		.AddHttpServices()
		.AddMongoServices()
		.AddNotificationServices(builder.Configuration.GetSection("Notifications"))
		.AddReportServices()
		.AddShelterServices()
		.AddUserServices(builder.Configuration.GetSection("UserFields"))
		.AddConventionServices()
		.AddAwsServices(builder.Configuration.GetSection("Aws"))
		.AddFileServices(builder.Configuration.GetSection("Files"))
		.AddFilterBuilderServices()
		.AddUnverifiedUserCleanupTask(builder.Configuration.GetSection("BackgroundTasks:UnverifiedUserCleanupTask"))
		.AddTemporaryFilesCleanupTask(builder.Configuration.GetSection("BackgroundTasks:TemporaryFilesCleanupTask"))
        .AddRefreshTokenCleanupTask(builder.Configuration.GetSection("BackgroundTasks:RefreshTokenCleanupTask"))
        .AddCookiesServices()
		.AddEmbeddingServices(builder.Configuration.GetSection("Embedding"))
        .AddTranslationServices(builder.Configuration.GetSection("Translation"))
		.AddAiAssistantServices(builder.Configuration.GetSection("AiAssistant"));


        // CORS
        List<String> allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<List<String>>() ?? new List<String>();
		builder.Services.AddCors(options =>
		{
			options.AddPolicy("Cors", policyBuilder =>
			{
				policyBuilder.WithOrigins([.. allowedOrigins])
							 .WithMethods("GET", "POST", "PUT", "DELETE")
							 .AllowAnyHeader()
                             .WithExposedHeaders("Content-Disposition")
							 .AllowCredentials();
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
			// TODO: In prod be true
			options.RequireHttpsMetadata = builder.Environment.IsProduction();
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
                OnMessageReceived = context =>
                {
                    
					String? token = context.Request.Cookies[JwtService.ACCESS_TOKEN];
                    if (!String.IsNullOrEmpty(token))
                    {
                        context.Token = token;
                    }
                    return Task.CompletedTask;
                }
            };
		});

        // Authorization
        builder.Services.AddAuthorization(options =>
        {
            // Permission-based policies
            List<String> permissions = typeof(Permission).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Where(f => f.FieldType == typeof(String))
                .Select(f => f.GetValue(null)?.ToString())
                .Where(p => !String.IsNullOrEmpty(p))
                .ToList();

            foreach (String permission in permissions)
            {
                options.AddPolicy(permission, policy =>
                    policy.RequireAssertion(context =>
                    {
                        using IServiceScope scope = context.Resource is IServiceProvider sp
                            ? sp.CreateScope()
                            : builder.Services.BuildServiceProvider().CreateScope();
                        PermissionPolicyProvider permissionProvider = scope.ServiceProvider.GetRequiredService<PermissionPolicyProvider>();
                        return permissionProvider.HasPermission(context.User, permission);
                    }));
            }

            // Owned and Affiliated policies
            options.AddPolicy("OwnedPolicy", policy =>
                policy.AddRequirements(new OwnedRequirement(new OwnedResource())));
            options.AddPolicy("AffiliatedPolicy", policy =>
                policy.AddRequirements(new AffiliatedRequirement(new AffiliatedResource())));
        });

        // Memory Cache
        builder.Services.AddMemoryCache(options =>
        {
            // scan for expired entries every 5 mins
            options.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
            // when over limit, remove 20% of cache entries
            options.CompactionPercentage = 0.2; 
        });

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
        app.UseCors("Cors");

        if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
			app.UseDeveloperExceptionPage();
		}

		// TODO
		if (app.Environment.IsProduction())
		{
			app.UseHttpsRedirection();
			app.UseDefaultFiles();
			app.UseStaticFiles();
        }

		// Authentication
		app.UseAuthentication();

		// MIDDLEWARES
		app.UseRateLimitMiddleware();
		app.UseApiKeyMiddleware();
		app.UseJwtRevocation();
		app.UseVerifiedUserMiddleware();
		app.UseErrorHandlingMiddleware();

		// Authorization
		app.UseAuthorization();

		app.MapControllers();

		if (app.Environment.IsProduction())
		{
            app.MapFallbackToFile("index.html", new StaticFileOptions
			{
				OnPrepareResponse = ctx =>
				{
					ctx.Context.Response.Headers["Cache-Control"] = "private, max-age=3600";
				}
			});
		}
    }
}