using NoMercy.Providers.FanArt.Client;

namespace NoMercy.Data.Logic.ImageLogic;

public abstract class FanArtImage : BaseImage
{
    public static async Task<string> ColorPalette(string type, Uri url, bool? download = true)
    {
        return await BaseImage.ColorPalette(FanArtImageClient.Download, type, url, download);
    }

    public static async Task<string> MultiColorPalette(IEnumerable<MultiUriType> items, bool? download = true)
    {
        return await BaseImage.MultiColorPalette(FanArtImageClient.Download, items, download);
    }
}