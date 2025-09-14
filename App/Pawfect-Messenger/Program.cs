using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Pawfect_Messenger.Data.Entities.Types.Authentication;
using Pawfect_Messenger.Data.Entities.Types.Cache;
using Pawfect_Messenger.DevTools;
using Pawfect_Messenger.Middlewares;
using Pawfect_Messenger.Services.AuthenticationServices;
using Pawfect_Messenger.Services.AuthenticationServices.Extentions;
using Pawfect_Messenger.Services.AwsServices.Extention;
using Pawfect_Messenger.Services.Convention.Extention;
using Pawfect_Messenger.Services.ConversationServices.Extention;
using Pawfect_Messenger.Services.FileServices.Extention;
using Pawfect_Messenger.Services.FilterServices.Extensions;
using Pawfect_Messenger.Services.MessageServices.Extentions;
using Pawfect_Messenger.Services.MongoServices.Extentions;
using Pawfect_Messenger.Services.QueryServices.Extentions;
using Pawfect_Messenger.Services.ValidationServices.Extentions;

using Serilog;

using System.Text;
using Pawfect_Messenger.Data.Entities.Types.Apis;
using Pawfect_Messenger.Data.Entities.Types.Authorisation;
using Pawfect_Messenger.Data.Entities.Types.Mongo;
using Pawfect_Messenger.Data.Entities.Types.RateLimiting;
using Pawfect_Messenger.Hubs.ChatHub;
using Pawfect_Messenger.Services.PresenceServices.Extentions;
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
        AddConfigurationFiles(configBuilder, configurationPaths, "api-keys", env);
        AddConfigurationFiles(configBuilder, configurationPaths, "rate-limit", env);


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
        .AddAuthenticationServices(builder.Configuration.GetSection("Authentication"), builder.Configuration.GetSection("PermissionConfig"))
        .AddConversationServices()
        .AddMessageServices()
        .AddMongoServices()
        .AddConventionServices()
        .AddAwsServices(builder.Configuration.GetSection("Aws"))
        .AddFileServices(builder.Configuration.GetSection("Files"))
        .AddPresenceServices()
        .AddFilterBuilderServices()
        .AddSignalR();


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
                    PathString path = context.HttpContext.Request.Path;

                    // Allow hub connections to auth via querystring
                    if (path.StartsWithSegments("/hubs/chat"))
                    {
                        Microsoft.Extensions.Primitives.StringValues qsToken = context.Request.Query["access_token"];
                        if (!String.IsNullOrEmpty(qsToken))
                        {
                            context.Token = qsToken;
                            return Task.CompletedTask;
                        }

                        Microsoft.Extensions.Primitives.StringValues alt = context.Request.Query[JwtService.ACCESS_TOKEN];
                        if (!String.IsNullOrEmpty(alt))
                        {
                            context.Token = alt;
                            return Task.CompletedTask;
                        }
                    }

                    String? token = context.Request.Cookies[JwtService.ACCESS_TOKEN];
                    if (!String.IsNullOrEmpty(token)) context.Token = token;

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
        app.UseRateLimitMiddleware();
        app.UseApiKeyMiddleware();
        app.UseJwtRevocation();
        app.UseVerifiedUserMiddleware();
        app.UseErrorHandlingMiddleware();

        // Authorization
        app.UseAuthorization();

        app.MapControllers();

        app.MapHub<ChatHub>("/hubs/chat");
    }
}