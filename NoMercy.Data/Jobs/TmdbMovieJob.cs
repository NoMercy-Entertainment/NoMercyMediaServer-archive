using NoMercy.Data.Logic;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.Helpers.system;
using NoMercy.Networking;
using NoMercy.NmSystem;

namespace NoMercy.Data.Jobs;

[Serializable]
public class TmdbMovieJob : IShouldQueue
{
    public int Id { get; set; }
    public Library? Library { get; set; }

    public TmdbMovieJob()
    {
        //
    }

    public TmdbMovieJob(int id, Library? library = null)
    {
        Id = id;
        Library = library;
    }

    public async Task Handle()
    {
        await using MediaContext context = new();

        if (Library is null) return;

        await using MovieLogic movie = new(Id, Library);
        await movie.Process();
        if (movie.Movie != null)
        {
            Logger.MovieDb($"Movie {movie.Movie.Title}: Processed");

            Networking.Networking.SendToAll("RefreshLibrary", "socket", new RefreshLibraryDto
            {
                QueryKey = ["movie", movie.Movie.Id.ToString()]
            });
        }
    }
}