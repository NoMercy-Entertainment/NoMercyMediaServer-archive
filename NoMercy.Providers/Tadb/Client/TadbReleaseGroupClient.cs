using NoMercy.Providers.TADB.Models;

namespace NoMercy.Providers.TADB.Client;

public class TadbReleaseGroupClient : TadbBaseClient
{
    public TadbAlbum? ByMusicBrainzId(Guid id, bool priority = false)
    {
        Dictionary<string, string?> queryParams = new()
        {
            { "i", id.ToString() }
        };

        try
        {
            return Get<TadbAlbumResponse>("album-mb.php", queryParams, priority)
                .Result?.Albums?.FirstOrDefault();
        }
        catch (Exception)
        {
            return null;
        }
    }
}