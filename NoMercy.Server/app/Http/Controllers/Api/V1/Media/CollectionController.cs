using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.Helpers;
using NoMercy.Networking;
using NoMercy.Providers.TMDB.Client;
using NoMercy.Server.app.Helper;
using NoMercy.Server.app.Http.Controllers.Api.V1.DTO;
using NoMercy.Server.app.Http.Controllers.Api.V1.Media.DTO;
using NoMercy.Server.app.Http.Middleware;
using NoMercy.Server.app.Jobs;
using NoMercy.Server.system;
using Collection = NoMercy.Database.Models.Collection;

namespace NoMercy.Server.app.Http.Controllers.Api.V1.Media;

[ApiController]
[Tags("Media Collections")]
[ApiVersion("1")]
[Authorize]
[Route("api/v{Version:apiVersion}/collection")] // match themoviedb.org API
public class CollectionsController: BaseController
{
    [HttpGet]
    public async Task<IActionResult> Collections([FromQuery] PageRequestDto request)
    {
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view collections");
        
        var language = Language();
        
        var collections = await CollectionsResponseDto.GetCollections(userId, language, request.Take  + 1, request.Page);

        if (request.Version != "lolomo"){
            
            var concat = collections
                .Select(collection => new CollectionsResponseItemDto(collection));
            
            return GetPaginatedResponse(concat, request);
            
        }

        string[] numbers = ["*", "#", "'", "\"", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0"];
        string[] letters =
        [
            "#", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N",
            "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"
        ];

        return Ok(new LoloMoResponseDto<LibraryResponseItemDto>
        {
            Data = letters.Select(genre => new LoloMoRowDto<LibraryResponseItemDto>
            {
                Title = genre,
                Id = genre,

                Items = collections.Where(collection => genre == "#"
                        ? numbers.Any(p => collection.Title.StartsWith(p))
                        : collection.Title.StartsWith(genre))
                    .Select(collection => new LibraryResponseItemDto(collection))
            })
        });
    }

    [HttpGet]
    [Route("{id:int}")]
    public async Task<IActionResult> Collection(int id)
    {
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view collections");
        
        var language = Language();
        var country = Country();

        await using MediaContext mediaContext = new();
        var collection = await CollectionResponseDto.GetCollection(mediaContext, userId, id, language,
           language ?? string.Empty);

        if (collection is not null && collection.CollectionMovies.Count > 0 && collection.Images.Count > 0)
            return Ok(new CollectionResponseDto
            {
                Data = new CollectionResponseItemDto(collection, country)
            });

        TmdbCollectionClient tmdbCollectionsClient = new(id);
        var collectionAppends = await tmdbCollectionsClient.WithAllAppends(true);

        if (collectionAppends is null)
            return NotFound(new CollectionResponseDto
            {
                Data = null
            });

        var library = await mediaContext.Libraries
            .Where(f => f.Type == "movie")
            .Include(l => l.FolderLibraries)
            .ThenInclude(fl => fl.Folder)
            .FirstOrDefaultAsync();

        TmdbCollectionJob tmdbJob = new(collectionAppends.Id, library);
        JobDispatcher.Dispatch(tmdbJob, "queue", 10);

        return Ok(new CollectionResponseDto
        {
            Data = new CollectionResponseItemDto(collectionAppends)
        });
    }

    [HttpGet]
    [Route("{id:int}/available")]
    public async Task<IActionResult> Available(int id)
    {
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view collections");

        await using MediaContext mediaContext = new();
        var collection = await mediaContext.Collections
            .AsNoTracking()
            .Where(collection => collection.Id == id)
            .Where(collection => collection.Library.LibraryUsers
                .FirstOrDefault(u => u.UserId == userId) != null)
            .Include(movie => movie.CollectionMovies)
            .Where(collectionMovie => collectionMovie.CollectionMovies
                .Any(movie => movie.Movie.VideoFiles.Any()))
            .Include(movie => movie.CollectionMovies)
            .ThenInclude(collectionMovie => collectionMovie.Movie)
            .ThenInclude(movie => movie.VideoFiles)
            .ThenInclude(file => file.UserData
                .Where(movieUser => movieUser.UserId == userId))
            .FirstOrDefaultAsync();
        
        var available = collection is not null && collection.CollectionMovies
            .Select(movie => movie.Movie.VideoFiles)
            .Any();
        
        if (!available)
            return NotFound(new AvailableResponseDto
            {
                Available = false
            });

        return Ok(new AvailableResponseDto
        {
            Available = true
        });
    }

    [HttpGet]
    [Route("{id:int}/watch")]
    public async Task<IActionResult> Watch(int id)
    {
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view collections");
        
        var language = Language();
        
        await using MediaContext mediaContext = new();
        var collection = await mediaContext.Collections
            .AsNoTracking()
            .Where(collection => collection.Id == id)
            .Include(collection =>
                collection.CollectionMovies.OrderBy(collectionMovie => collectionMovie.Movie.ReleaseDate))
                .ThenInclude(collectionMovie => collectionMovie.Movie)
                    .ThenInclude(movie => movie.Library.LibraryUsers)
                    .Where(collection => collection.Library.LibraryUsers
                        .FirstOrDefault(libraryUser => libraryUser.UserId == userId) != null)
            
            .Include(collection => collection.CollectionMovies)
                .ThenInclude(collectionMovie => collectionMovie.Movie)
                    .ThenInclude(movie => movie.Media
                        .Where(media => media.Type == "video"))
            
            .Include(collection => collection.Images
                .Where(image => image.Type == "logo"))
            .Include(collection => collection.Translations
                .Where(translation =>
                    translation.Iso6391 == language))
            
            .Include(collection => collection.CollectionMovies)
                .ThenInclude(collectionMovie => collectionMovie.Movie)
                    .ThenInclude(movie => movie.Images)
            
            .Include(collection => collection.CollectionMovies)
                .ThenInclude(collectionMovie => collectionMovie.Movie)
                    .ThenInclude(movie => movie.Translations
                        .Where(translation => translation.Iso6391 == language)
                    )
            
            .Include(collection => collection.CollectionMovies)
                .ThenInclude(collectionMovie => collectionMovie.Movie)
                    .ThenInclude(movie => movie.VideoFiles)
                        .ThenInclude(file => file.UserData
                            .Where(movieUser => movieUser.UserId == userId))
            
            .FirstOrDefaultAsync();
        
        if (collection is null)
            return NotFoundResponse("Collection not found");

        return Ok(collection.CollectionMovies
                .Select((movie, index) => new PlaylistResponseDto(movie.Movie, index + 1, collection))
                .ToArray());
    }

    [HttpPost]
    [Route("{id:int}/like")]
    public async Task<IActionResult> Like(int id, [FromBody] LikeRequestDto request)
    {
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to like collections");

        await using MediaContext mediaContext = new();
        var collection = await mediaContext.Collections
            .AsNoTracking()
            .Where(collection => collection.Id == id)
            .FirstOrDefaultAsync();

        if (collection is null)
            return UnprocessableEntityResponse("Collection not found");

        if (request.Value)
        {
            await mediaContext.CollectionUser.Upsert(new CollectionUser(collection.Id, userId))
                .On(m => new { m.CollectionId, m.UserId })
                .WhenMatched(m => new CollectionUser
                {
                    CollectionId = m.CollectionId,
                    UserId = m.UserId
                })
                .RunAsync();
        }
        else
        {
            var collectionUser = await mediaContext.CollectionUser
                .Where(collectionUser =>
                    collectionUser.CollectionId == collection.Id && collectionUser.UserId == userId)
                .FirstOrDefaultAsync();

            if (collectionUser is not null) mediaContext.CollectionUser.Remove(collectionUser);

            await mediaContext.SaveChangesAsync();
        }

        return Ok(new StatusResponseDto<string>
        {
            Status = "ok",
            Message = "{0} {1}",
            Args = new object[]
            {
                collection.Title,
                request.Value ? "liked" : "unliked"
            }
        });
    }
}