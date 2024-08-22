using NoMercy.Providers.TMDB.Models.TV;

namespace NoMercy.MediaProcessing.People;

public interface IPersonManager
{
    public Task StorePeoplesAsync(TmdbTvShowAppends show);
    public Task UpdatePeopleAsync(string showName, TmdbTvShowAppends show);
    public Task RemovePeopleAsync(string showName, TmdbTvShowAppends show);
}