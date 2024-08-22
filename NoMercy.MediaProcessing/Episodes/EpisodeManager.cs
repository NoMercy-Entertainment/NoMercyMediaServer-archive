using System.Collections.Concurrent;
using NoMercy.Database.Models;
using NoMercy.MediaProcessing.Common;
using NoMercy.MediaProcessing.Images;
using NoMercy.MediaProcessing.Jobs;
using NoMercy.MediaProcessing.Jobs.PaletteJobs;
using NoMercy.NmSystem;
using NoMercy.Providers.TMDB.Client;
using NoMercy.Providers.TMDB.Models.Episode;
using NoMercy.Providers.TMDB.Models.Season;
using NoMercy.Providers.TMDB.Models.TV;
using Serilog.Events;

namespace NoMercy.MediaProcessing.Episodes;

public class EpisodeManager(
    IEpisodeRepository episodeRepository,
    JobDispatcher jobDispatcher
) : BaseManager, IEpisodeManager
{
    
    public async Task StoreEpisodes(TmdbTvShow show, TmdbSeasonAppends season)
    {
        (ConcurrentStack<TmdbEpisodeAppends> episodeAppends, IEnumerable<Episode> episodes) = await CollectEpisodes(show, season);

        await episodeRepository.StoreEpisodes(episodes);
        
        Logger.MovieDb($"Show {show.Name}, Season {season.SeasonNumber} Episodes stored");

         List<Task> promises = [];
         foreach (TmdbEpisodeAppends episode in episodeAppends)
        {
            promises.Add(StoreImages(show.Name, episode));
            promises.Add(StoreTranslations(show.Name, episode));
        }
        
        await Task.WhenAll(promises);
    }

    private static async Task<(ConcurrentStack<TmdbEpisodeAppends> episodeAppends, IEnumerable<Episode> episodes)> CollectEpisodes(TmdbTvShow show, TmdbSeasonAppends season)
    {
        ConcurrentStack<TmdbEpisodeAppends> episodeAppends = [];
        
        await Parallel.ForEachAsync(season.Episodes, async (episode, _) =>
        {
            try
            {
                using TmdbEpisodeClient tmdbEpisodeClient = new(show.Id, episode.SeasonNumber, episode.EpisodeNumber);
                TmdbEpisodeAppends? seasonTask = await tmdbEpisodeClient.WithAllAppends();
                if (seasonTask is null) return;
                
                episodeAppends.Push(seasonTask);
            }
            catch (Exception e)
            {
                Logger.MovieDb(e, LogEventLevel.Error);
            }
        });

        IEnumerable<Episode> episodes = episodeAppends
            .Select(episode => new Episode
            {
                TvId = show.Id,
                SeasonId = season.Id,
                    
                Id = episode.Id,
                Title = episode.Name,
                AirDate = episode.AirDate,
                EpisodeNumber = episode.EpisodeNumber,
                ImdbId = episode.TmdbEpisodeExternalIds.ImdbId,
                Overview = episode.Overview,
                ProductionCode = episode.ProductionCode,
                SeasonNumber = episode.SeasonNumber,
                Still = episode.StillPath,
                TvdbId = episode.TmdbEpisodeExternalIds.TvdbId,
                VoteAverage = episode.VoteAverage,
                VoteCount = episode.VoteCount,
                _colorPalette = MovieDbImage.ColorPalette("still", episode.StillPath).Result
            })
            .OrderBy(keySelector: f => f.EpisodeNumber);
        
        return (episodeAppends, episodes);
    }

    internal async Task StoreTranslations(string showName, TmdbEpisodeAppends episode)
    {
        IEnumerable<Translation> translations = episode.Translations.Translations
            .Where(translation => translation.Data.Title != null || translation.Data.Overview != "")
            .Select(translation => new Translation
            {
                Iso31661 = translation.Iso31661,
                Iso6391 = translation.Iso6391,
                Name = translation.Name == "" ? null : translation.Name,
                Title = translation.Data.Title == "" ? null : translation.Data.Title,
                Overview = translation.Data.Overview == "" ? null : translation.Data.Overview,
                EnglishName = translation.EnglishName,
                Homepage = translation.Data.Homepage?.ToString(),
                EpisodeId = episode.Id
            });
        
        await episodeRepository.StoreEpisodeTranslations(translations);

        Logger.MovieDb($"Show {showName}, Season {episode.SeasonNumber} Episode {episode.EpisodeNumber}: Translations stored");
    }

    internal async Task StoreImages(string showName, TmdbEpisodeAppends episode)
    {
        IEnumerable<Image> stills = episode.TmdbEpisodeImages.Stills
            .Select(image => new Image
            {
                AspectRatio = image.AspectRatio,
                FilePath = image.FilePath,
                Height = image.Height,
                Iso6391 = image.Iso6391,
                VoteAverage = image.VoteAverage,
                VoteCount = image.VoteCount,
                Width = image.Width,
                EpisodeId = episode.Id,
                Type = "still",
                Site = "https://image.tmdb.org/t/p/"
            })
            .ToList();

         await episodeRepository.StoreEpisodeImages(stills);
         
         IEnumerable<Image> posterJobItems = stills
             .Select(x => new Image { FilePath = x.FilePath });
         jobDispatcher.DispatchJob<ImagePaletteJob, Image>(episode.Id, posterJobItems);
         
         Logger.MovieDb($"Show {showName}, Season {episode.SeasonNumber} Episode {episode.EpisodeNumber}: Images stored");
    }
}
