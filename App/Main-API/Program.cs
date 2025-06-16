using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using MongoDB.Driver;
using Main_API.BackgroundTasks.TemporaryFilesCleanupTask.Extensions;
using Main_API.BackgroundTasks.UnverifiedUserCleanupTask.Extensions;
using Main_API.Data.Entities.Types.Authentication;
using Main_API.Data.Entities.Types.Authorization;
using Main_API.Data.Entities.Types.Cache;
using Main_API.DevTools;
using Main_API.Middleware;
using Main_API.Middlewares;
using Main_API.Models;
using Main_API.Services.AdoptionApplicationServices.Extention;
using Main_API.Services.AnimalServices.Extention;
using Main_API.Services.AnimalTypeServices.Extentions;
using Main_API.Services.AuthenticationServices;
using Main_API.Services.AuthenticationServices.Extentions;
using Main_API.Services.AwsServices.Extention;
using Main_API.Services.BreedServices.Extention;
using Main_API.Services.Convention.Extention;
using Main_API.Services.ConversationServices.Extention;
using Main_API.Services.CookiesServices.Extensions;
using Main_API.Services.EmailServices.Extention;
using Main_API.Services.FileServices.Extention;
using Main_API.Services.FilterServices.Extensions;
using Main_API.Services.HttpServices.Extentions;
using Main_API.Services.MessageServices.Extentions;
using Main_API.Services.MongoServices.Extentions;
using Main_API.Services.NotificationServices.Extention;
using Main_API.Services.QueryServices.Extentions;
using Main_API.Services.ReportServices.Extentions;
using Main_API.Services.SearchServices.Extentions;
using Main_API.Services.ShelterServices.Extentions;
using Main_API.Services.SmsServices.Extentions;
using Main_API.Services.UserServices.Extentions;
using Main_API.Services.ValidationServices.Extentions;

using Serilog;

using System.Text;
using Main_API.BackgroundTasks.RefreshTokensCleanupTask.Extensions;
using Pawfect_Pet_Adoption_App_API.Middlewares;

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

		if (args.Length == 1 && args[0].Equals("seeddata", StringComparison.OrdinalIgnoreCase))
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
		AddConfigurationFiles(configBuilder, configurationPaths, "aws", env);
		AddConfigurationFiles(configBuilder, configurationPaths, "files", env);
        AddConfigurationFiles(configBuilder, configurationPaths, "permissions", env);
        AddConfigurationFiles(configBuilder, configurationPaths, "background_tasks", env);
        AddConfigurationFiles(configBuilder, configurationPaths, "profile-fields", env);


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

        // HttpContextAccessor
        builder.Services.AddHttpContextAccessor();

		// Services
		builder.Services
		.AddQueryAndBuilderServices()
		.AddValidationServices()
		.AddAdoptionApplicationServices()
		.AddAnimalServices()
		.AddAnimalTypeServices()
		.AddBreedServices()
		.AddAuthenticationServices(builder.Configuration.GetSection("Authentication"), builder.Configuration.GetSection("PermissionConfig"))
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
		.AddUserServices(builder.Configuration.GetSection("UserFields"))
		.AddConventionServices()
		.AddAwsServices(builder.Configuration.GetSection("Aws"))
		.AddFileServices(builder.Configuration.GetSection("Files"))
		.AddFilterBuilderServices()
		.AddUnverifiedUserCleanupTask(builder.Configuration.GetSection("BackgroundTasks:UnverifiedUserCleanupTask"))
		.AddTemporaryFilesCleanupTask(builder.Configuration.GetSection("BackgroundTasks:TemporaryFilesCleanupTask"))
        .AddRefreshTokenCleanupTask(builder.Configuration.GetSection("BackgroundTasks:RefreshTokenCleanupTask"))
        .AddCookiesServices();

        //builder.WebHost.ConfigureKestrel(serverOptions =>
        //{
        //    serverOptions.ListenAnyIP(7200); // for HTTP
        //    serverOptions.ListenAnyIP(7201, listenOptions => listenOptions.UseHttps()); // for HTTPS
        //});

        // CORS
        List<String> allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<List<String>>() ?? new List<String>();
		builder.Services.AddCors(options =>
		{
			options.AddPolicy("Cors", policyBuilder =>
			{
				policyBuilder.WithOrigins([.. allowedOrigins])
							 .WithMethods("GET", "POST", "PUT", "DELETE")
							 .AllowAnyHeader()
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
			options.RequireHttpsMetadata = false;
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
		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
			app.UseDeveloperExceptionPage();
		}

		// TODO
		if (!app.Environment.IsDevelopment())
		{
            app.UseHttpsRedirection();
        }

		app.UseCors("Cors");

		// Authentication
		app.UseAuthentication();

		// MIDDLEWARES
		app.UseJwtRevocation();
        app.UseVerifiedUserMiddleware();
        app.UseErrorHandlingMiddleware();

        // Authorization
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