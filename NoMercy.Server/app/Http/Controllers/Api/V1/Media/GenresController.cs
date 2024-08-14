using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoMercy.Database;
using NoMercy.Helpers;
using NoMercy.Networking;
using NoMercy.Server.app.Helper;
using NoMercy.Server.app.Http.Controllers.Api.V1.DTO;
using NoMercy.Server.app.Http.Controllers.Api.V1.Media.DTO;
using NoMercy.Server.app.Http.Middleware;
using static NoMercy.Server.app.Http.Controllers.Api.V1.Media.DTO.GenresResponseDto;

namespace NoMercy.Server.app.Http.Controllers.Api.V1.Media;

[ApiController]
[Tags("Media Genres")]
[ApiVersion("1")]
[Authorize]
[Route("api/v{Version:apiVersion}/genres")]
public class GenresController: BaseController
{
    [HttpGet]
    public async Task<IActionResult> Genres()
    {
        List<GenresResponseItemDto> genres = [];
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view genres");
        
        var language = Language();

        await using MediaContext mediaContext = new();
        await foreach (var genre in GetGenres(mediaContext, userId, language))
            genres.Add(new GenresResponseItemDto(genre));

        return Ok(new GenresResponseDto
        {
            Data = genres.OrderBy(genre => genre.Title)
        });
    }

    [HttpGet]
    [Route("{genreId}")]
    public async Task<IActionResult> Genre(int genreId, [FromQuery] PageRequestDto request)
    {
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view genres");

        var language = Language();
        
        await using MediaContext mediaContext = new();
        var genre = await GenreResponseDto.GetGenre(mediaContext, userId, genreId, language, request.Take  + 1, request.Page);
        
        if (genre.GenreTvShows.Count == 0 && genre.GenreMovies.Count == 0)
            return NotFoundResponse("Genre not found");

        if (request.Version != "lolomo")
            return Ok(new GenreResponseDto
            {
                Data = genre.GenreMovies.Take(request.Take)
                    .Select(movie => new GenreResponseItemDto(movie))
                    .Concat(genre.GenreTvShows.Take(request.Take)
                        .Select(tv => new GenreResponseItemDto(tv)))
                    .OrderBy(libraryResponseDto => libraryResponseDto.TitleSort),

                NextId = genre.GenreMovies.Count + genre.GenreTvShows.Count < request.Take
                    ? null
                    : genre.GenreMovies.Count + genre.GenreTvShows.Count + request.Page * request.Take
            });

        string[] numbers = ["*", "#", "'", "\"", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0"];
        string[] letters =
        [
            "#", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T",
            "U", "V", "W", "X", "Y", "Z"
        ];

        return Ok(new LoloMoResponseDto<GenreResponseItemDto>
        {
            Data = letters.Select(g => new LoloMoRowDto<GenreResponseItemDto>
            {
                Title = g,
                Id = g,
                Items = genre.GenreMovies.Take(request.Take)
                    .Where(libraryMovie => g == "#"
                        ? numbers.Any(p => libraryMovie.Movie.Title.StartsWith(p))
                        : libraryMovie.Movie.Title.StartsWith(g))
                    .Select(movie => new GenreResponseItemDto(movie))
                    .Concat(genre.GenreTvShows.Take(request.Take)
                        .Where(libraryTv => g == "#"
                            ? numbers.Any(p => libraryTv.Tv.Title.StartsWith(p))
                            : libraryTv.Tv.Title.StartsWith(g))
                        .Select(tv => new GenreResponseItemDto(tv)))
                    .OrderBy(libraryResponseDto => libraryResponseDto.TitleSort)
            })
        });
    }
}