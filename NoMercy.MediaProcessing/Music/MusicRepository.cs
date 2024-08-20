using NoMercy.Database;
using NoMercy.Database.Models;

namespace NoMercy.MediaProcessing.Music;

public class MusicRepository(MediaContext context) : IMusicRepository
{
    public async Task AddAsync(Movie movie)
    {
        await Task.CompletedTask;
    }
}