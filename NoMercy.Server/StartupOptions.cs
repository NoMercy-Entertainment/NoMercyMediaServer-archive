using CommandLine;
using NoMercy.Networking;
using NoMercy.NmSystem;

namespace NoMercy.Server;

public class StartupOptions
{
    // dev
    [Option('d', "dev", Required = false, HelpText = "Run the server in development mode.")]
    public bool Dev { get; set; }
    
    [Option('i', "internal-port", Required = false, HelpText = "Internal port to use for the server.")]
    public int InternalPort { get; set; }
    
    [Option('e', "external-port", Required = false, HelpText = "External port to use for the server.")]
    public int ExternalPort { get; set; }

    public Dictionary<string, string> ConvertToConfig()
    {
        Dictionary<string, string> config = new();
        
        if (Dev)
        {
            config.Add("Dev", "true");
            Logger.App("Running in development mode.");
            Config.IsDev = true;
            Config.AppBaseUrl = "https://app-dev.nomercy.tv/";
            Config.ApiBaseUrl = "https://api-dev.nomercy.tv/";
            Config.AuthBaseUrl = "https://auth-dev.nomercy.tv/realms/NoMercyTV/";
            Config.TokenClientSecret = "1lHWBazSTHfBpuIzjAI6xnNjmwUnryai";
        }

        if (InternalPort != 0)
        {
            config.Add("InternalPort", InternalPort.ToString());
            Logger.App("Setting internal port to " + InternalPort);
            Config.InternalServerPort = InternalPort;
        }
        
        if (ExternalPort != 0)
        {
            config.Add("ExternalPort", ExternalPort.ToString());
            Logger.App("Setting internal port to " + ExternalPort);
            Config.ExternalServerPort = ExternalPort;
        }
        
        return config;
    }
}