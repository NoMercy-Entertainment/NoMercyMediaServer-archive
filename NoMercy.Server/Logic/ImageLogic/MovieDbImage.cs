using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using NoMercy.Providers.TMDB.Client;

namespace NoMercy.Server.Logic.ImageLogic;

[SupportedOSPlatform("windows10.0.18362")]
public abstract class MovieDbImage : BaseImage
{
    public static async Task<string> ColorPalette(string type, string? path, bool? download = true)
    {
        return await BaseImage.ColorPalette(TmdbImageClient.Download, type, path, download);
    }
    
    public static async Task<string> MultiColorPalette(IEnumerable<MultiStringType> items, bool? download = true)
    {
        return await BaseImage.MultiColorPalette(TmdbImageClient.Download, items, download);
    }
}