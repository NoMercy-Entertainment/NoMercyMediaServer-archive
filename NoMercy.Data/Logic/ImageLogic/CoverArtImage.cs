using NoMercy.Providers.CoverArt.Client;

namespace NoMercy.Data.Logic.ImageLogic;

public abstract class CoverArtImage : BaseImage
{
    public static async Task<string> ColorPalette(string type, Uri url, bool? download = true)
    {
        return await BaseImage.ColorPalette(CoverArtCoverArtClient.Download, type, url, download);
    }

    public static async Task<string> MultiColorPalette(IEnumerable<MultiUriType> items, bool? download = true)
    {
        return await BaseImage.MultiColorPalette(CoverArtCoverArtClient.Download, items, download);
    }
}