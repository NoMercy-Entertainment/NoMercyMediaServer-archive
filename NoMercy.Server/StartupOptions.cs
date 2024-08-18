using CommandLine;

namespace NoMercy.Server;

public class StartupOptions
{
    [Option('d', "datadir", Required = false, HelpText = "Path to use for the data folder (database files, etc.).")]
    public string? DataDir { get; set; }

    [Option("nowebclient", Required = false,
        HelpText = "Indicates that the web Server should not host the web client.")]
    public bool NoWebClient { get; set; }

    [Option('w', "webdir", Required = false, HelpText = "Path to the Jellyfin web UI resources.")]
    public string? WebDir { get; set; }

    [Option('C', "cachedir", Required = false, HelpText = "Path to use for caching.")]
    public string? CacheDir { get; set; }

    [Option('c', "configdir", Required = false,
        HelpText = "Path to use for configuration data (User settings and pictures).")]
    public string? ConfigDir { get; set; }

    [Option('l', "logdir", Required = false, HelpText = "Path to use for writing log files.")]
    public string? LogDir { get; set; }

    [Option("ffmpeg", Required = false,
        HelpText = "Path to external FFmpeg executable to use in place of default found in PATH.")]
    public string? FFmpegPath { get; set; }

    [Option("service", Required = false, HelpText = "Run as headless service.")]
    public bool IsService { get; set; }

    [Option("package-name", Required = false, HelpText = "Used when packaging Jellyfin (example, synology).")]
    public string? PackageName { get; set; }

    [Option("restartpath", Required = false, HelpText = "Path to restart script.")]
    public string? RestartPath { get; set; }

    [Option("restartargs", Required = false, HelpText = "Arguments for restart script.")]
    public string? RestartArgs { get; set; }

    [Option("published-Server-url", Required = false,
        HelpText = "Jellyfin Server URL to publish via auto discover process")]
    public string? PublishedServerUrl { get; set; }

    public Dictionary<string, string> ConvertToConfig()
    {
        Dictionary<string, string> config = new();

        // if (NoWebClient)
        // {
        //     config.Add(HostWebClientKey, bool.FalseString);
        // }
        //
        // if (PublishedServerUrl != null)
        // {
        //     config.Add(AddressOverrideKey, PublishedServerUrl);
        // }
        //
        // if (FFmpegPath != null)
        // {
        //     config.Add(FfmpegPathKey, FFmpegPath);
        // }

        return config;
    }
}