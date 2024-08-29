using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
        if (args.Length > 0)
            if (args[0].StartsWith("-loglevel"))
            {
                string[] logLevelArgs = args[0].Split("=");
                if (Enum.TryParse(logLevelArgs[1], true, out LogEventLevel logLevel))
                {
                    Logger.App("Setting log level to " + logLevel);
                    Logger.SetLogLevel(logLevel);
                }
                else
                {
                    Logger.App("Invalid log level, using default.");
                }
            }

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
        // Console.Clear();
        Console.Title = "NoMercy Server";

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
        return WebHost.CreateDefaultBuilder([])
            .ConfigureKestrel(Certificate.KestrelConfig)
            .UseUrls("https://0.0.0.0:" + Config.InternalServerPort)
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