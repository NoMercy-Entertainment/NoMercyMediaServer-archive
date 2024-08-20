using NoMercy.Database.Models;

namespace NoMercy.MediaProcessing.Music;

public interface IMusicRepository
{
    public Task AddAsync(Movie movie);
}