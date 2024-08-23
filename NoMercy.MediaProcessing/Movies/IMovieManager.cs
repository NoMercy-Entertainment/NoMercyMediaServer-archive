using NoMercy.Database.Models;
using NoMercy.Providers.TMDB.Models.Movies;

namespace NoMercy.MediaProcessing.Movies;

public interface IMovieManager
{
    Task<TmdbMovieAppends?> AddMovieAsync(int id, Library library);
    Task UpdateMovieAsync(int id, Library library);
    Task RemoveMovieAsync(int id, Library library);
}