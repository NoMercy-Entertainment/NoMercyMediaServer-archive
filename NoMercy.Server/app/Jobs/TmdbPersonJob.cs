using NoMercy.Helpers;
using NoMercy.Networking;
using NoMercy.Providers.TMDB.Client;
using NoMercy.Providers.TMDB.Models.Episode;
using NoMercy.Providers.TMDB.Models.Movies;
using NoMercy.Providers.TMDB.Models.Season;
using NoMercy.Providers.TMDB.Models.TV;
using NoMercy.Server.app.Helper;
using NoMercy.Server.Logic;
using NoMercy.Server.system;

namespace NoMercy.Server.app.Jobs;

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
        if(TvShowAppends is not null) 
        {
            await using var personShowLogic = new PersonLogic(TvShowAppends);
            await personShowLogic.FetchPeople();

            foreach (var s in TvShowAppends.Seasons)
            {
                using TmdbSeasonClient tmdbSeasonClient = new(TvShowAppends.Id, s.SeasonNumber);
                var season = await tmdbSeasonClient.WithAllAppends();
                if (season is null) continue;

                await using var personSeasonLogic = new PersonLogic(TvShowAppends, season);
                await personSeasonLogic.FetchPeople();

                foreach (var e in season.Episodes)
                {
                    using TmdbEpisodeClient tmdbEpisodeClient = new(TvShowAppends.Id, season.SeasonNumber, e.EpisodeNumber);
                    var episode = await tmdbEpisodeClient.WithAllAppends();
                    if (episode is null) continue;

                    await using var personEpisodeLogic = new PersonLogic(TvShowAppends, season, episode);
                    await personEpisodeLogic.FetchPeople();
                }
            }

            Networking.Networking.SendToAll("RefreshLibrary", "socket", new RefreshLibraryDto
            {
                QueryKey = ["tv", TvShowAppends.Id]
            });
        }
        
        if(MovieAppends is not null) 
        {
            await using var personMovieLogic = new PersonLogic(MovieAppends);
            await personMovieLogic.FetchPeople();

            Networking.Networking.SendToAll("RefreshLibrary", "socket", new RefreshLibraryDto
            {
                QueryKey = ["movie", MovieAppends.Id]
            });
            
        }
    }
}