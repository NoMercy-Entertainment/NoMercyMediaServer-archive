using System.Globalization;
using Microsoft.EntityFrameworkCore;
using NoMercy.Data.Jobs;
using NoMercy.Data.Logic.ImageLogic;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.Helpers.system;
using NoMercy.NmSystem;
using NoMercy.Providers.Other;
using NoMercy.Providers.TMDB.Client;
using NoMercy.Providers.TMDB.Models.TV;
using Serilog.Events;

namespace NoMercy.Data.Logic;

public class TvShowLogic(int id, Library library) : IDisposable, IAsyncDisposable
{
    private TmdbTvClient TmdbTvClient { get; } = new(id);
    private string MediaType { get; set; } = "";
    private string Folder { get; set; } = "";

    public TmdbTvShowAppends? Show { get; set; }

    private readonly MediaContext _mediaContext = new();

    public async Task Process()
    {
        Show = await TmdbTvClient.WithAllAppends();
        if (Show is null)
        {
            Logger.MovieDb($"TvShow {id}: not found");
            return;
        }

        Logger.MovieDb($"TvShow {Show.Name}: found");

        Folder = FindFolder();
        MediaType = GetMediaType();

        Logger.MovieDb($"TvShow {Show.Name}: Folder: {Folder} MediaType: {MediaType}");

        await Store();

        await StoreAlternativeTitles();
        await StoreTranslations();
        await StoreGenres();
        await StoreKeywords();
        await StoreCompanies();
        await StoreNetworks();
        await StoreVideos();
        await StoreRecommendations();
        await StoreSimilar();
        await StoreContentRatings();
        await StoreWatchProviders();

        await using SeasonLogic seasonLogic = new(Show);
        await seasonLogic.FetchSeasons();

        await DispatchJobs();
    }

    private async Task StoreAlternativeTitles()
    {
        AlternativeTitle[] alternativeTitles = Show?.AlternativeTitles.Results.ToList()
            .ConvertAll<AlternativeTitle>(alternativeTitle => new AlternativeTitle
            {
                Iso31661 = alternativeTitle.Iso31661,
                Title = alternativeTitle.Title,
                TvId = Show.Id
            }).ToArray() ?? [];

        await _mediaContext.AlternativeTitles.UpsertRange(alternativeTitles)
            .On(a => new { a.Title, a.TvId })
            .WhenMatched((ats, ati) => new AlternativeTitle
            {
                Title = ati.Title,
                Iso31661 = ati.Iso31661,
                TvId = ati.TvId
            })
            .RunAsync();

        Logger.MovieDb($"TvShow {Show?.Name}: AlternativeTitles stored");
    }

    private async Task StoreAggregateCredits()
    {
        Logger.MovieDb($"TvShow {Show?.Name}: AggregateCredits stored");
        await Task.CompletedTask;
    }

    private async Task StoreWatchProviders()
    {
        Logger.MovieDb($"TvShow {Show?.Name}: WatchProviders stored");
        await Task.CompletedTask;
    }

    private async Task StoreTranslations()
    {
        try
        {
            Translation[] translations = Show?.Translations.Translations.ToList()
                .ConvertAll<Translation>(translation => new Translation
                {
                    Iso31661 = translation.Iso31661,
                    Iso6391 = translation.Iso6391,
                    Name = translation.Name == "" ? null : translation.Name,
                    Title = translation.Data.Title == "" ? null : translation.Data.Title,
                    Overview = translation.Data.Overview == "" ? null : translation.Data.Overview,
                    EnglishName = translation.EnglishName,
                    Homepage = translation.Data.Homepage?.ToString(),
                    Biography = translation.Data.Biography,
                    TvId = Show.Id
                }).ToArray() ?? [];

            await _mediaContext.Translations
                .UpsertRange(translations.Where(translation => translation.Title != null || translation.Overview != ""))
                .On(t => new { t.Iso31661, t.Iso6391, t.TvId })
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
                .RunAsync();
        }
        catch (Exception e)
        {
            Logger.MovieDb(e, LogEventLevel.Error);
        }
    }

