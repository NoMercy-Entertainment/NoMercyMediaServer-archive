using CommandLine;
using NoMercy.NmSystem.Extensions;
using NoMercy.NmSystem.Information;
using NoMercy.NmSystem.SystemCalls;
using Serilog.Events;

namespace NoMercy.Server;

public class StartupOptions
{
    // dev
    [Option('d', "dev", Required = false, HelpText = "Run the server in development mode.")]
    public bool Dev { get; set; }
    
    [Option('l', "loglevel", Required = false, HelpText = "Run the server in development mode.")]
    public string LogLevel { get; set; } = LogEventLevel.Information.ToString();
    
    [Option("seed", Required = false, HelpText = "Run the server in development mode.")]
    public bool Seed { get; set; }
    
    [Option('i', "internal-port", Required = false, HelpText = "Internal port to use for the server.")]
    public int InternalPort { get; set; }
    
    [Option('x', "external-port", Required = false, HelpText = "External port to use for the server.")]
    public int ExternalPort { get; set; }
    
    [Option("internal-ip", Required = false, HelpText = "Internal ip to use for the server.")]
    public string?InternalIp { get; set; }
    
    [Option("external-ip", Required = false, HelpText = "External ip to use for the server.")]
    public string? ExternalIp { get; set; }

    public void ApplySettings(out bool shouldSeedMarvel)
    {
        shouldSeedMarvel = false;

        if (Dev)
        {
            Logger.App("Running in development mode.");

            Config.IsDev = true;

            Config.AppBaseUrl = "https://app-dev.nomercy.tv/";
            Config.ApiBaseUrl = "https://api-dev.nomercy.tv/";
            Config.ApiServerBaseUrl = $"{Config.ApiBaseUrl}v1/server/";

            Config.AuthBaseUrl = "https://auth-dev.nomercy.tv/realms/NoMercyTV/";
            Config.TokenClientSecret = "1lHWBazSTHfBpuIzjAI6xnNjmwUnryai";
        }

        if (Seed)
        {
            Logger.App("Seeding database.");
            shouldSeedMarvel = true;
        }

        if (!string.IsNullOrEmpty(LogLevel))
        {
            Logger.App($"Setting log level to: {LogLevel}.");
            Logger.SetLogLevel(Enum.Parse<LogEventLevel>(LogLevel.ToTitleCase()));
        }

        if (InternalPort != 0)
        {
            Logger.App("Setting internal port to " + InternalPort);
            Config.InternalServerPort = InternalPort;
        }

        if (ExternalPort != 0)
        {
            Logger.App("Setting external port to " + ExternalPort);
            Config.ExternalServerPort = ExternalPort;
        }
        
        if (!string.IsNullOrEmpty(InternalIp))
        {
            Logger.App("Setting internal ip to " + InternalIp);
            Networking.Networking.InternalIp = InternalIp;
        }
        
        if (!string.IsNullOrEmpty(ExternalIp))
        {
            Logger.App("Setting external ip to " + ExternalIp);
            Networking.Networking.ExternalIp = ExternalIp;
        }
       
    }

}