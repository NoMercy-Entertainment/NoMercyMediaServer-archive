using System.Drawing;
using System.Runtime.InteropServices;
using ColorThiefDotNet;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NoMercy.Database;
using NoMercy.Providers.CoverArt.Client;
using NoMercy.Providers.FanArt.Client;
using NoMercy.Providers.TMDB.Client;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = System.Drawing.Color;

namespace NoMercy.Server.Logic;

public abstract class ImageLogic2 : IDisposable, IAsyncDisposable
{
    private static PaletteColors ColorPaletteFromImage(Image<Rgba32>?  image)
    {
        if (image is null) 
            return new PaletteColors
            {
                Dominant = null,
                Primary = null,
                LightVibrant = null,
                DarkVibrant = null,
                LightMuted = null,
                DarkMuted = null
            };
        
        var colorCounts = new Dictionary<Rgba32, int>();
        for(var x = 0; x < image.Width; x++)
        {
            for(var y = 0; y < image.Height; y++)
            {
                var color = image[x, y];
                if (!colorCounts.TryAdd(color, 1))
                {
                    colorCounts[color]++;
                }
            }
        }
        
        var sorted = colorCounts
            .OrderByDescending(x => x.Value).ToList();
        
        var dominantColor = sorted.First();
        colorCounts.Remove(dominantColor.Key);
        
        var sorted2 = colorCounts
            .OrderByDescending(x => 
                Color.FromArgb(x.Key.R, x.Key.G, x.Key.B)).ToList();
        
        return new PaletteColors
        {
            Dominant = dominantColor.Key.ToHex(),
            Primary = sorted2[0].Key.ToHex(),
            LightVibrant = sorted2[1].Key.ToHex(),
            LightMuted = sorted2[3].Key.ToHex(),
            DarkVibrant = sorted2[2].Key.ToHex(),
            DarkMuted = sorted2[4].Key.ToHex()
        };
        
    }
    
    private static PaletteColors ColorPaletteFromBitmap(Bitmap? bitmap)
    {
        try
        {
            ColorThief colorThief = new();
            var palette = colorThief.GetPalette(bitmap, 7, 10, false).ToList();

            var sorted = palette.OrderByDescending(x => x.Population).ToList();
            var dominantColor = sorted.First();
            palette.RemoveAt(0);
            var sorted2 = palette.OrderByDescending(x => x.IsDark).ToList();

            return new PaletteColors
            {
                Dominant = dominantColor.Color.ToHexString(),
                Primary = sorted2[0].Color.ToHexString(),
                LightVibrant = sorted2[1].Color.ToHexString(),
                LightMuted = sorted2[3].Color.ToHexString(),
                DarkVibrant = sorted2[2].Color.ToHexString(),
                DarkMuted = sorted2[4].Color.ToHexString()
            };
        }
        catch (Exception)
        {
            return new PaletteColors
            {
                Dominant = null,
                Primary = null,
                LightVibrant = null,
                DarkVibrant = null,
                LightMuted = null,
                DarkMuted = null
            };
        }
    }
    
    private static async Task<PaletteColors> CreateColorPalette(string path, string? size = null, bool? download = false)
    {
        Bitmap? bitmap;

        if (Uri.TryCreate(path, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps) && download is false)
        {
            bitmap = new Bitmap(path);
        }
        else
        {
            if (size is not null)
            {
                bitmap = await TmdbImageClient.Download(path, download);
            }
            else if (path.Contains("fanart"))
            {
                bitmap = await FanArtImageClient.Download(new Uri(path), download);
            }
            else
            {
                bitmap = await CoverArtCoverArtClient.Download(new Uri(path), download);
            }
        }
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return ColorPaletteFromImage(null);
        }
        
