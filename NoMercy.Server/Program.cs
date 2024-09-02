using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using CommandLine;
using Microsoft.AspNetCore;
using NoMercy.Data.Logic;
using NoMercy.Database;
using NoMercy.Networking;
using NoMercy.NmSystem;
using NoMercy.Providers.AniDb.Clients;
using NoMercy.Queue;
using NoMercy.Server.app.Helper;
using Serilog.Events;
using AppFiles = NoMercy.NmSystem.AppFiles;

namespace NoMercy.Server;

public static class Program
{
    public static Task Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
        {
            Exception exception = (Exception)eventArgs.ExceptionObject;
            Logger.App("UnhandledException " + exception);
        };

        Console.CancelKeyPress += (_, _) =>
        {
            Shutdown().Wait();
            Environment.Exit(0);
        };

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            Logger.App("SIGTERM received, shutting down.");
            Shutdown().Wait();
        };

        return Parser.Default.ParseArguments<StartupOptions>(args)
            .MapResult(Start, ErrorParsingArguments);

        static Task ErrorParsingArguments(IEnumerable<Error> errors)
        {
            Environment.ExitCode = 1;
            return Task.CompletedTask;
        }
    }

    private static async Task Start(StartupOptions options)
    {
        Console.Clear();
        Console.Title = "NoMercy Server";
        
        if (options.Dev)
        {
            Logger.App("Running in development mode.");
            
            Config.IsDev = true;
            
            Config.AppBaseUrl = "https://app-dev.nomercy.tv/";
            Config.ApiBaseUrl = "https://api-dev.nomercy.tv/";
            Config.ApiServerBaseUrl = $"{Config.ApiBaseUrl}v1/server/";
            
            Config.AuthBaseUrl = "https://auth-dev.nomercy.tv/realms/NoMercyTV/";
            Config.TokenClientSecret = "1lHWBazSTHfBpuIzjAI6xnNjmwUnryai";
        }
        
        if (!string.IsNullOrEmpty(options.LogLevel))
        {
            Logger.App($"Setting log level to: {options.LogLevel}.");
            Logger.SetLogLevel(Enum.Parse<LogEventLevel>(options.LogLevel.ToTitleCase()));
        }
        
        Logger.App(Config.AuthBaseUrl);

        if (options.InternalPort != 0)
        {
            Logger.App("Setting internal port to " + options.InternalPort);
            Config.InternalServerPort = options.InternalPort;
        }
        
        if (options.ExternalPort != 0)
        {
            Logger.App("Setting external port to " + options.ExternalPort);
            Config.ExternalServerPort = options.ExternalPort;
        }

        Stopwatch stopWatch = new();
        stopWatch.Start();

        Databases.QueueContext = new QueueContext();
        Databases.MediaContext = new MediaContext();

        await Init();

        IWebHost app = CreateWebHostBuilder(new WebHostBuilder()).Build();

        app.Services.GetService<IHostApplicationLifetime>()?.ApplicationStarted.Register(() =>
        {
            Task.Run(() =>
            {
                stopWatch.Stop();

                Task.Delay(300).Wait();

                Logger.App($"Internal Address: {Networking.Networking.InternalAddress}");
                Logger.App($"External Address: {Networking.Networking.ExternalAddress}");

                ConsoleMessages.ServerRunning();

                Logger.App($"Server started in {stopWatch.ElapsedMilliseconds}ms");
            });
        });

        new Thread(() => app.RunAsync()).Start();
        new Thread(Dev.Run).Start();

        await Task.Delay(-1);
    }

    private static async Task Shutdown()
    {
        await Task.CompletedTask;
    }

    private static async Task Restart()
    {
        await Task.CompletedTask;
    }

    private static IWebHostBuilder CreateWebHostBuilder(this IWebHostBuilder _)
    {
        var url = new UriBuilder
        {
            Host = IPAddress.Any.ToString(),
            Port = Config.InternalServerPort,
            Scheme = "https"
        };

        return WebHost.CreateDefaultBuilder([])
            .ConfigureKestrel(Certificate.KestrelConfig)
            .UseUrls(url.ToString())
            .UseKestrel(options =>
            {
                options.AddServerHeader = false;
                options.Limits.MaxRequestBodySize = null;
                options.Limits.MaxRequestBufferSize = null;
                options.Limits.MaxConcurrentConnections = null;
                options.Limits.MaxConcurrentUpgradedConnections = null;
            })
            .UseQuic()
            .UseSockets()
            .ConfigureServices(services =>
            {
                services.AddSingleton<IApiVersionDescriptionProvider, DefaultApiVersionDescriptionProvider>();
                services.AddSingleton<ISunsetPolicyManager, DefaultSunsetPolicyManager>();
            })
            .UseStartup<Startup>();
    }

    private static async Task Init()
    {
        ApiInfo.RequestInfo().Wait();

        List<Task> startupTasks =
        [
            ConsoleMessages.Logo(),
            AppFiles.CreateAppFolders(),
            Networking.Networking.Discover(),
            Auth.Init(),
            Seed.Init(),
            Register.Init(),
            Binaries.DownloadAll(),
            // AniDbBaseClient.Init(),
            TrayIcon.Make()
        ];

        AppDomain.CurrentDomain.ProcessExit += (_, _) => { AniDbBaseClient.Dispose(); };

        await RunStartup(startupTasks);
        
        await using MediaContext mediaContext = new();
        var configuration = mediaContext.Configuration.ToList();
        
        foreach (var config in configuration)
        {
            Logger.App($"Configuration: {config.Key} = {config.Value}");
            if (config.Key == "InternalServerPort")
            {
                Config.InternalServerPort = int.Parse(config.Value);
            }
            else if (config.Key == "ExternalServerPort")
            {
                Config.ExternalServerPort = int.Parse(config.Value);
            }
        }

        Thread t = new(new Task(() => QueueRunner.Initialize().Wait()).Start)
        {
            Name = "Queue workers",
            Priority = ThreadPriority.Lowest,
            IsBackground = true
        };
        t.Start();
    }

    private static async Task RunStartup(List<Task> startupTasks)
    {
        foreach (Task task in startupTasks)
        {
            if (task.IsCompleted) continue;
            task.Start();
        }

        await Task.CompletedTask;
    }
}

internal class DefaultSunsetPolicyManager : ISunsetPolicyManager
{
    public bool TryGetPolicy(string? name, ApiVersion? apiVersion, [MaybeNullWhen(false)] out SunsetPolicy sunsetPolicy)
    {
        sunsetPolicy = new SunsetPolicy();
        return true;
    }
}