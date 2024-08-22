using System.Collections.Concurrent;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using NoMercy.Data.Jobs;
using NoMercy.Data.Logic.ImageLogic;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.NmSystem;
using NoMercy.Providers.TMDB.Client;
using NoMercy.Providers.TMDB.Models.Season;
using NoMercy.Providers.TMDB.Models.TV;
using NoMercy.Queue;
using Serilog.Events;
using Season = NoMercy.Database.Models.Season;

namespace NoMercy.Data.Logic;

public class SeasonLogic(TmdbTvShowDetails show) : IDisposable, IAsyncDisposable
{
    private readonly ConcurrentStack<TmdbSeasonAppends> _seasonAppends = [];

    public async Task FetchSeasons()
    {
        foreach (TmdbSeason season in show.Seasons)
            try
            {
                using TmdbSeasonClient tmdbSeasonClient = new(show.Id, season.SeasonNumber);
                TmdbSeasonAppends? seasonTask = await tmdbSeasonClient.WithAllAppends();
                if (seasonTask is null) continue;
                _seasonAppends.Push(seasonTask);
            }
            catch (Exception e)
            {
                Logger.MovieDb(e, LogEventLevel.Error);
            }

        await Store();

        await StoreTranslations();

        await StoreEpisodes();

        await DispatchJobs();
    }

    private Task DispatchJobs()
    {
        return Task.CompletedTask;
    }

    private Task Store()
    {
        try
        {
            Season[] seasons = _seasonAppends.ToList()
                .ConvertAll<Season>(s => new Season
                {
                    Id = s.Id,
                    Title = s.Name,
                    AirDate = s.AirDate,
                    EpisodeCount = s.Episodes.Length,
                    Overview = s.Overview,
                    Poster = s.PosterPath,
                    SeasonNumber = s.SeasonNumber,
                    TvId = show.Id
                    // _colorPalette = MovieDbImage.ColorPalette("poster", x.PosterPath).Result
                })
                .ToArray();

            using MediaContext mediaContext = new();
            mediaContext.Seasons.UpsertRange(seasons)
                .On(s => new { s.Id })
                .WhenMatched((ss, si) => new Season
                {
                    Id = si.Id,
                    Title = si.Title,
                    AirDate = si.AirDate,
                    EpisodeCount = si.EpisodeCount,
                    Overview = si.Overview,
                    Poster = si.Poster,
                    SeasonNumber = si.SeasonNumber,
                    TvId = si.TvId
                    // _colorPalette = si._colorPalette
                })
                .Run();
        }
        catch (Exception e)
        {
            Logger.MovieDb(e, LogEventLevel.Error);
        }

        Logger.MovieDb($"TvShow {show.Name}: Seasons stored");

        return Task.CompletedTask;
    }

    private Task StoreEpisodes()
    {
        foreach (TmdbSeasonAppends season in _seasonAppends)
        {
            using EpisodeLogic episodeLogic = new(show, season);
            episodeLogic.FetchEpisodes().Wait();
        }

        return Task.CompletedTask;
    }