    private async Task StoreContentRatings()
    {
        List<CertificationTv> certifications = [];

        foreach (TmdbTvContentRating tvContentRating in Show?.ContentRatings.Results ?? [])
        {
            Certification? certification = _mediaContext.Certifications
                .FirstOrDefault(c => c.Iso31661 == tvContentRating.Iso31661 && c.Rating == tvContentRating.Rating);
            if (certification is null) continue;

            certifications.Add(new CertificationTv
            {
                CertificationId = certification.Id,
                TvId = Show?.Id ?? 0
            });
        }

        await _mediaContext.CertificationTv.UpsertRange(certifications)
            .On(v => new { v.CertificationId, v.TvId })
            .WhenMatched((ts, ti) => new CertificationTv
            {
                CertificationId = ti.CertificationId,
                TvId = ti.TvId
            })
            .RunAsync();

        Logger.MovieDb($"TvShow {Show?.Name}: ContentRatings stored");
        await Task.CompletedTask;
    }

    private async Task StoreSimilar()
    {
        List<Similar> data = Show?.Similar.Results.ToList()
            .ConvertAll<Similar>(similar => new Similar
            {
                Backdrop = similar.BackdropPath,
                Overview = similar.Overview,
                Poster = similar.PosterPath,
                Title = similar.Name,
                TitleSort = similar.Name,
                MediaId = similar.Id,
                TvFromId = Show.Id
            }) ?? [];

        await _mediaContext.Similar.UpsertRange(data)
            .On(v => new { v.MediaId, v.TvFromId })
            .WhenMatched((ts, ti) => new Similar
            {
                TvToId = ti.TvToId,
                TvFromId = ti.TvFromId,
                Overview = ti.Overview,
                Title = ti.Title,
                TitleSort = ti.TitleSort,
                Backdrop = ti.Backdrop,
                Poster = ti.Poster,
                MediaId = ti.MediaId
            })
            .RunAsync();

        TmdbColorPaletteJob similarColorPaletteJob = new(id, model: "similar", type: "tv");
        JobDispatcher.Dispatch(similarColorPaletteJob, "image", 2);

        Logger.MovieDb($"TvShow {Show?.Name}: Similar stored");
        await Task.CompletedTask;
    }

    private async Task StoreRecommendations()
    {
        List<Recommendation> data = Show?.Recommendations.Results.ToList()
            .ConvertAll<Recommendation>(recommendation => new Recommendation
            {
                Backdrop = recommendation.BackdropPath,
                Overview = recommendation.Overview,
                Poster = recommendation.PosterPath,
                Title = recommendation.Name,
                TitleSort = recommendation.Name.TitleSort(),
                MediaId = recommendation.Id,
                TvFromId = Show.Id
            }) ?? [];

        await _mediaContext.Recommendations.UpsertRange(data)
            .On(v => new { v.MediaId, v.TvFromId })
            .WhenMatched((ts, ti) => new Recommendation
            {
                TvToId = ti.TvToId,
                TvFromId = ti.TvFromId,
                Overview = ti.Overview,
                Title = ti.Title,
                TitleSort = ti.TitleSort,
                Backdrop = ti.Backdrop,
                Poster = ti.Poster,
                MediaId = ti.MediaId
            })
            .RunAsync();

        TmdbColorPaletteJob recommendationColorPaletteJob = new(id, model: "recommendation", type: "tv");
        JobDispatcher.Dispatch(recommendationColorPaletteJob, "image", 2);

        Logger.MovieDb($"TvShow {Show?.Name}: Recommendations stored");
        await Task.CompletedTask;
    }

    private async Task StoreVideos()
    {
        try
        {
            List<Media> videos = Show?.Videos.Results.ToList()
                .ConvertAll<Media>(media => new Media
                {
                    _type = "video",
                    Id = Ulid.NewUlid(),
                    Iso6391 = media.Iso6391,
                    Name = media.Name,
                    Site = media.Site,
                    Size = media.Size,
                    Src = media.Key,
                    Type = media.Type,
                    TvId = Show.Id
                }) ?? [];

            await _mediaContext.Medias.UpsertRange(videos)
                .On(v => new { v.Src, v.TvId })
                .WhenMatched((ts, ti) => new Media
                {
                    Id = ti.Id,
                    Src = ti.Src,
                    Iso6391 = ti.Iso6391,
                    Type = ti.Type,
                    TvId = ti.TvId,
                    Name = ti.Name,
                    Site = ti.Site,
                    Size = ti.Size
                })
                .RunAsync();
        }
        catch (Exception e)
        {
            Logger.MovieDb(e, LogEventLevel.Error);
        }

        Logger.MovieDb($"TvShow {Show?.Name}: Videos stored");
        await Task.CompletedTask;
    }

