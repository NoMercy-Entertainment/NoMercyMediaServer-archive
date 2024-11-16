using System.Collections;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NoMercy.Api.Controllers.V1.Dashboard.DTO;
using NoMercy.Api.Controllers.V1.Media.DTO;
using NoMercy.Data.Repositories;
using NoMercy.Database.Models;
using NoMercy.Networking;

namespace NoMercy.Api.Controllers.V1.Media;

[ApiController]
[Tags("Media Libraries")]
[ApiVersion(1.0)]
[Authorize]
[Route("api/v{version:apiVersion}/libraries")]
public class LibrariesController(
    ILibraryRepository libraryRepository,
    ICollectionRepository collectionRepository,
    ISpecialRepository specialRepository)
    : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Libraries()
    {
        Guid userId = User.UserId();
        if (!User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view libraries");

        IQueryable<Library> libraries = libraryRepository.GetLibraries(userId);

        List<LibrariesResponseItemDto> response = libraries
            .Select(library => new LibrariesResponseItemDto(library))
            .ToList();

        return Ok(new LibrariesDto
        {
            Data = response.OrderBy(library => library.Order)
        });
    }

    [HttpGet]
    [Route("mobile")]
    public async Task<IActionResult> Mobile()
    {
        Guid userId = User.UserId();
        if (!User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view libraries");

        string language = Language();

        IQueryable<Library> libraries = libraryRepository.GetLibraries(userId);

        List<GenreRowDto<dynamic>> list = [];

        foreach (Library library in libraries)
        {
            IEnumerable<Movie> movies = libraryRepository.GetLibraryMovies(userId, library.Id, language, 10, 1, m => m.CreatedAt, "desc");
            IEnumerable<Tv> shows = libraryRepository.GetLibraryShows(userId, library.Id, language, 10, 1, m => m.CreatedAt, "desc");

            list.Add(new GenreRowDto<dynamic>
            {
                Title = library.Title,
                MoreLink = $"/libraries/{library.Id}",
                Items = movies.Select(movie => new LibraryResponseItemDto(movie))
                    .Concat(shows.Select(tv => new LibraryResponseItemDto(tv)))
            });
        }

        IEnumerable<Collection> collections = collectionRepository.GetCollectionItems(userId, language, 10, 1, m => m.CreatedAt, "desc");
        IEnumerable<Special> specials = specialRepository.GetSpecialItems(userId, language, 10, 1, m => m.CreatedAt, "desc");

        list.Add(new GenreRowDto<dynamic>
        {
            Title = "Collections",
            MoreLink = "/collection",
            Items = collections.Select(collection => new LibraryResponseItemDto(collection))
        });

        list.Add(new GenreRowDto<dynamic>
        {
            Title = "Specials",
            MoreLink = "/specials",
            Items = specials.Select(special => new LibraryResponseItemDto(special))
        });

        return Ok(new HomeResponseDto<dynamic>
        {
            Data = list
        });
    }

    [HttpGet]
    [Route("{libraryId:ulid}")]
    public async Task<IActionResult> Library(Ulid libraryId, [FromQuery] PageRequestDto request)
    {
        Guid userId = User.UserId();
        if (!User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view library");

        string language = Language();

        IEnumerable<Movie> movies = libraryRepository
            .GetLibraryMovies(userId, libraryId, language, request.Take, request.Page);
        IEnumerable<Tv> shows = libraryRepository
            .GetLibraryShows(userId, libraryId, language, request.Take, request.Page);

        if (request.Version != "lolomo")
        {
            IOrderedEnumerable<LibraryResponseItemDto> concat = movies
                .Select(movie => new LibraryResponseItemDto(movie))
                .Concat(shows.Select(tv => new LibraryResponseItemDto(tv)))
                .OrderBy(item => item.TitleSort);

            return GetPaginatedResponse(concat, request);
        }

        string[] numbers = ["*", "#", "'", "\"", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0"];
        string[] letters =
        [
            "#", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T",
            "U", "V", "W", "X", "Y", "Z"
        ];

        return Ok(new LoloMoResponseDto<LibraryResponseItemDto>
        {
            Data = letters.Select(genre => new LoloMoRowDto<LibraryResponseItemDto>
            {
                Title = genre,
                Id = genre,
                Items = movies.Select(movie => new LibraryResponseItemDto(movie))
                    .Where(item =>
                        genre == "#" ? numbers.Any(p => item.Title.StartsWith(p)) : item.Title.StartsWith(genre))
                    .Concat(shows.Select(tv => new LibraryResponseItemDto(tv))
                        .Where(item =>
                            genre == "#" ? numbers.Any(p => item.Title.StartsWith(p)) : item.Title.StartsWith(genre)))
            })
        });
    }
}
