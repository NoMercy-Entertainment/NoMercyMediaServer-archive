using System.Globalization;
using Microsoft.EntityFrameworkCore;
using NoMercy.Data.Jobs;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.MediaProcessing.Images;
using NoMercy.NmSystem;
using NoMercy.Providers.TMDB.Client;
using NoMercy.Providers.TMDB.Models.Movies;
using NoMercy.Queue;
using Serilog.Events;

namespace NoMercy.Data.Logic;

public class MovieLogic(int id, Library library) : IDisposable, IAsyncDisposable
{
    private TmdbMovieClient TmdbMovieClient { get; } = new(id);
    private string MediaType { get; set; } = "";
    private string Folder { get; set; } = "";

    public TmdbMovieAppends? Movie { get; private set; }

    private readonly MediaContext _mediaContext = new();

    public async Task Process()
    {
        // Movie? movie = _mediaContext.Movies.FirstOrDefault(e => e.Id == Id);
        // if (movie is not null)
        // {
        //     Logger.MovieDb($"Movie {movie.Title}: already exists");
        //     return;
        // }

        Movie = await TmdbMovieClient.WithAllAppends();
        if (Movie is null)
        {
            Logger.MovieDb($"Movie {id}: not found");
            return;
        }

        Folder = FindFolder();
        MediaType = MakeMediaType();

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

        await DispatchJobs();
    }

    private async Task StoreAlternativeTitles()
    {
        AlternativeTitle[] alternativeTitles = Movie?.AlternativeTitles.Results.ToList()
            .ConvertAll<AlternativeTitle>(tmdbMovieAlternativeTitles => new AlternativeTitle
            {
                Iso31661 = tmdbMovieAlternativeTitles.Iso31661,
                Title = tmdbMovieAlternativeTitles.Title,
                MovieId = Movie.Id
            }).ToArray() ?? [];

        await _mediaContext.AlternativeTitles.UpsertRange(alternativeTitles)
            .On(a => new { a.Title, a.MovieId })
            .WhenMatched((ats, ati) => new AlternativeTitle
            {
                Title = ati.Title,
                Iso31661 = ati.Iso31661,
                MovieId = ati.MovieId
            })
            .RunAsync();

        Logger.MovieDb($"Movie {Movie?.Title}: AlternativeTitles stored");
    }

    private async Task StoreWatchProviders()
    {
        Logger.MovieDb($"Movie {Movie?.Title}: WatchProviders stored");
        await Task.CompletedTask;
    }

    private async Task StoreTranslations()
    {
        try
        {
            Translation[] translations = Movie?.Translations.Translations.ToList()
                .ConvertAll<Translation>(translation => new Translation
                {
                    Iso31661 = translation.Iso31661,
                    Iso6391 = translation.Iso6391,
                    Name = translation.Name == "" ? null : translation.Name,
                    Title = translation.Data.Title == "" ? null : translation.Data.Title,
                    Overview = translation.Data.Overview == "" ? null : translation.Data.Overview,
                    EnglishName = translation.EnglishName,
                    Homepage = translation.Data.Homepage?.ToString(),
                    MovieId = Movie.Id
                }).ToArray() ?? [];

            await _mediaContext.Translations
                .UpsertRange(translations.Where(translation => translation.Title != "" || translation.Overview != ""))
                .On(t => new { t.Iso31661, t.Iso6391, t.MovieId })
                .WhenMatched((ts, ti) => new Translation
                {
                    Iso31661 = ti.Iso31661,
                    Iso6391 = ti.Iso6391,
                    Title = ti.Title,
                    EnglishName = ti.EnglishName,
                    Name = ti.Name,
                    Overview = ti.Overview,
                    Homepage = ti.Homepage,
                    Biography = ti.Biography,
                    MovieId = ti.MovieId,
                    SeasonId = ti.SeasonId,
                    EpisodeId = ti.EpisodeId,
                    CollectionId = ti.CollectionId,
                    PersonId = ti.PersonId
                })
                .RunAsync();
        }
        catch (Exception e)
        {
            Logger.MovieDb(e, LogEventLevel.Error);
            throw;
        }
    }