    public static async Task StoreImages(TmdbTvShowAppends tvShowAppends)
    {
        List<Image> posters = tvShowAppends.Images.Posters.ToList()
            .ConvertAll<Image>(image => new Image
            {
                AspectRatio = image.AspectRatio,
                Height = image.Height,
                Iso6391 = image.Iso6391,
                FilePath = image.FilePath,
                Width = image.Width,
                VoteAverage = image.VoteAverage,
                VoteCount = image.VoteCount,
                TvId = tvShowAppends.Id,
                Type = "poster",
                Site = "https://image.tmdb.org/t/p/"
            });

        List<Image> backdrops = tvShowAppends.Images.Backdrops.ToList()
            .ConvertAll<Image>(image => new Image
            {
                AspectRatio = image.AspectRatio,
                Height = image.Height,
                Iso6391 = image.Iso6391,
                FilePath = image.FilePath,
                Width = image.Width,
                VoteAverage = image.VoteAverage,
                VoteCount = image.VoteCount,
                TvId = tvShowAppends.Id,
                Type = "backdrop",
                Site = "https://image.tmdb.org/t/p/"
            });

        List<Image> logos = tvShowAppends.Images.Logos.ToList()
            .ConvertAll<Image>(image => new Image
            {
                AspectRatio = image.AspectRatio,
                Height = image.Height,
                Iso6391 = image.Iso6391,
                FilePath = image.FilePath,
                Width = image.Width,
                VoteAverage = image.VoteAverage,
                VoteCount = image.VoteCount,
                TvId = tvShowAppends.Id,
                Type = "logo",
                Site = "https://image.tmdb.org/t/p/"
            });

        List<Image> images = posters
            .Concat(backdrops)
            .Concat(logos)
            .ToList();

        await using MediaContext mediaContext = new();
        await mediaContext.Images.UpsertRange(images)
            .On(v => new { v.FilePath, v.TvId })
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
                TvId = ti.TvId
            })
            .RunAsync();

        TmdbColorPaletteJob tvColorPaletteJob = new(tvShowAppends);
        JobDispatcher.Dispatch(tvColorPaletteJob, "image", 2);

        Logger.MovieDb($"TvShow {tvShowAppends.Name}: Images stored");
        await Task.CompletedTask;
    }

    private async Task StoreNetworks()
    {
        // List<Keyword> keywords = Show?.Networks.Results.ToList()
        //     .ConvertAll<Network>(x => new Network(x)).ToArray() ?? [];
        //
        // await _mediaContext.Networks.UpsertRange(keywords)
        //     .On(v => new { v.Id })
        //     .WhenMatched((ts, ti) => new Network
        //     {
        //         Id = ti.Id,
        //         Name = ti.Name,
        //     })
        //     .RunAsync();

        Logger.MovieDb($"TvShow {Show?.Name}: Networks stored");
        await Task.CompletedTask;
    }

    private async Task StoreCompanies()
    {
        // List<Company> companies = Show?.ProductionCompanies.Results.ToList()
        //     .ConvertAll<ProductionCompany>(x => new ProductionCompany(x)).ToArray() ?? [];
        //
        // await _mediaContext.Companies.UpsertRange(companies)
        //     .On(v => new { v.Id })
        //     .WhenMatched((ts, ti) => new ProductionCompany
        //     {
        //         Id = ti.Id,
        //         Name = ti.Name,
        //     })
        //     .RunAsync();

        Logger.MovieDb($"TvShow {Show?.Name}: Companies stored");
        await Task.CompletedTask;
    }

    private async Task StoreKeywords()
    {
        Keyword[] keywords = Show?.Keywords.Results.ToList()
            .ConvertAll<Keyword>(keyword => new Keyword
            {
                Id = keyword.Id,
                Name = keyword.Name
            }).ToArray() ?? [];

        await _mediaContext.Keywords.UpsertRange(keywords)
            .On(v => new { v.Id })
            .WhenMatched((ts, ti) => new Keyword
            {
                Id = ti.Id,
                Name = ti.Name
            })
            .RunAsync();

        KeywordTv[] keywordTvs = Show?.Keywords.Results.ToList()
            .ConvertAll<KeywordTv>(keyword => new KeywordTv
            {
                KeywordId = keyword.Id,
                TvId = Show.Id
            }).ToArray() ?? [];

        await _mediaContext.KeywordTv.UpsertRange(keywordTvs)
            .On(v => new { v.KeywordId, v.TvId })
            .WhenMatched((ts, ti) => new KeywordTv
            {
                KeywordId = ti.KeywordId,
                TvId = ti.TvId
            })
            .RunAsync();

        Logger.MovieDb($"TvShow {Show?.Name}: Keywords stored");
        await Task.CompletedTask;
    }

    private async Task StoreGenres()
    {
        try
        {
            GenreTv[] genreTvs = Show?.Genres.ToList()
                .ConvertAll<GenreTv>(genre => new GenreTv
                {
                    GenreId = genre.Id,
                    TvId = Show.Id
                }).ToArray() ?? [];

            await _mediaContext.GenreTv.UpsertRange(genreTvs)
                .On(v => new { v.GenreId, v.TvId })
                .WhenMatched((ts, ti) => new GenreTv
                {
                    GenreId = ti.GenreId,
                    TvId = ti.TvId
                })
                .RunAsync();

            Logger.MovieDb($"TvShow {Show?.Name}: Genres stored");
        }
        catch (Exception e)
        {
            Logger.MovieDb(e, LogEventLevel.Error);
        }

        await Task.CompletedTask;
    }

    private string GetMediaType()
    {
        if (Show is null) return "";

        string searchName = string.IsNullOrEmpty(Show.OriginalName)
            ? Show.Name
            : Show.OriginalName;

        bool isAnime = KitsuIo.IsAnime(searchName, Show.FirstAirDate.ParseYear()).Result;

        return isAnime ? "anime" : "tv";
    }

    private string FindFolder()
    {
        return Show == null ? "" : FileNameParsers.CreateBaseFolder(Show);
    }

    private async Task Store()
    {
        if (Show == null) return;

        Tv tvResponse = new()
        {
            Id = Show.Id,
            Backdrop = Show.BackdropPath,
            Duration = Show.EpisodeRunTime?.Length > 0
                ? (int?)Show.EpisodeRunTime?.Average()
                : 0,
            FirstAirDate = Show.FirstAirDate,
            HaveEpisodes = 0,
            Homepage = Show.Homepage?.ToString(),
            ImdbId = Show.ExternalIds.ImdbId,
            InProduction = Show.InProduction,
            LastEpisodeToAir = Show.LastEpisodeToAir?.Id,
            NextEpisodeToAir = Show.NextEpisodeToAir?.Id,
            NumberOfEpisodes = Show.NumberOfEpisodes,
            NumberOfSeasons = Show.NumberOfSeasons,
            OriginCountry = Show.OriginCountry.Length > 0 ? Show.OriginCountry[0] : null,
            OriginalLanguage = Show.OriginalLanguage,
            Overview = Show.Overview,
            Popularity = Show.Popularity,
            Poster = Show.PosterPath,
            SpokenLanguages = Show.SpokenLanguages.Length > 0 ? Show.SpokenLanguages[0].Name : null,
            Status = Show.Status,
            Tagline = Show.Tagline,
            Title = Show.Name,
            TitleSort = Show.Name.TitleSort(Show.FirstAirDate),
            Trailer = Show.Videos.Results.Length > 0 ? Show.Videos.Results[0].Key : null,
            TvdbId = Show.ExternalIds.TvdbId,
            Type = Show.Type,
            VoteAverage = Show.VoteAverage,
            VoteCount = Show.VoteCount,
            Folder = Folder,
            LibraryId = library.Id,
            MediaType = "tv",
            _colorPalette = await MovieDbImage
                .MultiColorPalette(new[]
                {
                    new BaseImage.MultiStringType("poster", Show.PosterPath),
                    new BaseImage.MultiStringType("backdrop", Show.BackdropPath)
                })
        };

        await _mediaContext.Tvs.Upsert(tvResponse)
            .On(v => new { v.Id })
            .WhenMatched((ts, ti) => new Tv
            {
                Id = ti.Id,
                Backdrop = ti.Backdrop,
                Duration = ti.Duration,
                FirstAirDate = ti.FirstAirDate,
                Homepage = ti.Homepage,
                ImdbId = ti.ImdbId,
                InProduction = ti.InProduction,
                LastEpisodeToAir = ti.LastEpisodeToAir,
                NextEpisodeToAir = ti.NextEpisodeToAir,
                NumberOfEpisodes = ti.NumberOfEpisodes,
                NumberOfSeasons = ti.NumberOfSeasons,
                OriginCountry = ti.OriginCountry,
                OriginalLanguage = ti.OriginalLanguage,
                Overview = ti.Overview,
                Popularity = ti.Popularity,
                Poster = ti.Poster,
                SpokenLanguages = ti.SpokenLanguages,
                Status = ti.Status,
                Tagline = ti.Tagline,
                Title = ti.Title,
                TitleSort = ti.TitleSort,
                Trailer = ti.Trailer,
                TvdbId = ti.TvdbId,
                Type = ti.Type,
                VoteAverage = ti.VoteAverage,
                VoteCount = ti.VoteCount,
                Folder = ti.Folder,
                LibraryId = ti.LibraryId,
                MediaType = ti.MediaType,
                UpdatedAt = ti.UpdatedAt
            })
            .RunAsync();

        await _mediaContext.LibraryTv.Upsert(new LibraryTv(library.Id, Show.Id))
            .On(v => new { v.LibraryId, v.TvId })
            .WhenMatched((lts, lti) => new LibraryTv
            {
                LibraryId = lti.LibraryId,
                TvId = lti.TvId
            })
            .RunAsync();

        Logger.MovieDb($"TvShow {Show.Name}: TvShow stored");
    }

    private Task DispatchJobs()
    {
        if (Show == null) return Task.CompletedTask;

        FindMediaFilesJob findMediaFilesJob = new(id, library);
        JobDispatcher.Dispatch(findMediaFilesJob, "queue", 6);

        TmdbPersonJob personJob = new(new TmdbTvShowAppends()
        {
            Id = id,
            Credits = Show.Credits
        });
        JobDispatcher.Dispatch(personJob, "queue", 3);

        TmdbImagesJob imagesJob = new(new TmdbTvShowAppends()
        {
            Id = id,
            Images = Show.Images
        });
        JobDispatcher.Dispatch(imagesJob, "data", 2);

        return Task.CompletedTask;
    }

    public static async Task Palette(int id, bool download = true)
    {
        Logger.Queue($"Fetching color palette for TvShow {id}");

        await using MediaContext mediaContext = new();
        Tv? tv = await mediaContext.Tvs
            .Where(e => e._colorPalette == "")
            .FirstOrDefaultAsync(e => e.Id == id);

        if (tv is { _colorPalette: "" })
        {
            tv._colorPalette = await MovieDbImage
                .MultiColorPalette(new[]
                {
                    new BaseImage.MultiStringType("poster", tv.Poster),
                    new BaseImage.MultiStringType("backdrop", tv.Backdrop)
                });

            await mediaContext.SaveChangesAsync();
        }

        Image[] images = await mediaContext.Images
            .Where(e => e.TvId == id)
            .Where(e => e._colorPalette == "")
            .Where(e => e.Iso6391 == null || e.Iso6391 == "en" || e.Iso6391 == "" ||
                        e.Iso6391 == CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
            .ToArrayAsync();

        foreach (Image image in images)
        {
            if (string.IsNullOrEmpty(image.FilePath)) continue;

            image._colorPalette = await MovieDbImage.ColorPalette("image", image.FilePath);
        }

        await mediaContext.SaveChangesAsync();
    }

    public static async Task SimilarPalette(int id)
    {
        await using MediaContext mediaContext = new();

        List<Similar> similarList = await mediaContext.Similar
            .Where(e => e.TvFromId == id && e._colorPalette == "")
            .ToListAsync();

        if (similarList.Count is 0) return;

        foreach (Similar similar in similarList)
        {
            if (similar is not { _colorPalette: "" }) continue;

            string palette = await MovieDbImage
                .MultiColorPalette(new[]
                {
                    new BaseImage.MultiStringType("poster", similar.Poster),
                    new BaseImage.MultiStringType("backdrop", similar.Backdrop)
                });

            similar._colorPalette = palette;
        }

        await mediaContext.SaveChangesAsync();
    }

    public static async Task RecommendationPalette(int id)
    {
        await using MediaContext mediaContext = new();

        List<Recommendation> recommendations = await mediaContext.Recommendations
            .Where(e => e.TvFromId == id && e._colorPalette == "")
            .ToListAsync();

        if (recommendations.Count is 0) return;

        foreach (Recommendation recommendation in recommendations)
        {
            if (recommendation is not { _colorPalette: "" }) continue;

            string palette = await MovieDbImage
                .MultiColorPalette(new[]
                {
                    new BaseImage.MultiStringType("poster", recommendation.Poster),
                    new BaseImage.MultiStringType("backdrop", recommendation.Backdrop)
                });

            recommendation._colorPalette = palette;
        }

        await mediaContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        _mediaContext.Dispose();
        TmdbTvClient.Dispose();
        GC.Collect();
        GC.WaitForFullGCComplete();
        GC.WaitForPendingFinalizers();
    }

    public async ValueTask DisposeAsync()
    {
        await _mediaContext.DisposeAsync();
        GC.Collect();
        GC.WaitForFullGCComplete();
        GC.WaitForPendingFinalizers();
    }
}