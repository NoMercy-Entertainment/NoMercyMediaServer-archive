using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using NoMercy.Api.Constraints;
using NoMercy.Api.Controllers.Socket;
using NoMercy.Api.Middleware;
using NoMercy.Data.Repositories;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.MediaProcessing.Collections;
using NoMercy.MediaProcessing.Episodes;
using NoMercy.MediaProcessing.Libraries;
using NoMercy.MediaProcessing.Movies;
using NoMercy.MediaProcessing.People;
using NoMercy.MediaProcessing.Seasons;
using NoMercy.MediaProcessing.Shows;
using NoMercy.Networking;
using NoMercy.NmSystem;
using NoMercy.Providers.File;
using NoMercy.Queue;
using CollectionRepository = NoMercy.Data.Repositories.CollectionRepository;
using ICollectionRepository = NoMercy.Data.Repositories.ICollectionRepository;
using ILibraryRepository = NoMercy.Data.Repositories.ILibraryRepository;
using IMovieRepository = NoMercy.Data.Repositories.IMovieRepository;
using LibraryRepository = NoMercy.Data.Repositories.LibraryRepository;
using MovieRepository = NoMercy.Data.Repositories.MovieRepository;

namespace NoMercy.Server;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Add Memory Cache
        services.AddMemoryCache();

        // Add Singleton Services
        services.AddSingleton<JobQueue>();
        services.AddSingleton<Helpers.Monitoring.ResourceMonitor>();
        services.AddSingleton<Networking.Networking>();
        services.AddSingleton(LibraryFileWatcher.Instance);

        // Add DbContexts
        services.AddDbContext<QueueContext>(optionsAction =>
        {
            optionsAction.UseSqlite($"Data Source={AppFiles.QueueDatabase}");
        });
        services.AddTransient<QueueContext>();

        services.AddDbContext<MediaContext>(optionsAction =>
        {
            optionsAction.UseSqlite($"Data Source={AppFiles.MediaDatabase} Pooling=True",
                o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        });
        services.AddTransient<MediaContext>();

        // Add Repositories
        services.AddScoped<IEncoderRepository, EncoderRepository>();
        services.AddScoped<ILibraryRepository, LibraryRepository>();
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<IFolderRepository, FolderRepository>();
        services.AddScoped<ILanguageRepository, LanguageRepository>();
        services.AddScoped<ICollectionRepository, CollectionRepository>();
        services.AddScoped<IGenreRepository, GenreRepository>();
        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<ITvShowRepository, TvShowRepository>();

        // Add Managers
        // services.AddScoped<IEncoderManager, EncoderManager>();
        services.AddScoped<ILibraryManager, LibraryManager>();
        services.AddScoped<IMovieManager, MovieManager>();
        services.AddScoped<ICollectionManager, CollectionManager>();
        services.AddScoped<IShowManager, ShowManager>();
        services.AddScoped<ISeasonManager, SeasonManager>();
        services.AddScoped<IEpisodeManager, EpisodeManager>();
        services.AddScoped<IPersonManager, PersonManager>();
        

        // Add Controllers and JSON Options
        services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.Configure<RouteOptions>(options =>
        {
            options.ConstraintMap.Add("ulid", typeof(UlidRouteConstraint));
        });

        // Configure Logging
        services.AddLogging(builder =>
        {
            builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
        });

        // Configure Authorization
        services.AddAuthorizationBuilder()
            .AddPolicy("api", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddAuthenticationSchemes(IdentityConstants.BearerScheme);
                policy.RequireClaim("scope", "openid", "profile");
                policy.AddRequirements(new AssertionRequirement(context =>
                {
                    using MediaContext mediaContext = new();
                    User? user = mediaContext.Users
                        .FirstOrDefault(user =>
                            user.Id == Guid.Parse(context.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                                                  string.Empty));
                    return user is not null;
                }));
            });

        // Configure Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = Config.AuthBaseUrl;
                options.RequireHttpsMetadata = true;
                options.Audience = "nomercy-ui";
                options.Audience = "nomercy-server";
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        StringValues accessToken = context.Request.Query["access_token"];
                        string[] result = accessToken.ToString().Split('&');

                        if (result.Length > 0 && !string.IsNullOrEmpty(result[0])) context.Token = result[0];

                        return Task.CompletedTask;
                    }
                };
            });

        // Add Other Services
        services.AddCors();
        services.AddApiVersioning();
        services.AddDirectoryBrowser();
        services.AddResponseCaching();
        services.AddMvc(option => option.EnableEndpointRouting = false);
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(type => type.ToString());
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "NoMercy API",
                Version = "v1",
                Description = "NoMercy API",
                Contact = new OpenApiContact
                {
                    Name = "NoMercy",
                    Email = "stoney@nomercy.tv",
                    Url = new Uri("https://nomercy.tv")
                }
            });
            options.AddSecurityDefinition("Keycloak", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    Implicit = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl =
                            new Uri($"{Config.AuthBaseUrl}/protocol/openid-connect/auth"),
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "openid" },
                            { "profile", "profile" }
                        }
                    }
                }
            });

            OpenApiSecurityScheme keycloakSecurityScheme = new()
            {
                Reference = new OpenApiReference
                {
                    Id = "Keycloak",
                    Type = ReferenceType.SecurityScheme
                },
                In = ParameterLocation.Header,
                Name = "Bearer",
                Scheme = "Bearer"
            };

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { keycloakSecurityScheme, Array.Empty<string>() },
                {
                    new OpenApiSecurityScheme
                        { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                    new string[] { }
                }
            });
        });

        services.AddHttpContextAccessor();
        services.AddSignalR(o =>
            {
                o.EnableDetailedErrors = true;
                o.MaximumReceiveMessageSize = 1024 * 1000 * 100;
            })
            .AddNewtonsoftJsonProtocol(options => { options.PayloadSerializerSettings = JsonHelper.Settings; });

        services.AddCors(options =>
        {
            options.AddPolicy("AllowNoMercyOrigins",
                builder =>
                {
                    builder
                        .WithOrigins("https://dev.nomercy.tv")
                        .WithOrigins("https://nomercy.tv")
                        .WithOrigins("https://app-dev.nomercy.tv")
                        .WithOrigins("https://app.nomercy.tv")
                        .WithOrigins("https://vue-dev.nomercy.tv")
                        .WithOrigins("https://vue.nomercy.tv")
                        .WithOrigins("https://app-vilt.nomercy.tv")
                        .WithOrigins("https://vilt.nomercy.tv")
                        .WithOrigins("https://cast.nomercy.tv")
                        .WithOrigins("https://vscode.nomercy.tv")
                        .WithOrigins("https://hlsjs.video-dev.org")
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .WithHeaders("Access-Control-Allow-Private-Network", "true")
                        .AllowAnyHeader();
                });
        });

        services.AddResponseCompression(options => { options.EnableForHttps = true; });

        services.AddTransient<DynamicStaticFilesMiddleware>();
    }

    public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseCors("AllowNoMercyOrigins");

        // Security Middleware
        app.UseHsts();
        app.UseHttpsRedirection();

        // Performance Middleware
        app.UseResponseCompression();
        app.UseRequestLocalization();
        app.UseResponseCaching();

        // Custom Middleware
        app.UseMiddleware<LocalizationMiddleware>();
        app.UseMiddleware<TokenParamAuthMiddleware>();

        // Authentication and Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Logging Middleware
        app.UseMiddleware<AccessLogMiddleware>();

        // Static Files Middleware
        app.UseMiddleware<DynamicStaticFilesMiddleware>();

        // Development Tools
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "NoMercy API");
            options.RoutePrefix = string.Empty;
            options.OAuthClientId("nomercy-server");
            options.OAuthScopes("openid");
            options.DocumentTitle = "NoMercy MediaServer API";
            options.EnablePersistAuthorization();
            options.EnableTryItOutByDefault();
        });

        // MVC
        app.UseMvcWithDefaultRoute();

        // WebSockets
        app.UseWebSockets()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapHub<VideoHub>("/socket", options =>
                {
                    options.Transports = HttpTransportType.WebSockets;
                    options.CloseOnAuthenticationExpiration = true;
                });

                endpoints.MapHub<DashboardHub>("/dashboardHub", options =>
                {
                    options.Transports = HttpTransportType.WebSockets;
                    options.CloseOnAuthenticationExpiration = true;
                });
            });

        // Static Files
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(AppFiles.TranscodePath),
            RequestPath = new PathString("/transcode"),
            ServeUnknownFileTypes = true,
            HttpsCompression = HttpsCompressionMode.Compress
        });

        app.UseDirectoryBrowser(new DirectoryBrowserOptions
        {
            FileProvider = new PhysicalFileProvider(AppFiles.TranscodePath),
            RequestPath = new PathString("/transcode")
        });

        // Initialize Dynamic Static Files Middleware
        MediaContext mediaContext = new();
        List<Folder> folderLibraries = mediaContext.Folders.ToList();

        foreach (Folder folder in folderLibraries.Where(folder => Directory.Exists(folder.Path)))
            DynamicStaticFilesMiddleware.AddPath(folder.Id, folder.Path);
    }
}