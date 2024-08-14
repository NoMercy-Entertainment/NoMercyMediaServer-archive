using System.Drawing;
using System.Runtime.Versioning;
using AcoustID;
using NoMercy.Helpers;
using NoMercy.Networking;
using NoMercy.NmSystem;
using NoMercy.Providers.CoverArt.Models;
using Uri = System.Uri;

namespace NoMercy.Providers.CoverArt.Client;

public class CoverArtCoverArtClient : CoverArtBaseClient
{
    public CoverArtCoverArtClient(Guid id) : base(id)
    {
        Configuration.ClientKey = ApiInfo.AcousticIdKey;
    }

    public Task<CoverArtCovers?> Cover(bool priority = false)
    {
        Dictionary<string, string?> queryParams = new()
        {
            //
        };
        
        try
        {
            return Get<CoverArtCovers>("release/" + Id, queryParams, priority);
        }
        catch (Exception)
        {
            return Task.FromResult<CoverArtCovers?>(null);
        }
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

        var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        
        var stream = await response.Content.ReadAsStreamAsync();
                
        if (download is false) return new Bitmap(stream);

        if (!File.Exists(filePath))
            await File.WriteAllBytesAsync(filePath, await response.Content.ReadAsByteArrayAsync());
        
        return new Bitmap(stream);
    }
}