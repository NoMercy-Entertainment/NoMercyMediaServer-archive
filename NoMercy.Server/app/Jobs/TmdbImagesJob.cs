using NoMercy.Providers.TMDB.Models.Collections;
using NoMercy.Providers.TMDB.Models.Episode;
using NoMercy.Providers.TMDB.Models.Movies;
using NoMercy.Providers.TMDB.Models.People;
using NoMercy.Providers.TMDB.Models.Season;
using NoMercy.Providers.TMDB.Models.TV;
using NoMercy.Server.Logic;
using NoMercy.Server.system;

namespace NoMercy.Server.app.Jobs;

[Serializable]
public class TmdbImagesJob : IShouldQueue
{
    public TmdbMovieAppends? MovieAppends { get; set; }
    public TmdbCollectionAppends? CollectionAppends { get; set; }
    
    public TmdbTvShowAppends? TvShowAppends { get; set; }
    
    public int ShowId { get; set; }
    public TmdbSeasonAppends? SeasonAppends { get; set; }
    
    public int SeasonNumber { get; set; }
    public TmdbEpisodeAppends? EpisodeAppends { get; set; }
    
    public TmdbPersonAppends? PersonAppends { get; set; }
    

    public TmdbImagesJob()
    {
        //
    }

    public TmdbImagesJob(TmdbMovieAppends movieAppends)
    {
        MovieAppends = movieAppends;
    }
    
    public TmdbImagesJob(TmdbCollectionAppends collectionAppends)
    {
        CollectionAppends = collectionAppends;
    }

    public TmdbImagesJob(TmdbTvShowAppends tvShowAppends)
    {
        TvShowAppends = tvShowAppends;
    }

    public TmdbImagesJob(int showId, TmdbSeasonAppends seasonNumber)
    {
        ShowId = showId;
        SeasonAppends = seasonNumber;
    }
    
    public TmdbImagesJob(int showId, int seasonNumber, TmdbEpisodeAppends episodeAppends)
    {
        ShowId = showId;
        SeasonNumber = seasonNumber;
        EpisodeAppends = episodeAppends;
    }
    
    public TmdbImagesJob(TmdbPersonAppends personAppends)
    {
        PersonAppends = personAppends;
    }
    
    public async Task Handle()
    {
        if (CollectionAppends is not null)
        {
            await CollectionLogic.StoreImages(CollectionAppends);
            return;
        }

        if (MovieAppends is not null)
        {
            await MovieLogic.StoreImages(MovieAppends);
            return;
        }

        if (TvShowAppends is not null)
        {
            await TvShowLogic.StoreImages(TvShowAppends);
            return;
        }

        if (SeasonAppends is not null)
        {
            await SeasonLogic.StoreImages(ShowId, SeasonAppends);
            return;
        }

        if (EpisodeAppends is not null)
        {
            await EpisodeLogic.StoreImages(ShowId, SeasonNumber, EpisodeAppends);
            return;
        }

        if (PersonAppends is not null)
        {
            await PersonLogic.StoreImages(PersonAppends);
            return;
        }
    }
}