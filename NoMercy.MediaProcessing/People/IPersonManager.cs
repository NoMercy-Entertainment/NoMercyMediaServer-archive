using System.Collections.Concurrent;
using NoMercy.Providers.TMDB.Models.People;
using NoMercy.Providers.TMDB.Models.TV;

namespace NoMercy.MediaProcessing.People;

public interface IPersonManager
{
    public Task<ConcurrentStack<TmdbPersonAppends>> StorePeoplesAsync(TmdbTvShowAppends show);
    public Task UpdatePeopleAsync(string showName, TmdbPersonAppends season);
    public Task RemovePeopleAsync(string showName, TmdbPersonAppends season);
}