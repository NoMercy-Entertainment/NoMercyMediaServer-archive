using System.Collections.Concurrent;
using NoMercy.Providers.TMDB.Models.People;
using NoMercy.Providers.TMDB.Models.TV;

namespace NoMercy.MediaProcessing.People;

public class PersonManager: IPersonManager
{
    public Task<ConcurrentStack<TmdbPersonAppends>> StorePEoplesAsync(TmdbTvShowAppends show)
    {
        throw new NotImplementedException();
    }

    public Task UpdatePEopleAsync(string showName, TmdbPersonAppends season)
    {
        throw new NotImplementedException();
    }

    public Task RemovePEopleAsync(string showName, TmdbPersonAppends season)
    {
        throw new NotImplementedException();
    }
}