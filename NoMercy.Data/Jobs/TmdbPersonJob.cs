using NoMercy.Data.Logic;
using NoMercy.Networking;
using NoMercy.Providers.TMDB.Client;
using NoMercy.Providers.TMDB.Models.Episode;
using NoMercy.Providers.TMDB.Models.Movies;
using NoMercy.Providers.TMDB.Models.Season;
using NoMercy.Providers.TMDB.Models.TV;
using NoMercy.Queue;

namespace NoMercy.Data.Jobs;

[Serializable]
public class TmdbPersonJob : IShouldQueue
{
    public TmdbTvShowAppends? TvShowAppends { get; set; }
    public TmdbMovieAppends? MovieAppends { get; set; }

    public TmdbPersonJob()
    {
        //
    }

    public TmdbPersonJob(TmdbTvShowAppends tvShowAppends)
    {
        TvShowAppends = tvShowAppends;
    }

    public TmdbPersonJob(TmdbMovieAppends movieAppends)
    {
        MovieAppends = movieAppends;
    }

    public async Task Handle()
    {
        if (TvShowAppends is not null)
        {
            await using PersonLogic personShowLogic = new(TvShowAppends);
            await personShowLogic.FetchPeople();

            foreach (TmdbSeason s in TvShowAppends.Seasons)
            {
                using TmdbSeasonClient tmdbSeasonClient = new(TvShowAppends.Id, s.SeasonNumber);
                TmdbSeasonAppends? season = await tmdbSeasonClient.WithAllAppends();
                if (season is null) continue;

                await using PersonLogic personSeasonLogic = new(TvShowAppends, season);
                await personSeasonLogic.FetchPeople();

                foreach (TmdbEpisodeDetails e in season.Episodes)
                {
                    using TmdbEpisodeClient tmdbEpisodeClient =
                        new(TvShowAppends.Id, season.SeasonNumber, e.EpisodeNumber);
                    TmdbEpisodeAppends? episode = await tmdbEpisodeClient.WithAllAppends();
                    if (episode is null) continue;

                    await using PersonLogic personEpisodeLogic = new(TvShowAppends, season, episode);
                    await personEpisodeLogic.FetchPeople();
                }
            }

            Networking.Networking.SendToAll("RefreshLibrary", "socket", new RefreshLibraryDto
            {
                QueryKey = ["tv", TvShowAppends.Id]
            });
        }

        if (MovieAppends is not null)
        {
            await using PersonLogic personMovieLogic = new(MovieAppends);
            await personMovieLogic.FetchPeople();

            Networking.Networking.SendToAll("RefreshLibrary", "socket", new RefreshLibraryDto
            {
                QueryKey = ["movie", MovieAppends.Id]
            });
        }
    }
}