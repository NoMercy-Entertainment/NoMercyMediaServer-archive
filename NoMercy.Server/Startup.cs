using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using NoMercy.Database;
using NoMercy.Helpers;
using NoMercy.Networking;
using NoMercy.Server.app.Constraints;
using NoMercy.Server.app.Http.Controllers.Socket;
using NoMercy.Server.app.Http.Middleware;
using NoMercy.Server.system;
using AppFiles = NoMercy.NmSystem.AppFiles;
using Uri = System.Uri;

namespace NoMercy.Server;

public class Startup(IConfiguration _)
{
    
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddLogging();
        // services.AddSingleton<AniDbBaseClient>();
        services.AddSingleton<JobQueue>();
        services.AddScoped<Networking.Networking>();
        
        // services.AddControllers().AddJsonOptions(jsonOptions =>
        // {
        //     jsonOptions.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        //     jsonOptions.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        //     jsonOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        //     jsonOptions.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
        //     
        // });

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

        services.AddControllers().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        });

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.AddAuthorizationBuilder()
            .AddPolicy("api", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddAuthenticationSchemes(IdentityConstants.BearerScheme);
                policy.RequireClaim("scope", "openid", "profile");
                policy.AddRequirements([
                    new AssertionRequirement(context =>
                    {
                        using MediaContext mediaContext = new();
                        var user = mediaContext.Users
                            .FirstOrDefault(user =>
                                user.Id == Guid.Parse(context.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                                                      string.Empty));
                        return user is not null;
                    })
                ]);
            });

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
                        var accessToken = context.Request.Query["access_token"];
                        var result = accessToken.ToString().Split('&');

                        if (result.Length > 0 && !string.IsNullOrEmpty(result[0])) context.Token = result[0];

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        services.AddCors();
        services.AddApiVersioning();
        services.AddDirectoryBrowser();

        // services.AddResponseCaching();
        services.AddMvc(option => option.EnableEndpointRouting = false);

        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

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
                            new Uri("https://auth-dev.nomercy.tv/realms/NoMercyTV/protocol/openid-connect/auth"),
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "openid" },
                            { "profile", "profile" }
                        }
                    }
                }
            });

            // options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            // {
            //     Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            //     Name = "Authorization",
            //     In = ParameterLocation.Header,
            //     Type = SecuritySchemeType.Http,
            //     Scheme = "bearer",
            //     BearerFormat = "JWT"
            // });

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
        .AddNewtonsoftJsonProtocol(options =>
        {
            options.PayloadSerializerSettings = JsonHelper.Settings;
        });

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

                        // .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .WithHeaders("Access-Control-Allow-Private-Network", "true")
                        .AllowAnyHeader();
                });
        });

        services.AddResponseCompression(options => { options.EnableForHttps = true; });
        
        services.Configure<RouteOptions>(options =>
        {
            options.ConstraintMap.Add("ulid", typeof(UlidRouteConstraint));
        });
        
        services.AddTransient<DynamicStaticFilesMiddleware>();
    }

    public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        
        app.UseRouting();
        app.UseCors("AllowNoMercyOrigins");

        app.UseHsts();
        app.UseHttpsRedirection();
        app.UseResponseCompression();
        app.UseRequestLocalization();
        // app.UseResponseCaching();

        app.UseMiddleware<LocalizationMiddleware>();
        app.UseMiddleware<TokenParamAuthMiddleware>();

        app.UseAuthentication();

        app.UseAuthorization();
        
        app.UseMiddleware<AccessLogMiddleware>();

        app.UseMiddleware<DynamicStaticFilesMiddleware>();

        // if (env.IsDevelopment())
        // {
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
        // }

        app.UseMvcWithDefaultRoute();

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
        
        var mediaContext = new MediaContext();
        var folderLibraries = mediaContext.Folders.ToList();
        
        foreach (var folder in folderLibraries.Where(folder => Directory.Exists(folder.Path)))
        {
            DynamicStaticFilesMiddleware.AddPath(folder.Id, folder.Path);
        }
    }
}