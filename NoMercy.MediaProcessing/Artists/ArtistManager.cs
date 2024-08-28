using NoMercy.MediaProcessing.Common;
using NoMercy.MediaProcessing.Jobs;
using NoMercy.Providers.MusicBrainz.Models;

namespace NoMercy.MediaProcessing.Artists;

public class ArtistManager(
    IArtistRepository artistRepository,
    JobDispatcher jobDispatcher
) : BaseManager, IArtistManager
{
    public Task StoreArtistAsync(MusicBrainzArtistAppends artist)
    {
        throw new NotImplementedException();
    }
}