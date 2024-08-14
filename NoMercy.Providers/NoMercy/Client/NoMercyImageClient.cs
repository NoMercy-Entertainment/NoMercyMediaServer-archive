using System.Drawing;
using System.Runtime.Versioning;
using NoMercy.Helpers;
using NoMercy.Networking;
using NoMercy.NmSystem;
using NoMercy.Providers.TMDB.Client;
using Serilog.Events;

namespace NoMercy.Providers.NoMercy.Client;

[SupportedOSPlatform("windows10.0.18362")]
public abstract class NoMercyImageClient : TmdbBaseClient
{
    public static Task<Bitmap?> Download(string? path, bool? download = true)
    {
        return Queue().Enqueue(Task, $"original{path}", true);

        async Task<Bitmap?> Task()
        {
            if (path is null) return null;
            
            try
            {
                var folder = Path.Join(AppFiles.ImagesPath, "original");
                
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                var filePath = Path.Join(folder, path.Replace("/", ""));

                if (File.Exists(filePath)) return new Bitmap(filePath);

                using HttpClient httpClient = new();
                httpClient.DefaultRequestHeaders.Add("User-Agent", ApiInfo.UserAgent);
                httpClient.BaseAddress = new Uri("https://image.nomercy.tv/");
                httpClient.DefaultRequestHeaders.Add("Accept", "image/*");
                httpClient.Timeout = TimeSpan.FromMinutes(5);

                var url = path.StartsWith("http") ? path : $"original{path}";

                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;
                
                if (download is false) 
                    return new Bitmap(await response.Content.ReadAsStreamAsync());

                if (!File.Exists(filePath))
                    await File.WriteAllBytesAsync(filePath, await response.Content.ReadAsByteArrayAsync());
                
                return new Bitmap(await response.Content.ReadAsStreamAsync());
            }
            catch (Exception e)
            {
                Logger.MovieDb($"Error downloading image: {path} - {e.Message}", LogEventLevel.Error);
            }

            return null;
        }
    }
}