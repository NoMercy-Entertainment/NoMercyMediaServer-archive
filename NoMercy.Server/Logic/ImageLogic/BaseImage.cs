using System.Drawing;
using System.Runtime.InteropServices;
using ColorThiefDotNet;
using Newtonsoft.Json;
using NoMercy.Database;

namespace NoMercy.Server.Logic.ImageLogic;

public abstract class BaseImage: IDisposable
{
    public delegate Task<Bitmap?> DownloadUrl(Uri path, bool? download);
    public delegate Task<Bitmap?> DownloadPath(string? path, bool? download);

    private class ColorPaletteArgument
    {
        public required string Key { get; set; }
        public Bitmap? Bitmap { get; set; }
    }
    
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MultiUriType(string key, Uri url)
    {
        public readonly string Key = key;
        public readonly Uri Url = url;
    }
    
    public class MultiStringType(string key, string? path)
    {
        public readonly string Key = key;
        public readonly string? Path = path;
    }

    private static string GenerateColorPalette(IEnumerable<ColorPaletteArgument> items)
    {
        Dictionary<string, PaletteColors?> palette = new();
        
        foreach (var item in items)
        {
            palette.Add(item.Key, ColorPaletteFromBitmap(item.Bitmap));
        }

        return JsonConvert.SerializeObject(palette.Where(x => x.Value != null)
            .ToDictionary(x => x.Key, x => x.Value));
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
                #pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                Dominant = null,
                Primary = null,
                LightVibrant = null,
                DarkVibrant = null,
                LightMuted = null,
                DarkMuted = null
                #pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            };
        }
    }
    
    internal static async Task<string> ColorPalette(DownloadUrl client, string type, Uri path, bool? download = true)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return "";
        var bitmap = await client.Invoke(path, download);
        
        return GenerateColorPalette(new List<ColorPaletteArgument>
        {
            new() {
                Key = type,
                Bitmap = bitmap
            }
        });
    }
    
    internal static async Task<string> MultiColorPalette(DownloadUrl client, IEnumerable<MultiUriType> items, bool? download = true)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return "";
        var list = new List<ColorPaletteArgument>();
        foreach (var item in items)
        {
            var bitmap = await client.Invoke(item.Url, download);
            list.Add(new ColorPaletteArgument
            {
                Key = item.Key,
                Bitmap = bitmap
            });
        }

        return GenerateColorPalette(list);
    }
    
    internal static async Task<string> ColorPalette(DownloadPath client, string type, string? path, bool? download = true)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return "";
        var bitmap = await client.Invoke(path, download);
        
        return GenerateColorPalette(new List<ColorPaletteArgument>
        {
            new() {
                Key = type,
                Bitmap = bitmap
            }
        });
    }
    
    internal static async Task<string> MultiColorPalette(DownloadPath client, IEnumerable<MultiStringType> items, bool? download = true)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return "";
        var list = new List<ColorPaletteArgument>();
        foreach (var item in items)
        {
            var bitmap = await client.Invoke(item.Path, download);
            list.Add(new ColorPaletteArgument
            {
                Key = item.Key,
                Bitmap = bitmap
            });
        }

        return GenerateColorPalette(list);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}