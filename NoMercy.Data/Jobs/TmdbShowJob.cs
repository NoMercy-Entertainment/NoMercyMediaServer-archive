using Microsoft.EntityFrameworkCore;
using NoMercy.Data.Logic;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.Networking;
using NoMercy.NmSystem;
using NoMercy.Queue;
using Serilog.Events;

namespace NoMercy.Data.Jobs;

[Serializable]
public class TmdbShowJob : IShouldQueue, IDisposable, IAsyncDisposable
{
    public int Id { get; set; }
    public Library? Library { get; set; }

    public TmdbShowJob()
    {
        //
    }

    public TmdbShowJob(int id)
    {
        Id = id;
    }

    public TmdbShowJob(int id, Library? library = null)
    {
        Id = id;
        Library = library;
    }

    public async Task Handle()
    {
        await using MediaContext context = new();

        Library ??= await context.Libraries
            .Include(x => x.LibraryTvs)
            .Where(x => x.LibraryTvs.Any(y => y.TvId == Id))
            .FirstOrDefaultAsync();

        if (Library is null)
        {
            Logger.MovieDb($"TvShow {Id}: Library not found", LogEventLevel.Error);
            return;
        }

        await using TvShowLogic tvShow = new(Id, Library);
        await tvShow.Process();

        if (tvShow.Show != null)
        {
            Logger.MovieDb($"TvShow {tvShow.Show.Name}: Processed");

            Networking.Networking.SendToAll("RefreshLibrary", "socket", new RefreshLibraryDto
            {
                QueryKey = ["tv", tvShow.Show.Id.ToString()]
            });
        }
    }

    public void Dispose()
    {
        GC.Collect();
        GC.WaitForFullGCComplete();
        GC.WaitForPendingFinalizers();
    }

    public ValueTask DisposeAsync()
    {
        GC.Collect();
        GC.WaitForFullGCComplete();
        GC.WaitForPendingFinalizers();

        return ValueTask.CompletedTask;
    }
}