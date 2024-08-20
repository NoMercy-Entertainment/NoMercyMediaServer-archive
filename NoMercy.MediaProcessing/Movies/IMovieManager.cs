using NoMercy.Database.Models;

namespace NoMercy.MediaProcessing.Movies;

public interface IMovieManager
{
    Task AddMovieAsync(int id, Library library);
    Task UpdateMovieAsync(int id, Library library);
    Task RemoveMovieAsync(int id);
}