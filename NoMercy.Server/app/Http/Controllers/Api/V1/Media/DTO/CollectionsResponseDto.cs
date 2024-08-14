using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.NmSystem;
using NoMercy.Server.app.Http.Controllers.Api.V1.DTO;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace NoMercy.Server.app.Http.Controllers.Api.V1.Media.DTO;

public record CollectionsResponseDto
{
    [JsonProperty("nextId")] public object NextId { get; set; }

    [JsonProperty("data")] public IOrderedEnumerable<CollectionsResponseItemDto> Data { get; set; }

    public static async Task<List<Collection>> GetCollections(Guid userId, string language, int take, int page = 0)
    {        
        await using MediaContext mediaContext = new();

        var query = mediaContext.Collections.AsNoTracking()
            .Where(collection => collection.Library.LibraryUsers
                .Any(u => u.UserId == userId)
            )
            .Where(collection => collection.CollectionMovies
                .Any(collectionMovie => collectionMovie.Movie.VideoFiles.Count != 0)
            )

            .Include(collection => collection.Images)

            .Include(collection => collection.Translations
                .Where(translation => translation.Iso6391 == language))

            .Include(collection => collection.CollectionMovies)
                .ThenInclude(movie => movie.Movie)
                    .ThenInclude(movie => movie.VideoFiles)

            .Include(collection => collection.CollectionMovies)
                .ThenInclude(movie => movie.Movie)
                    .ThenInclude(movie => movie.Media)

            .Include(collection => collection.CollectionMovies)
            .ThenInclude(movie => movie.Movie)
                .ThenInclude(movie => movie.GenreMovies)
                    .ThenInclude(genreMovie => genreMovie.Genre)
            
            .OrderBy(collection => collection.TitleSort);

        var collections = await query
            .Skip(page * take)
            .Take(take)
            .ToListAsync();

        return collections;
    }

}

public record CollectionsResponseItemDto
{
    [JsonProperty("id")] public long Id { get; set; }
    [JsonProperty("backdrop")] public string? Backdrop { get; set; }
    [JsonProperty("favorite")] public bool Favorite { get; set; }
    [JsonProperty("watched")] public bool Watched { get; set; }
    [JsonProperty("logo")] public string? Logo { get; set; }
    [JsonProperty("media_type")] public string MediaType { get; set; }
    [JsonProperty("number_of_items")] public int? NumberOfItems { get; set; }
    [JsonProperty("have_items")] public int? HaveItems { get; set; }
    [JsonProperty("overview")] public string? Overview { get; set; }

    [JsonProperty("color_palette")] public IColorPalettes? ColorPalette { get; set; }

    [JsonProperty("poster")] public string? Poster { get; set; }
    [JsonProperty("title")] public string Title { get; set; }
    [JsonProperty("titleSort")] public string? TitleSort { get; set; }
    [JsonProperty("type")] public string Type { get; set; }
    [JsonProperty("year")] public int? Year { get; set; }
    [JsonProperty("genres")] public GenreDto[]? Genres { get; set; }
    [JsonProperty("videoId")] public string? VideoId { get; set; }
    [JsonProperty("videos")] public VideoDto[]? Videos { get; set; } = [];

    public CollectionsResponseItemDto(CollectionMovie collectionMovie)
    {
        var title = collectionMovie.Movie.Translations.FirstOrDefault()?.Title;
        var overview = collectionMovie.Movie.Translations.FirstOrDefault()?.Overview;

        Id = collectionMovie.Movie.Id;
        Title = !string.IsNullOrEmpty(title)
            ? title
            : collectionMovie.Movie.Title;
        Overview = !string.IsNullOrEmpty(overview)
            ? overview
            : collectionMovie.Movie.Overview;

        Backdrop = collectionMovie.Movie.Backdrop;
        Logo = collectionMovie.Movie.Images
            .FirstOrDefault(media => media.Type == "logo")
            ?.FilePath;
        MediaType = "collectionMovie";
        Year = collectionMovie.Movie.ReleaseDate.ParseYear();
        ColorPalette = collectionMovie.Movie.ColorPalette;
        Poster = collectionMovie.Movie.Poster;
        TitleSort = collectionMovie.Movie.Title.TitleSort(collectionMovie.Movie.ReleaseDate);
        Type = "collectionMovie";
        Genres = collectionMovie.Movie.GenreMovies
            .Select(genreMovie => new GenreDto(genreMovie))
            .ToArray();
        VideoId = collectionMovie.Movie.Video;
        Videos = collectionMovie.Movie.Media
            .Where(media => media.Site == "YouTube")
            .Select(media => new VideoDto(media))
            .ToArray();
    }

    public CollectionsResponseItemDto(Collection collection)
    {
        var title = collection.Translations.FirstOrDefault()?.Title;
        var overview = collection.Translations.FirstOrDefault()?.Overview;

        Id = collection.Id;
        Title = !string.IsNullOrEmpty(title)
            ? title
            : collection.Title;
        Overview = !string.IsNullOrEmpty(overview)
            ? overview
            : collection.Overview;
        Backdrop = collection.Backdrop;
        Logo = collection.Images
            .FirstOrDefault(media => media.Type == "logo")?.FilePath;

        Year = collection.CollectionMovies
            .MinBy(collectionMovie => collectionMovie.Movie.ReleaseDate)
            ?.Movie.ReleaseDate.ParseYear();

        ColorPalette = collection.ColorPalette;
        Poster = collection.Poster;
        TitleSort = collection.Title
            .TitleSort(collection.CollectionMovies
                .MinBy(collectionMovie => collectionMovie.Movie.ReleaseDate)
                ?.Movie.ReleaseDate.ParseYear());

        MediaType = "collection";
        Type = "collection";

        NumberOfItems = collection.Parts;
        HaveItems = collection.CollectionMovies
            .Count(collectionMovie => collectionMovie.Movie.VideoFiles.Any(v => v.Folder != null));

        Genres = collection.CollectionMovies
            .Select(genreTv => genreTv.Movie)
            .SelectMany(movie => movie.GenreMovies
                .Select(genreMovie => genreMovie.Genre))
            .Select(genre => new GenreDto(genre))
            .ToArray();

        VideoId = collection.CollectionMovies
            .FirstOrDefault()
            ?.Movie.Video;
    }
}