using NoMercy.Database.Models;
using NoMercy.Providers.TMDB.Client;
using NoMercy.Queue;

namespace NoMercy.MediaProcessing.Shows;

public class ShowManager(
    IShowRepository movieRepository,
    TmdbTvClient tmdbShowClientService,
    JobDispatcher jobDispatcher)
    : IShowManager
{
    private readonly IShowRepository _movieRepository = movieRepository;
    private readonly TmdbTvClient _tmdbShowClient = tmdbShowClientService;
    private readonly JobDispatcher _jobDispatcher = jobDispatcher;

    public Task AddShowAsync(int id, Library library)
    {
        throw new NotImplementedException();
    }

    public Task UpdateShowAsync(int id, Library library)
    {
        throw new NotImplementedException();
    }

    public Task RemoveShowAsync(int id)
    {
        throw new NotImplementedException();
    }
}