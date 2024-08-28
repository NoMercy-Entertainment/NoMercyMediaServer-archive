using NoMercy.MediaProcessing.Common;
using NoMercy.MediaProcessing.Jobs;
using NoMercy.Providers.MusicBrainz.Models;

namespace NoMercy.MediaProcessing.MusicGenres;

public class MusicGenreManager(
    IMusicGenreRepository genreRepository,
    JobDispatcher jobDispatcher
) : BaseManager, IMusicGenreManager
{
    public Task StoreMusicGenreAsync(MusicBrainzGenreDetails genre)
    {
        throw new NotImplementedException();
    }
}