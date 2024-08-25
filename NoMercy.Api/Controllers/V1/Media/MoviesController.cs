using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoMercy.Api.Controllers.V1.DTO;
using NoMercy.Api.Controllers.V1.Media.DTO;
using NoMercy.Data.Repositories;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.Networking;
using NoMercy.NmSystem;
using NoMercy.Providers.TMDB.Client;
using NoMercy.Providers.TMDB.Models.Movies;
using Serilog.Events;

namespace NoMercy.Api.Controllers.V1.Media;

[ApiController]
[Tags(tags: "Media Movies")]
[ApiVersion("1")]
[Authorize]
[Route("api/v{Version:apiVersion}/movie/{id:int}")] // match themoviedb.org API
public class MoviesController : BaseController
{
    private readonly IMovieRepository _movieRepository;

    public MoviesController(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Movie(int id)
    {
        Guid userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view movies");

        string language = Language();
        string country = Country();

        Movie? movie = await _movieRepository.GetMovieAsync(userId, id, language);

        if (movie is not null)
            return Ok(new InfoResponseDto
            {
                Data = new InfoResponseItemDto(movie, country)
            });

        TmdbMovieClient tmdbMovieClient = new(id);
        TmdbMovieAppends? movieAppends = await tmdbMovieClient.WithAllAppends(true);

        if (movieAppends is null)
            return NotFoundResponse("Movie not found");
        
        await _movieRepository.AddMovieAsync(id);
        
        return Ok(new InfoResponseDto
        {
            Data = new InfoResponseItemDto(movieAppends, country)
        });
    }

    [HttpGet]
    [Route("available")]
    public async Task<IActionResult> Available(int id)
    {
        Guid userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view movies");

        bool available = await _movieRepository.GetMovieAvailableAsync(userId, id);

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
    [Route("watch")]
    public async Task<IActionResult> Watch(int id)
    {
        Guid userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view movies");

        string language = Language();

        IEnumerable<PlaylistResponseDto> playlist = _movieRepository.GetMoviePlaylistAsync(userId, id, language)
            .Select(movie => new PlaylistResponseDto(movie));

        if (!playlist.Any())
            return NotFoundResponse("Movie not found");

        return Ok(playlist);
    }

    [HttpPost]
    [Route("like")]
    public async Task<IActionResult> Like(int id, [FromBody] LikeRequestDto request)
    {
        Guid userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to like movies");

        bool success = await _movieRepository.LikeMovieAsync(id, userId, request.Value);

        if (!success)
            return UnprocessableEntityResponse("Movie not found");

        return Ok(new StatusResponseDto<string>
        {
            Status = "ok",
            Message = "{0}: {1}",
            Args = new object[]
            {
                request.Value ? "liked" : "unliked"
            }
        });
    }

    [HttpPost]
    [Route("rescan")]
    public async Task<IActionResult> Like(int id)
    {
        if (!HttpContext.User.IsModerator())
            return UnauthorizedResponse("You do not have permission to rescan movies");

        await using MediaContext mediaContext = new();
        Movie[] movies = await mediaContext.Movies
            .AsNoTracking()
            .Where(movie => movie.Id == id)
            .Include(movie => movie.Library)
            .ToArrayAsync();

        if (movies.Length == 0)
            return UnprocessableEntityResponse("Movie not found");

        foreach (Movie movie in movies)
            try
            {
                // FindMediaFilesJob findMediaFilesJob = new(movie.Id, movie.Library);
                // jobDispatcher.Dispatch(findMediaFilesJob, "queue", 6);
            }
            catch (Exception e)
            {
                Logger.MovieDb(e, LogEventLevel.Error);
            }

        return Ok(new StatusResponseDto<string>
        {
            Status = "ok",
            Message = "Rescanning {0} for files",
            Args = new object[]
            {
                movies[0].Title
            }
        });
    }

    [HttpPost]
    [Route("refresh")]
    public async Task<IActionResult> Refresh(int id)
    {
        if (!HttpContext.User.IsModerator())
            return UnauthorizedResponse("You do not have permission to refresh movies");

        await using MediaContext mediaContext = new();
        Movie[] movies = await mediaContext.Movies
            .AsNoTracking()
            .Where(movie => movie.Id == id)
            .Include(movie => movie.Library)
            .ToArrayAsync();

        if (movies.Length == 0)
            return UnprocessableEntityResponse("Movie not found");

        try
        {
            // TmdbMovieJob tmdbMovieJob = new(id);
            // jobDispatcher.Dispatch(tmdbMovieJob, "queue", 10);
        }
        catch (Exception e)
        {
            Logger.Encoder(e, LogEventLevel.Error);
            return InternalServerErrorResponse(e.Message);
        }

        return Ok(new StatusResponseDto<string>
        {
            Status = "ok",
            Message = "Refreshing {0}",
            Args = new object[]
            {
                movies[0].Title
            }
        });
    }
}