    public static async Task Palette(int id)
    {
        Logger.Queue($"Fetching color palette for Season {id}");

        await using MediaContext mediaContext = new();

        Season? season = await mediaContext.Seasons
            .Where(e => e._colorPalette == "")
            .FirstOrDefaultAsync(e => e.Id == id);

        if (season is { _colorPalette: "" })
        {
            season._colorPalette = await MovieDbImage.ColorPalette("poster", season.Poster);

            await mediaContext.SaveChangesAsync();
        }

        Image[] images = await mediaContext.Images
            .Where(e => e.SeasonId == id)
            .Where(e => e._colorPalette == "")
            .Where(e => e.Iso6391 == null || e.Iso6391 == "en" || e.Iso6391 == "" ||
                        e.Iso6391 == CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
            .ToArrayAsync();

        foreach (Image image in images)
        {
            if (string.IsNullOrEmpty(image.FilePath)) continue;

            Logger.Queue($"Fetching color palette for Season Image {image.FilePath}");

            TmdbColorPaletteJob tmdbColorPaletteJob = new(image.FilePath, "image", image.Iso6391);
            JobDispatcher.Dispatch(tmdbColorPaletteJob, "image", 2);
        }
    }

    public static async Task StoreImages(int id, TmdbSeasonAppends seasonAppends)
    {
        await using MediaContext mediaContext = new();

        Tv? tvShow = await mediaContext.Tvs
            .FirstOrDefaultAsync(tv => tv.Id == id);

        TmdbSeasonClient tmdbSeasonClient = new(id, seasonAppends.Id);
        TmdbSeasonAppends? season = await tmdbSeasonClient.WithAllAppends();
        if (season is null) return;

        List<Image> images = season.TmdbSeasonImages.Posters.ToList()
            .ConvertAll<Image>(image => new Image
            {
                AspectRatio = image.AspectRatio,
                Height = image.Height,
                Iso6391 = image.Iso6391,
                FilePath = image.FilePath,
                Width = image.Width,
                VoteAverage = image.VoteAverage,
                VoteCount = image.VoteCount,
                SeasonId = seasonAppends.Id,
                Type = "poster",
                Site = "https://image.tmdb.org/t/p/"
            });

        await mediaContext.Images.UpsertRange(images)
            .On(v => new { v.FilePath, v.SeasonId })
            .WhenMatched((ts, ti) => new Image
            {
                AspectRatio = ti.AspectRatio,
                FilePath = ti.FilePath,
                Height = ti.Height,
                Iso6391 = ti.Iso6391,
                Site = ti.Site,
                VoteAverage = ti.VoteAverage,
                VoteCount = ti.VoteCount,
                Width = ti.Width,
                Type = ti.Type,
                SeasonId = ti.SeasonId
            })
            .RunAsync();

        TmdbColorPaletteJob tmdbColorPaletteJob = new(season.Id, "season");
        JobDispatcher.Dispatch(tmdbColorPaletteJob, "image", 2);

        Logger.MovieDb($"TvShow {tvShow?.Title}, Season {season.SeasonNumber}: Images stored");
    }

    private Task StoreTranslations()
    {
        using MediaContext mediaContext = new();
        foreach (TmdbSeasonAppends season in _seasonAppends)
        {
            Translation[] translations = season.Translations.Translations.ToList()
                .ConvertAll<Translation>(translation => new Translation
                {
                    Iso31661 = translation.Iso31661,
                    Iso6391 = translation.Iso6391,
                    Name = translation.Name == "" ? null : translation.Name,
                    Title = translation.Data.Title == "" ? null : translation.Data.Title,
                    Overview = translation.Data.Overview == "" ? null : translation.Data.Overview,
                    EnglishName = translation.EnglishName,
                    Homepage = translation.Data.Homepage?.ToString(),
                    SeasonId = season.Id
                }).ToArray();

            mediaContext.Translations
                .UpsertRange(translations.Where(translation =>
                    translation.Title != null || translation.Overview != ""))
                .On(t => new { t.Iso31661, t.Iso6391, t.SeasonId })
                .WhenMatched((ts, ti) => new Translation
                {
                    Iso31661 = ti.Iso31661,
                    Iso6391 = ti.Iso6391,
                    Name = ti.Name,
                    EnglishName = ti.EnglishName,
                    Title = ti.Title,
                    Overview = ti.Overview,
                    Homepage = ti.Homepage,
                    Biography = ti.Biography,
                    TvId = ti.TvId,
                    SeasonId = ti.SeasonId,
                    EpisodeId = ti.EpisodeId,
                    MovieId = ti.MovieId,
                    CollectionId = ti.CollectionId,
                    PersonId = ti.PersonId
                })
                .Run();

            Logger.MovieDb($"TvShow {show?.Name}, Season {season.SeasonNumber}: Translations stored");
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _seasonAppends.Clear();
        GC.Collect();
        GC.WaitForFullGCComplete();
        GC.WaitForPendingFinalizers();
    }

    public ValueTask DisposeAsync()
    {
        _seasonAppends.Clear();
        GC.Collect();
        GC.WaitForFullGCComplete();
        GC.WaitForPendingFinalizers();
        return ValueTask.CompletedTask;
    }
}