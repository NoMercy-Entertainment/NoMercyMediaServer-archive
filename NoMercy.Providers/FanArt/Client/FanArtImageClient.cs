using System.Drawing;
using System.Runtime.Versioning;
using AcoustID;
using NoMercy.Helpers;
using NoMercy.Networking;
using NoMercy.NmSystem;
using NoMercy.Providers.CoverArt.Models;

namespace NoMercy.Providers.FanArt.Client;

public class FanArtImageClient : FanArtBaseClient
{
    public FanArtImageClient()
    {
        Configuration.ClientKey = ApiInfo.AcousticIdKey;
    }

    public Task<CoverArtCovers?> Cover(bool priority = false)
    {
        Dictionary<string, string?> queryParams = new()
        {
            //
        };

        return Get<CoverArtCovers>("release/" + Id, queryParams, priority);
    }

    // [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    [SupportedOSPlatform("windows10.0.18362")]
    public static async Task<Bitmap?> Download(Uri url, bool? download = true)
    {
        var filePath = Path.Combine(AppFiles.MusicImagesPath, Path.GetFileName(url.LocalPath));
        
        if (File.Exists(filePath)) return new Bitmap(filePath);
        
        HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.Add("User-Agent", ApiInfo.UserAgent);
        httpClient.DefaultRequestHeaders.Add("Accept", "image/*");
        httpClient.BaseAddress = new Uri("https://assets.fanart.tv");

        var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        
        var stream = await response.Content.ReadAsStreamAsync();
                
        if (download is false) return new Bitmap(stream);

        if (!File.Exists(filePath))
            await File.WriteAllBytesAsync(filePath, await response.Content.ReadAsByteArrayAsync());
        
        return new Bitmap(stream);
    }
}