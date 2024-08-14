namespace NoMercy.Networking;

public class Config
{
    
    public static readonly string AuthBaseUrl = "https://auth-dev.nomercy.tv/realms/NoMercyTV/";
    public static readonly string TokenClientSecret = "1lHWBazSTHfBpuIzjAI6xnNjmwUnryai";
    public static readonly string TokenClientId = "nomercy-server";
    
    public static readonly string ApiBaseUrl = "https://api-dev.nomercy.tv/";
    public static readonly string ApiServerBaseUrl = $"{ApiBaseUrl}v1/server/";
    
    public static int InternalServerPort  { get; set; } = 7626;
    public static int ExternalServerPort  { get; set; } = 7626;
    
    public static readonly string AppDataPath =
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public static readonly string AppPath = Path.Combine(AppDataPath, "NoMercy_C#");

    public static readonly string ConfigPath = Path.Combine(AppPath, "config");
    public static readonly string TokenFile = Path.Combine(ConfigPath, "token.json");
    public static readonly string ConfigFile = Path.Combine(ConfigPath, "config.json");
    
}