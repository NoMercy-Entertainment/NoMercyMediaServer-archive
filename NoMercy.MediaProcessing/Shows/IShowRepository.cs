using NoMercy.Database.Models;

namespace NoMercy.MediaProcessing.Shows;

public interface IShowRepository
{
    public Task AddAsync(Movie movie);
}