using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoMercy.Database;
using NoMercy.Networking;
using NoMercy.Server.app.Http.Controllers.Api.V1.Dashboard.DTO;
using NoMercy.Server.app.Http.Controllers.Api.V1.Media.DTO;

namespace NoMercy.Server.app.Http.Controllers.Api.V1.Media;

[ApiController]
[Tags("Media Libraries")]
[ApiVersion("1")]
[Authorize]
[Route("api/v{Version:apiVersion}/libraries")]
public class LibrariesController: BaseController
{
    [HttpGet]
    public async Task<IActionResult> Libraries()
    {
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view libraries");
        
        List<LibrariesResponseItemDto> libraries = [];

        await using MediaContext mediaContext = new();
        await foreach (var library in LibrariesDto.GetLibraries(mediaContext, userId))
        {
            if (library is null) continue;
            libraries.Add(new LibrariesResponseItemDto(library));
        }

        return Ok(new LibrariesDto
        {
            Data = libraries.OrderBy(library => library.Order)
        });
    }

    [HttpGet]
    [Route("{libraryId}")]
    public async Task<IActionResult> Library(Ulid libraryId, [FromQuery] PageRequestDto request)
    {
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view library");

        var language = Language();
        
        var movies = await LibraryResponseDto.GetLibraryMovies(userId, libraryId,
            language, request.Take + 1, request.Page);

        var shows = await LibraryResponseDto.GetLibraryShows(userId, libraryId,
            language, request.Take + 1, request.Page);

        if (request.Version != "lolomo")
        {
            IEnumerable<LibraryResponseItemDto> concat = movies
                .Select(movie => new LibraryResponseItemDto(movie))
                .Concat(shows.Select(movie => new LibraryResponseItemDto(movie)))
                .OrderBy(libraryResponseDto => libraryResponseDto.TitleSort);
            
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
                    .Where(libraryMovie => genre == "#"
                        ? numbers.Any(p => libraryMovie.Title.StartsWith(p))
                        : libraryMovie.Title.StartsWith(genre))
                    .Concat(shows.Select(movie => new LibraryResponseItemDto(movie))
                        .Where(libraryTv => genre == "#"
                            ? numbers.Any(p => libraryTv.Title.StartsWith(p))
                            : libraryTv.Title.StartsWith(genre)))
            })
        });
    }

}
