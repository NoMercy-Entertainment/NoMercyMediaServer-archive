using NoMercy.Database;
using NoMercy.Database.Models;

namespace NoMercy.MediaProcessing.Shows;

public class ShowRepository(MediaContext context) : IShowRepository
{
    public async Task AddAsync(Movie movie)
    {
        await context.Movies.AddAsync(movie);
        await context.SaveChangesAsync();
    }
}