    private async Task StoreContentRatings()
    {
        try
        {
            List<CertificationMovie> certifications = [];

            foreach (TmdbReleaseDatesResult movieContentRating in Movie?.ReleaseDates.Results ?? [])
            {
                Certification? certification = _mediaContext.Certifications
                    .FirstOrDefault(c =>
                        c.Iso31661 == movieContentRating.Iso31661 &&
                        c.Rating == movieContentRating.ReleaseDates[0].Certification);
                if (certification is null) continue;

                certifications.Add(new CertificationMovie
                {
                    CertificationId = certification.Id,
                    MovieId = Movie!.Id
                });
            }

            await _mediaContext.CertificationMovie.UpsertRange(certifications)
                .On(v => new { v.CertificationId, v.MovieId })
                .WhenMatched((ts, ti) => new CertificationMovie
                {
                    CertificationId = ti.CertificationId,
                    MovieId = ti.MovieId
                })
                .RunAsync();
        }
        catch (Exception e)
        {
            Logger.App(e, LogEventLevel.Fatal);
            throw;
        }

        Logger.MovieDb($"Movie {Movie?.Title}: ContentRatings stored");
        await Task.CompletedTask;
    }

    private async Task StoreSimilar()
    {
        List<Similar> data = Movie?.Similar.Results.ToList()
            .ConvertAll<Similar>(tmdbSimilar => new Similar
            {
                Backdrop = tmdbSimilar.BackdropPath,
                Overview = tmdbSimilar.Overview,
                Poster = tmdbSimilar.PosterPath,
                Title = tmdbSimilar.Title,
                TitleSort = tmdbSimilar.Title,
                MediaId = tmdbSimilar.Id,
                MovieFromId = Movie.Id
            }) ?? [];

        await _mediaContext.Similar.UpsertRange(data)
            .On(v => new { v.MediaId, v.MovieFromId })
            .WhenMatched((ts, ti) => new Similar
            {
                MovieToId = ti.MovieToId,
                MovieFromId = ti.MovieFromId,
                Overview = ti.Overview,
                Title = ti.Title,
                TitleSort = ti.TitleSort,
                Backdrop = ti.Backdrop,
                Poster = ti.Poster,
                MediaId = ti.MediaId
            })
            .RunAsync();

        TmdbColorPaletteJob similarTmdbColorPaletteJob = new(id, model: "similar", type: "movie");
        JobDispatcher.Dispatch(similarTmdbColorPaletteJob, "image", 2);

        Logger.MovieDb($"Movie {Movie?.Title}: Similar stored");
        await Task.CompletedTask;
    }

    private async Task StoreRecommendations()
    {
        List<Recommendation> data = Movie?.Recommendations.Results.ToList()
            .ConvertAll<Recommendation>(movie => new Recommendation
            {
                Backdrop = movie.BackdropPath,
                Overview = movie.Overview,
                Poster = movie.PosterPath,
                Title = movie.Title,
                TitleSort = movie.Title.TitleSort(),
                MediaId = movie.Id,
                MovieFromId = Movie.Id
            }) ?? [];

        await _mediaContext.Recommendations.UpsertRange(data)
            .On(v => new { v.MediaId, v.MovieFromId })
            .WhenMatched((ts, ti) => new Recommendation
            {
                MovieToId = ti.MovieToId,
                MovieFromId = ti.MovieFromId,
                Overview = ti.Overview,
                Title = ti.Title,
                TitleSort = ti.TitleSort,
                Backdrop = ti.Backdrop,
                Poster = ti.Poster,
                MediaId = ti.MediaId
            })
            .RunAsync();

        TmdbColorPaletteJob recommendationTmdbColorPaletteJob = new(id, model: "recommendation", type: "movie");
        JobDispatcher.Dispatch(recommendationTmdbColorPaletteJob, "image", 2);

        Logger.MovieDb($"Movie {Movie?.Title}: Recommendations stored");
        await Task.CompletedTask;
    }

    private async Task StoreVideos()
    {
        try
        {
            List<Media> videos = Movie?.Videos.Results.ToList()
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
                    MovieId = Movie.Id
                }) ?? [];

