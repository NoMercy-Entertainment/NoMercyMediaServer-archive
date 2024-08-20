using NoMercy.Database.Models;
using NoMercy.Providers.MusicBrainz.Client;
using NoMercy.Queue;

namespace NoMercy.MediaProcessing.Music;

public class MusicManager(
    IMusicRepository musicRepository,
    MusicBrainzBaseClient musicClientManager,
    JobDispatcher jobDispatcher)
    : IMusicManager
{
    private readonly IMusicRepository _musicRepository = musicRepository;
    private readonly MusicBrainzBaseClient _tmdbMusicClient = musicClientManager;
    private readonly JobDispatcher _jobDispatcher = jobDispatcher;

    public Task AddMusicAsync(int id, Library library)
    {
        throw new NotImplementedException();
    }

    public Task UpdateMusicAsync(int id, Library library)
    {
        throw new NotImplementedException();
    }

    public Task RemoveMusicAsync(int id)
    {
        throw new NotImplementedException();
    }
}