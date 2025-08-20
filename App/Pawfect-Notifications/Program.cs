using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Pawfect_Notifications.Data.Entities.Types.Authentication;
using Pawfect_Notifications.Data.Entities.Types.Cache;
using Pawfect_Notifications.DevTools;
using Pawfect_Notifications.Middleware;
using Pawfect_Notifications.Middlewares;
using Pawfect_Notifications.Services.CookiesServices.Extensions;
using Pawfect_Notifications.Services.HttpServices.Extentions;
using Pawfect_Notifications.Services.MongoServices.Extentions;
using Pawfect_Notifications.Services.NotificationServices.Extention;
using Pawfect_Notifications.Services.QueryServices.Extentions;
using Pawfect_Notifications.Services.ValidationServices.Extentions;
using Serilog;
using System.Text;
using Pawfect_Notifications.Data.Entities.Types.Mongo;
using Pawfect_Notifications.Data.Entities.Types.Apis;
using Pawfect_Notifications.Services.Convention.Extention;
using Pawfect_Notifications.Services.AuthenticationServices.Extentions;
using Pawfect_Notifications.Services.AuthenticationServices;
using Pawfect_Notifications.Services.FilterServices.Extensions;
using Pawfect_Notifications.Data.Entities.Types.Authorization;
using Pawfect_Notifications.BackgroundTasks.Extentions;

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
        AddConfigurationFiles(configBuilder, configurationPaths, "notifications", env);
        AddConfigurationFiles(configBuilder, configurationPaths, "apis", env);
        AddConfigurationFiles(configBuilder, configurationPaths, "auth", env);
        AddConfigurationFiles(configBuilder, configurationPaths, "cors", env);
        AddConfigurationFiles(configBuilder, configurationPaths, "db", env);
        AddConfigurationFiles(configBuilder, configurationPaths, "logging", env);
        AddConfigurationFiles(configBuilder, configurationPaths, "cache", env);
        AddConfigurationFiles(configBuilder, configurationPaths, "permissions", env);
        AddConfigurationFiles(configBuilder, configurationPaths, "api-keys", env);


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

        // HttpContextAccessor
        builder.Services.AddHttpContextAccessor();

        // Services
        builder.Services
         .AddQueryAndBuilderServices()
         .AddValidationServices()
         .AddAuthenticationServices(builder.Configuration.GetSection("Authentication"), builder.Configuration.GetSection("PermissionConfig"))
         .AddHttpServices()
         .AddMongoServices()
         .AddNotificationServices(
            builder.Configuration.GetSection("Notification"), 
            builder.Configuration.GetSection("NotificationTemplates"),
            builder.Configuration.GetSection("EmailApi"),
            builder.Configuration.GetSection("SmsApi")
         )
         .AddBackgroundTasks(builder.Configuration.GetSection("NotificationProcessor"))
         .AddConventionServices()
         .AddFilterBuilderServices()
         .AddCookiesServices();


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
    }

    public static void Configure(WebApplication app)
    {
        app.UseCors("Cors");

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        if (app.Environment.IsProduction())
        {
            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }

        // Authentication
        app.UseAuthentication();

        // MIDDLEWARES
        app.UseApiKeyMiddleware();
        app.UseJwtRevocation();
        app.UseVerifiedUserMiddleware();
        app.UseErrorHandlingMiddleware();

        // Authorization
        app.UseAuthorization();

        app.MapControllers();
    }
}