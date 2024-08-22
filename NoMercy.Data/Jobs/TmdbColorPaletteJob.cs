using System.Globalization;
using NoMercy.Data.Logic;
using NoMercy.Data.Logic.ImageLogic;
using NoMercy.NmSystem;
using NoMercy.Providers.TMDB.Models.Collections;
using NoMercy.Providers.TMDB.Models.Episode;
using NoMercy.Providers.TMDB.Models.Movies;
using NoMercy.Providers.TMDB.Models.People;
using NoMercy.Providers.TMDB.Models.Season;
using NoMercy.Providers.TMDB.Models.TV;
using NoMercy.Queue;
using Serilog.Events;

namespace NoMercy.Data.Jobs;

[Serializable]
public class TmdbColorPaletteJob : IShouldQueue, IDisposable, IAsyncDisposable
{
    public int? Id { get; set; }
    public string? FilePath { get; set; }
    public string? Type { get; set; }
    public string? Model { get; set; }
    public string? Language { get; set; }
    public TmdbMovieAppends? MovieAppends { get; set; }
    public TmdbTvShowAppends? TvShowAppends { get; set; }

    public int? ShowId { get; set; }
    public TmdbSeasonAppends? SeasonAppends { get; set; }

    public int? SeasonId { get; set; }
    public TmdbEpisodeAppends? EpisodeAppends { get; set; }

    public TmdbPersonAppends? PersonAppends { get; set; }
    public TmdbCollectionAppends? CollectionAppends { get; set; }

    public TmdbColorPaletteJob()
    {
    }

    public TmdbColorPaletteJob(int id, string model)
    {
        Id = id;
        Model = model;
    }

    public TmdbColorPaletteJob(TmdbMovieAppends movieAppends)
    {
        MovieAppends = movieAppends;
    }

    public TmdbColorPaletteJob(TmdbCollectionAppends collectionAppends)
    {
        CollectionAppends = collectionAppends;
    }

    public TmdbColorPaletteJob(TmdbTvShowAppends tvShowAppends)
    {
        TvShowAppends = tvShowAppends;
    }

    public TmdbColorPaletteJob(int showId, TmdbSeasonAppends seasonAppends)
    {
        ShowId = showId;
        SeasonAppends = seasonAppends;
    }

    public TmdbColorPaletteJob(int showId, int seasonId, TmdbEpisodeAppends episodeAppends)
    {
        ShowId = showId;
        SeasonId = seasonId;
        EpisodeAppends = episodeAppends;
    }

    public TmdbColorPaletteJob(TmdbPersonAppends personAppends)
    {
        PersonAppends = personAppends;
    }

    public TmdbColorPaletteJob(int id, string type, string model)
    {
        Id = id;
        Model = model;
        Type = type;
    }

    public TmdbColorPaletteJob(string? filePath, string model, string? language = null)
    {
        FilePath = filePath;
        Model = model;
        Language = language;
    }

    public async Task Handle()
    {
        if (MovieAppends is not null)
        {
            await MovieLogic.Palette(MovieAppends.Id);
            return;
        }

        if (CollectionAppends is not null)
        {
            await CollectionLogic.Palette(CollectionAppends.Id);
            return;
        }

        if (TvShowAppends is not null)
        {
            await TvShowLogic.Palette(TvShowAppends.Id);
            return;
        }

        if (SeasonAppends is not null && ShowId.HasValue)
        {
            await SeasonLogic.Palette(SeasonAppends.Id);
            return;
        }

        if (EpisodeAppends is not null && ShowId.HasValue && SeasonId.HasValue)
        {
            await EpisodeLogic.Palette(EpisodeAppends.Id);
            return;
        }

        if (PersonAppends is not null)
        {
            await PersonLogic.Palette(PersonAppends.Id);
            return;
        }

        switch (Model)
        {
            case "image" when FilePath == null:
                await Task.CompletedTask;
                return;
            case "image":
                bool shouldDownload = Language is null || Language is "en" || Language is "" ||
                                      Language == CultureInfo.CurrentCulture
                                          .TwoLetterISOLanguageName;

                await MovieDbImage.ColorPalette("image", FilePath, shouldDownload);
                break;
        }

        if (!Id.HasValue) return;

        switch (Model)
        {
            case "person":
                await PersonLogic.Palette(Id.Value);
                break;
            case "recommendation":
                switch (Type)
                {
                    case "movie":
                        await MovieLogic.RecommendationPalette(Id.Value);
                        break;
                    case "tv":
                        await TvShowLogic.RecommendationPalette(Id.Value);
                        break;
                    default:
                        Logger.Queue(@"Invalid model Type: " + Model + @" id: " + Id + @" type: " + Type,
                            LogEventLevel.Error);
                        break;
                }

                break;
            case "similar":
                switch (Type)
                {
                    case "movie":
                        await MovieLogic.SimilarPalette(Id.Value);
                        break;
                    case "tv":
                        await TvShowLogic.SimilarPalette(Id.Value);
                        break;
                    default:
                        Logger.Queue(@"Invalid model Type: " + Model + @" id: " + Id + @" type: " + Type,
                            LogEventLevel.Error);
                        break;
                }

                break;
            default:
                Logger.Queue("Invalid model Type: " + Model + " id: " + Id + " type: " + Type, LogEventLevel.Error);
                break;
        }
    }

    private static void ReleaseUnmanagedResources()
    {
        GC.Collect();
        GC.WaitForFullGCComplete();
        GC.WaitForPendingFinalizers();
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
    }

    public async ValueTask DisposeAsync()
    {
        ReleaseUnmanagedResources();
    }
}