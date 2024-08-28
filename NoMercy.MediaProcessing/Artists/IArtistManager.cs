using NoMercy.Providers.MusicBrainz.Models;

namespace NoMercy.MediaProcessing.Artists;

public interface IArtistManager
{
    public Task StoreArtistAsync(MusicBrainzArtistAppends artist);
}