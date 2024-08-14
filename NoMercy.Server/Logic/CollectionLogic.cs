using System.Globalization;
using Microsoft.EntityFrameworkCore;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.Helpers;
using NoMercy.Networking;
using NoMercy.NmSystem;
using NoMercy.Providers.TMDB.Client;
using NoMercy.Providers.TMDB.Models.Collections;
using NoMercy.Server.app.Jobs;
using NoMercy.Server.Logic.ImageLogic;
using NoMercy.Server.system;
using Collection = NoMercy.Database.Models.Collection;
using Movie = NoMercy.Database.Models.Movie;

namespace NoMercy.Server.Logic;

public class CollectionLogic(TmdbCollectionAppends collectionAppends, Library library) : IDisposable, IAsyncDisposable
{
    private MediaContext MediaContext { get; set; } = new();

    public async Task Process()
    {
        await Store();

        await StoreTranslations();

        await DispatchJobs();
    }

    private async Task Store()
    {
        var collections = new Collection{
            Id = collectionAppends.Id,
            Title = collectionAppends.Name,
            TitleSort = collectionAppends.Name.TitleSort(collectionAppends.Parts.MinBy(movie => movie.ReleaseDate)?.ReleaseDate),
            Backdrop = collectionAppends.BackdropPath,
            Poster = collectionAppends.PosterPath,
            Overview = collectionAppends.Overview,
            Parts = collectionAppends.Parts.Length,
            LibraryId = library.Id,
            _colorPalette = MovieDbImage
                .MultiColorPalette(new[]
                {
                    new BaseImage.MultiStringType("poster", collectionAppends.PosterPath),
                    new BaseImage.MultiStringType("backdrop", collectionAppends.BackdropPath)
                }).Result
        };

        await MediaContext.Collections.Upsert(collections)
            .On(v => new { v.Id })
            .WhenMatched((ts, ti) => new Collection
            {
                Id = ti.Id,
                Backdrop = ti.Backdrop,
                Poster = ti.Poster,
                Title = ti.Title,
                Overview = ti.Overview,
                Parts = ti.Parts,
                LibraryId = ti.LibraryId,
                TitleSort = ti.TitleSort,
                _colorPalette = ti._colorPalette
            })
            .RunAsync();

        Logger.MovieDb($"Collection {collectionAppends.Name} stored");

        var movies = collectionAppends.Parts.ToList()
            .ConvertAll<Movie>(movie => new Movie {
                    Id = movie.Id,
                    Title = movie.Title,
                    TitleSort = movie.Title.TitleSort(movie.ReleaseDate),
                    Adult = movie.Adult,
                    Backdrop = movie.BackdropPath,
                    OriginalTitle = movie.OriginalTitle,
                    OriginalLanguage = movie.OriginalLanguage,
                    Overview = movie.Overview,
                    Popularity = movie.Popularity,
                    Poster = movie.PosterPath,
                    ReleaseDate = movie.ReleaseDate,
                    Tagline = movie.Tagline,
                    Trailer = movie.Video,
                    Video = movie.Video,
                    VoteAverage = movie.VoteAverage,
                    VoteCount = movie.VoteCount,
                
                    LibraryId = library.Id,
                }).ToArray();

        await MediaContext.Movies.UpsertRange(movies)
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
                UpdatedAt = ti.UpdatedAt,
                LibraryId = ti.LibraryId
            })
            .RunAsync();

        // foreach (Movie movie in movies)
        // {
        //     ColorPaletteJob colorPaletteJob = new(id: movie.Id, model: "movie");
        //     JobDispatcher.Dispatch(colorPaletteJob, "image", 2);
        // }

        Logger.MovieDb($"Collection {collectionAppends.Name} movies stored");

        var collectionMovies = collectionAppends.Parts.ToList()
            .ConvertAll<CollectionMovie>(CollectionMovie => new CollectionMovie {
                    MovieId = CollectionMovie.Id,
                    CollectionId = collections.Id,
                }).ToArray();

        await MediaContext.CollectionMovie.UpsertRange(collectionMovies)
            .On(v => new { v.CollectionId, v.MovieId })
            .WhenMatched((ts, ti) => new CollectionMovie
            {
                CollectionId = ti.CollectionId,
                MovieId = ti.MovieId
            })
            .RunAsync();

        await Task.CompletedTask;
    }

    private async Task DispatchJobs()
    {
        Networking.Networking.SendToAll("RefreshLibrary", "socket", new RefreshLibraryDto
        {
            QueryKey = ["special", collectionAppends.Id]
        });

        foreach (var movie in collectionAppends.Parts)
        {
            TmdbMovieClient tmdbMovieClient = new(movie.Id);
            var movieAppends = await tmdbMovieClient.WithAllAppends();
            if (movieAppends is null) continue;

            Logger.MovieDb($"Collection {collectionAppends.Name}: Dispatching movie {movieAppends.Title}");

            TmdbPersonJob tmdbPersonJob = new(movieAppends);
            JobDispatcher.Dispatch(tmdbPersonJob, "queue", 3);

            TmdbImagesJob movieImagesJob = new(movieAppends);
            JobDispatcher.Dispatch(movieImagesJob, "data", 2);
        }

        TmdbImagesJob imagesJob = new(collectionAppends);
        JobDispatcher.Dispatch(imagesJob, "data", 2);
    }

    private async Task StoreTranslations()
    {
        var translations = collectionAppends.Translations.Translations.ToList()
            .ConvertAll<Translation>(translation => new Translation {
                    Iso31661 = translation.Iso31661,
                    Iso6391 = translation.Iso6391,
                    Name = translation.Name == "" ? null : translation.Name,
                    Title = translation.Data.Title == "" ? null : translation.Data.Title,
                    Overview = translation.Data.Overview == "" ? null : translation.Data.Overview,
                    EnglishName = translation.EnglishName,
                    Homepage = translation.Data.Homepage?.ToString(),
                    CollectionId = collectionAppends.Id,
                }).ToArray();

        await using MediaContext mediaContext = new();
        await mediaContext.Translations
            .UpsertRange(translations.Where(translation => translation.Title != null || translation.Overview != ""))
            .On(t => new { t.Iso31661, t.Iso6391, t.CollectionId })
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
                SeasonId = ti.SeasonId,
                EpisodeId = ti.EpisodeId,
                CollectionId = ti.CollectionId,
                PersonId = ti.PersonId
            })
            .RunAsync();
    }

    public static async Task StoreImages(TmdbCollectionAppends collection)
    {
        var posters = collection.Images.Posters.ToList()
            .ConvertAll<Image>(image => new Image {
                    AspectRatio = image.AspectRatio,
                    FilePath = image.FilePath,
                    Height = image.Height,
                    Iso6391 = image.Iso6391,
                    VoteAverage = image.VoteAverage,
                    VoteCount = image.VoteCount,
                    Width = image.Width,
                    CollectionId = collection.Id,
                    Type = "poster",
                    Site = "https://image.tmdb.org/t/p/",
                });

        var backdrops = collection.Images.Backdrops.ToList()
            .ConvertAll<Image>(image => new Image {
                AspectRatio = image.AspectRatio,
                FilePath = image.FilePath,
                Height = image.Height,
                Iso6391 = image.Iso6391,
                VoteAverage = image.VoteAverage,
                VoteCount = image.VoteCount,
                Width = image.Width,
                CollectionId = collection.Id,
                Type = "backdrop",
                Site = "https://image.tmdb.org/t/p/",
            });

        // TODO: Future for when they add logos
        // List<Image> logos = special?.Images?.Logos.ToList()
        //     .ConvertAll<Image>(image => new Image(image:image, special:special, type:"logo"));

        var images = posters
            .Concat(backdrops)
            // .Concat(logos)
            .ToList();

        await using MediaContext mediaContext = new();
        await mediaContext.Images.UpsertRange(images)
            .On(v => new { v.FilePath, v.CollectionId })
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
                CollectionId = ti.CollectionId
            })
            .RunAsync();

        // ColorPaletteJob colorPaletteJob = new(id: id, model: "special");
        // JobDispatcher.Dispatch(colorPaletteJob, "image", 2);

        Logger.MovieDb($"Collection {collection.Name}: Images stored");
        await Task.CompletedTask;
    }

    public static async Task Palette(int id)
    {
        Logger.Queue($"Fetching color palette for Special {id}");
        await using MediaContext mediaContext = new();

        var collection = await mediaContext.Collections
            .Where(e => e._colorPalette == "")
            .FirstOrDefaultAsync(e => e.Id == id);

        if (collection is { _colorPalette: "" })
        {
            collection._colorPalette = await MovieDbImage
                .MultiColorPalette(new[]
                {
                    new BaseImage.MultiStringType("poster", collection.Poster),
                    new BaseImage.MultiStringType("backdrop", collection.Backdrop)
                });

            await mediaContext.SaveChangesAsync();
        }

        var images = await mediaContext.Images
            .Where(e => e.CollectionId == id)
            .Where(e => e._colorPalette == "")
            .Where(e => e.Iso6391 == null || e.Iso6391 == "en" || e.Iso6391 == "" ||
                        e.Iso6391 == CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
            .ToArrayAsync();

        foreach (var image in images)
        {
            if (string.IsNullOrEmpty(image.FilePath)) continue;

            Logger.Queue($"Fetching color palette for Collection Image {image.FilePath}");

            TmdbColorPaletteJob tmdbColorPaletteJob = new(image.FilePath, "image", image.Iso6391);
            JobDispatcher.Dispatch(tmdbColorPaletteJob, "image", 2);
        }
    }

    public void Dispose()
    {
        MediaContext.Dispose();
        GC.Collect();
        GC.WaitForFullGCComplete();
        GC.WaitForPendingFinalizers();
    }

    public async ValueTask DisposeAsync()
    {
        await MediaContext.DisposeAsync();
        GC.Collect();
        GC.WaitForFullGCComplete();
        GC.WaitForPendingFinalizers();
    }
}