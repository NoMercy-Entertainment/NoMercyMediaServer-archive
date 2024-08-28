using NoMercy.Providers.MusicBrainz.Models;

namespace NoMercy.MediaProcessing.MusicGenres;

public interface IMusicGenreManager
{
    public Task StoreMusicGenreAsync(MusicBrainzGenreDetails genre);
}