        return ColorPaletteFromBitmap(bitmap);
    }

    private static Task<PaletteColors> ColorPaletteFromTmdbImage(string filePath, bool download = true)
    {
        return CreateColorPalette(filePath, "original", download);
    }

    private static Task<PaletteColors> ColorPaletteFromCoverArchiveImage(string coverPath, bool download = true)
    {
        return CreateColorPalette(coverPath, null, download);
    }
    
    private static Task<PaletteColors> ColorPaletteFromFanartImage(string coverPath, bool download = true)
    {
        return CreateColorPalette(coverPath, null, download);
    }

    private static async Task<string> GenerateColorPalette(
        string? posterPath = null,
        string? backdropPath = null,
        string? logoPath = null,
        string? profilePath = null,
        string? filePath = null,
        string? coverPath = null,
        string? stillPath = null,
        string? fanartPath = null,
        bool download = true
    )
    {
        Dictionary<string, PaletteColors?> palette = new()
        {
            {
                "poster", posterPath != null
                    ? await ColorPaletteFromTmdbImage(posterPath, download)
                    : null
            },
            {
                "backdrop", backdropPath != null
                    ? await ColorPaletteFromTmdbImage(backdropPath, download)
                    : null
            },
            {
                "logo", logoPath != null
                    ? await ColorPaletteFromTmdbImage(logoPath, download)
                    : null
            },
            {
                "profile", profilePath != null
                    ? await ColorPaletteFromTmdbImage(profilePath, download)
                    : null
            },
            {
                "still", stillPath != null
                    ? await ColorPaletteFromTmdbImage(stillPath, download)
                    : null
            },
            {
                "image", filePath != null
                    ? await ColorPaletteFromTmdbImage(filePath, download)
                    : null
            },
            {
                "cover", coverPath != null
                    ? await ColorPaletteFromCoverArchiveImage(coverPath, download)
                    : null
            },
            {
                "fanart", fanartPath != null
                    ? await ColorPaletteFromFanartImage(fanartPath, download)
                    : null
            }
        };

        return JsonConvert.SerializeObject(palette.Where(x => x.Value != null)
            .ToDictionary(x => x.Key, x => x.Value));
    }

    public static async Task Palette(string filePath, bool download = true)
    {
        await using MediaContext mediaContext = new();
        var image = await mediaContext.Images
            .Where(e => e._colorPalette == "")
            .FirstOrDefaultAsync(e => e.FilePath == filePath);

        if (image is null) return;

        lock (image)
        {
            if (image is { _colorPalette: "" })
            {
                var palette = GenerateColorPalette(filePath: filePath, download: download).Result;
                image._colorPalette = palette;
                mediaContext.SaveChanges();
            }
        }
    }
    
    public static async Task Fanart(string filePath, bool download = true)
    {
        await using MediaContext mediaContext = new();
        var image = await mediaContext.Images
            .Where(e => e._colorPalette == "")
            .FirstOrDefaultAsync(e => e.FilePath == filePath);

        if (image is null) return;

        lock (image)
        {
            if (image is { _colorPalette: "" })
            {
                var palette = GenerateColorPalette(fanartPath: filePath, download: download).Result;
                image._colorPalette = palette;
                mediaContext.SaveChanges();
            }
        }
    }

    public static async Task MusicPalette(string filePath, bool download = true)
    {
        await using MediaContext mediaContext = new();
        var image = await mediaContext.Images
            .Where(e => e._colorPalette == "")
            .FirstOrDefaultAsync(e => e.FilePath == filePath);

        if (image is null) return;

        lock (image)
        {
            if (image is { _colorPalette: "" })
            {
                var palette = GenerateColorPalette(coverPath: filePath, download: download).Result;
                image._colorPalette = palette;
                mediaContext.SaveChanges();
            }
        }
    }

    public void Dispose()
    {
        GC.Collect();
        GC.WaitForFullGCComplete();
        GC.WaitForPendingFinalizers();
    }

    public ValueTask DisposeAsync()
    {
        GC.Collect();
        GC.WaitForFullGCComplete();
        GC.WaitForPendingFinalizers();
        
        return new ValueTask(Task.CompletedTask);
    }
}