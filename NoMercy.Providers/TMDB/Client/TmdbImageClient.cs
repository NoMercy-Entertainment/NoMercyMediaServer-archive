using System.Drawing;
using System.Runtime.Versioning;
using NoMercy.Helpers;
using NoMercy.Networking;
using NoMercy.NmSystem;
using Serilog.Events;

namespace NoMercy.Providers.TMDB.Client;

[SupportedOSPlatform("windows10.0.18362")]
public abstract class TmdbImageClient : TmdbBaseClient
{
    public static Task<Bitmap?> Download(string? path, bool? download = true)
    {
        return Task();
        // return Queue().Enqueue(Task, path, true);

        async Task<Bitmap?> Task()
        {
            try
            {
                if (path is null) return null;
                
                var isSvg = path.EndsWith(".svg");
            
                var folder = Path.Join(AppFiles.ImagesPath, "original");
                
                if (!Directory.Exists(folder)) 
                    Directory.CreateDirectory(folder);

                var filePath = Path.Join(folder, path.Replace("/", ""));

                if (File.Exists(filePath))
                {
                    return isSvg ? null : new Bitmap(filePath);
                }

                using HttpClient httpClient = new();
                httpClient.DefaultRequestHeaders.Add("User-Agent", ApiInfo.UserAgent);
                httpClient.BaseAddress = new Uri("https://image.tmdb.org/t/p/");
                httpClient.DefaultRequestHeaders.Add("Accept", "image/*");
                httpClient.Timeout = TimeSpan.FromMinutes(5);

                var url = path.StartsWith("http") ? path : $"original{path}";

                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) throw new Exception("Failed to download image");
                
                if (download is false)
                {
                    return isSvg ? null : new Bitmap(await response.Content.ReadAsStreamAsync());
                }

                if (!File.Exists(filePath))
                    await File.WriteAllBytesAsync(filePath, await response.Content.ReadAsByteArrayAsync());

                {
                    return isSvg ? null : new Bitmap(await response.Content.ReadAsStreamAsync());
                }
            }
            catch (Exception e)
            {
                Logger.MovieDb($"Error downloading image: {path} - {e.Message}", LogEventLevel.Error);
                throw new Exception("Failed to download image");
            }
        }
    }
}