            await _mediaContext.Medias.UpsertRange(videos)
                .On(v => new { v.Src, v.MovieId })
                .WhenMatched((ts, ti) => new Media
                {
                    Src = ti.Src,
                    Iso6391 = ti.Iso6391,
                    Type = ti.Type,
                    MovieId = ti.MovieId,
                    Name = ti.Name,
                    Site = ti.Site,
                    Size = ti.Size
                })
                .RunAsync();
        }
        catch (Exception e)
        {
            Logger.App(e, LogEventLevel.Error);
        }

        Logger.MovieDb($"Movie {Movie?.Title}: Videos stored");
        await Task.CompletedTask;
    }

    public static async Task StoreImages(TmdbMovieAppends movie)
    {
        List<Image> posters = movie.Images.Posters.ToList()
            .ConvertAll<Image>(image => new Image
            {
                AspectRatio = image.AspectRatio,
                FilePath = image.FilePath,
                Height = image.Height,
                Iso6391 = image.Iso6391,
                VoteAverage = image.VoteAverage,
                VoteCount = image.VoteCount,
                Width = image.Width,
                MovieId = movie.Id,
                Type = "poster",
                Site = "https://image.tmdb.org/t/p/"
            });

        List<Image> backdrops = movie.Images.Backdrops.ToList()
            .ConvertAll<Image>(image => new Image
            {
                AspectRatio = image.AspectRatio,
                FilePath = image.FilePath,
                Height = image.Height,
                Iso6391 = image.Iso6391,
                VoteAverage = image.VoteAverage,
                VoteCount = image.VoteCount,
                Width = image.Width,
                MovieId = movie.Id,
                Type = "backdrop",
                Site = "https://image.tmdb.org/t/p/"
            });

        List<Image> logos = movie.Images.Logos.ToList()
            .ConvertAll<Image>(image => new Image
            {
                AspectRatio = image.AspectRatio,
                FilePath = image.FilePath,
                Height = image.Height,
                Iso6391 = image.Iso6391,
                VoteAverage = image.VoteAverage,
                VoteCount = image.VoteCount,
                Width = image.Width,
                MovieId = movie.Id,
                Type = "logo",
                Site = "https://image.tmdb.org/t/p/"
            });

        List<Image> images = posters
            .Concat(backdrops)
            .Concat(logos)
            .ToList();

        await using MediaContext mediaContext = new();
        await mediaContext.Images.UpsertRange(images)
            .On(v => new { v.FilePath, v.MovieId })
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
                MovieId = ti.MovieId
            })
            .RunAsync();

        TmdbColorPaletteJob movieTmdbColorPaletteJob = new(movie);
        JobDispatcher.Dispatch(movieTmdbColorPaletteJob, "image", 2);

        Logger.MovieDb($"Movie {movie.Title}: Images stored");
        await Task.CompletedTask;
    }

    private async Task StoreNetworks()
    {
        // List<Keyword> keywords = Movie?.Networks.Results.ToList()
        //     .ConvertAll<Network>(x => new Network(x)).ToArray() ?? [];
        //
        // await _mediaContext.Networks.UpsertRange(keywords)
        //     .On(v => new { v.Id })
        //     .WhenMatched((ts, ti) => new Network
        //     {
        //         Id = ti.Id,
        //         Title = ti.Title,
        //     })
        //     .RunAsync();

        Logger.MovieDb($"Movie {Movie?.Title}: Networks stored");
        await Task.CompletedTask;
    }

    private async Task StoreCompanies()
    {
        // List<Company> companies = Movie?.ProductionCompanies.Results.ToList()
        //     .ConvertAll<ProductionCompany>(x => new ProductionCompany(x)).ToArray() ?? [];
        //
        // await _mediaContext.Companies.UpsertRange(companies)
        //     .On(v => new { v.Id })
        //     .WhenMatched((ts, ti) => new ProductionCompany
        //     {
        //         Id = ti.Id,
        //         Title = ti.Title,
        //     })
        //     .RunAsync();

        Logger.MovieDb($"Movie {Movie?.Title}: Companies stored");
        await Task.CompletedTask;
    }

    private async Task StoreKeywords()
    {
        Keyword[] keywords = Movie?.Keywords.Results.ToList()
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

        KeywordMovie[] keywordMovies = Movie?.Keywords.Results.ToList()
            .ConvertAll<KeywordMovie>(keyword => new KeywordMovie
            {
                KeywordId = keyword.Id,
                MovieId = Movie.Id
            }).ToArray() ?? [];

        await _mediaContext.KeywordMovie.UpsertRange(keywordMovies)
            .On(v => new { v.KeywordId, v.MovieId })
            .WhenMatched((ts, ti) => new KeywordMovie()
            {
                KeywordId = ti.KeywordId,
                MovieId = ti.MovieId
            })
            .RunAsync();

        Logger.MovieDb($"Movie {Movie?.Title}: Keywords stored");
        await Task.CompletedTask;
    }

    private async Task StoreGenres()
    {
        GenreMovie[] genreMovies = Movie?.Genres.ToList()
            .ConvertAll<GenreMovie>(genre => new GenreMovie
            {
                GenreId = genre.Id,
                MovieId = Movie.Id
            }).ToArray() ?? [];

        await _mediaContext.GenreMovie.UpsertRange(genreMovies)
            .On(v => new { v.GenreId, v.MovieId })
            .WhenMatched((ts, ti) => new GenreMovie
            {
                GenreId = ti.GenreId,
                MovieId = ti.MovieId
            })
            .RunAsync();

        Logger.MovieDb($"Movie {Movie?.Title}: Genres stored");
        await Task.CompletedTask;
    }

    private static string MakeMediaType()
    {
        const string defaultMediaType = "movie";

        return defaultMediaType;
    }

    private string FindFolder()
    {
        if (Movie == null) return "";

        return FileNameParsers.CreateBaseFolder(Movie);
    }

    private async Task Store()
    {
        if (Movie == null) return;

        Movie movieResponse = new()
        {
            Id = Movie.Id,
            Title = Movie.Title,
            TitleSort = Movie.Title.TitleSort(Movie.ReleaseDate),
            Duration = Movie.Runtime,
            Folder = Folder,
            Adult = Movie.Adult,
            Backdrop = Movie.BackdropPath,
            Budget = Movie.Budget,
            Homepage = Movie.Homepage?.ToString(),
            ImdbId = Movie.ImdbId,
            OriginalTitle = Movie.OriginalTitle,
            OriginalLanguage = Movie.OriginalLanguage,
            Overview = Movie.Overview,
            Popularity = Movie.Popularity,
            Poster = Movie.PosterPath,
            ReleaseDate = Movie.ReleaseDate,
            Revenue = Movie.Revenue,
            Runtime = Movie.Runtime,
            Status = Movie.Status,
            Tagline = Movie.Tagline,
            Trailer = Movie.Video,
            Video = Movie.Video,
            VoteAverage = Movie.VoteAverage,
            VoteCount = Movie.VoteCount,
            LibraryId = library.Id,
            _colorPalette = await MovieDbImage
                .MultiColorPalette(new[]
                {
                    new BaseImage.MultiStringType("poster", Movie.PosterPath),
                    new BaseImage.MultiStringType("backdrop", Movie.BackdropPath)
                })
        };

        await _mediaContext.Movies.Upsert(movieResponse)
            .On(v => new { v.Id })
            .WhenMatched((ts, ti) => new Movie
            {
                Id = ti.Id,
                Backdrop = ti.Backdrop,
                Duration = ti.Duration,
                ReleaseDate = ti.ReleaseDate,
                Homepage = ti.Homepage,
                ImdbId = ti.ImdbId,
                OriginalLanguage = ti.OriginalLanguage,
                Overview = ti.Overview,
                Popularity = ti.Popularity,
                Poster = ti.Poster,
                Status = ti.Status,
                Tagline = ti.Tagline,
                Title = ti.Title,
                TitleSort = ti.TitleSort,
                Trailer = ti.Trailer,
                VoteAverage = ti.VoteAverage,
                VoteCount = ti.VoteCount,
                Folder = ti.Folder,
                LibraryId = ti.LibraryId,
                UpdatedAt = ti.UpdatedAt
            })
            .RunAsync();

        await _mediaContext.LibraryMovie.Upsert(new LibraryMovie(library.Id, Movie.Id))
            .On(v => new { v.LibraryId, v.MovieId })
            .WhenMatched((lts, lti) => new LibraryMovie
            {
                LibraryId = lti.LibraryId,
                MovieId = lti.MovieId
            })
            .RunAsync();

        Logger.MovieDb($"Movie {Movie.Title}: Movie stored");
    }

    private Task DispatchJobs()
    {
        if (Movie is null) return Task.CompletedTask;

        FindMediaFilesJob findMediaFilesJob = new(id, library);
        JobDispatcher.Dispatch(findMediaFilesJob, "queue", 6);

        TmdbPersonJob tmdbPersonJob = new(new TmdbMovieAppends()
        {
            Id = id,
            Credits = Movie.Credits
        });
        JobDispatcher.Dispatch(tmdbPersonJob, "queue", 3);

        TmdbImagesJob imagesJob = new(new TmdbMovieAppends()
        {
            Id = id,
            Images = Movie.Images
        });
        JobDispatcher.Dispatch(imagesJob, "data", 2);

        if (Movie.BelongsToCollection is null) return Task.CompletedTask;

        TmdbCollectionJob tmdbCollectionJob = new(Movie.BelongsToCollection, library);
        JobDispatcher.Dispatch(tmdbCollectionJob, "queue", 4);

        return Task.CompletedTask;
    }

    public static async Task Palette(int id, bool download = true)
    {
        Logger.Queue($"Fetching color palette for Movie {id}");

        await using MediaContext mediaContext = new();
        Movie? movie = await mediaContext.Movies
            .Where(e => e._colorPalette == "")
            .FirstOrDefaultAsync(e => e.Id == id);

        if (movie is { _colorPalette: "" })
        {
            movie._colorPalette = await MovieDbImage
                .MultiColorPalette(new[]
                {
                    new BaseImage.MultiStringType("poster", movie.Poster),
                    new BaseImage.MultiStringType("backdrop", movie.Backdrop)
                });

            await mediaContext.SaveChangesAsync();
        }

        Image[] images = await mediaContext.Images
            .Where(e => e.MovieId == id)
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
            .Where(e => e.MovieFromId == id && e._colorPalette == "")
            .ToListAsync();

        if (similarList.Count is 0) return;

        foreach (Similar similar in similarList)
        {
            if (similar is not { _colorPalette: "" }) continue;

            similar._colorPalette = await MovieDbImage
                .MultiColorPalette(new[]
                {
                    new BaseImage.MultiStringType("poster", similar.Poster),
                    new BaseImage.MultiStringType("backdrop", similar.Backdrop)
                });
        }

        await mediaContext.SaveChangesAsync();
    }

    public static async Task RecommendationPalette(int id)
    {
        await using MediaContext mediaContext = new();

        List<Recommendation> recommendations = await mediaContext.Recommendations
            .Where(e => e.MovieFromId == id && e._colorPalette == "")
            .ToListAsync();

        if (recommendations.Count is 0) return;

        foreach (Recommendation recommendation in recommendations)
        {
            if (recommendation is not { _colorPalette: "" }) continue;

            recommendation._colorPalette = await MovieDbImage
                .MultiColorPalette(new[]
                {
                    new BaseImage.MultiStringType("poster", recommendation.Poster),
                    new BaseImage.MultiStringType("backdrop", recommendation.Backdrop)
                });
        }

        await mediaContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        Movie = null;
        _mediaContext.Dispose();
        TmdbMovieClient.Dispose();
        GC.Collect();
        GC.WaitForFullGCComplete();
        GC.WaitForPendingFinalizers();
    }

    public async ValueTask DisposeAsync()
    {
        Movie = null;
        await _mediaContext.DisposeAsync();
        GC.Collect();
        GC.WaitForFullGCComplete();
        GC.WaitForPendingFinalizers();
    }
}