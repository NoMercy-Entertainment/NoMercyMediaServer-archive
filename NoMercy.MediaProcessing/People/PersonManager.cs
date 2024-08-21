using System.Collections.Concurrent;
using NoMercy.Providers.TMDB.Models.People;
using NoMercy.Providers.TMDB.Models.TV;

namespace NoMercy.MediaProcessing.People;

public class PersonManager: IPersonManager
{
    public Task<ConcurrentStack<TmdbPersonAppends>> StorePeoplesAsync(TmdbTvShowAppends show)
    {
        throw new NotImplementedException();
    }

    public Task UpdatePeopleAsync(string showName, TmdbPersonAppends season)
    {
        throw new NotImplementedException();
    }

    public Task RemovePeopleAsync(string showName, TmdbPersonAppends season)
    {
        throw new NotImplementedException();
    }
}