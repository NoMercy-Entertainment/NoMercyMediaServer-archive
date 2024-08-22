using NoMercy.Providers.TMDB.Models.Season;
using NoMercy.Providers.TMDB.Models.TV;

namespace NoMercy.MediaProcessing.Episodes;

public interface IEpisodeManager
{
    public Task StoreEpisodes(TmdbTvShow show, TmdbSeasonAppends season);
}