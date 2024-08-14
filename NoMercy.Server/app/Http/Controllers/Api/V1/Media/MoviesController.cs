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
using Serilog.Events;

namespace NoMercy.Server.app.Http.Controllers.Api.V1.Media;

[ApiController]
[Tags("Media Movies")]
[ApiVersion("1")]
[Authorize]
[Route("api/v{Version:apiVersion}/movie/{id:int}")] // match themoviedb.org API
public class MoviesController: BaseController
{
    [HttpGet]
    public async Task<IActionResult> Movie(int id)
    {
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view movies");
        
        var language = Language();
        var country = Country();

        await using MediaContext mediaContext = new();
        var movie = await InfoResponseDto.GetMovie(mediaContext, userId, id, language);

        if (movie is not null)
            return Ok(new InfoResponseDto
            {
                Data = new InfoResponseItemDto(movie, country)
            });

        TmdbMovieClient tmdbMovieClient = new(id);
        var movieAppends = await tmdbMovieClient.WithAllAppends(true);

        if (movieAppends is null)
            return NotFoundResponse("Movie not found");

        TmdbMovieJob tmdbMovieJob = new(id);
        JobDispatcher.Dispatch(tmdbMovieJob, "queue", 10);

        return Ok(new InfoResponseDto
        {
            Data = new InfoResponseItemDto(movieAppends, country)
        });
    }

    [HttpGet]
    [Route("available")]
    public async Task<IActionResult> Available(int id)
    {
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view movies");

        await using MediaContext mediaContext = new();
        var movie = await InfoResponseDto.GetMovieAvailable(mediaContext, userId, id);
        
        var hasFiles = movie?.VideoFiles.Any() ?? false;
        
        if (!hasFiles)
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
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view movies");

        var language = Language();
        
        await using MediaContext mediaContext = new();
        var movie = await InfoResponseDto.GetMoviePlaylist(mediaContext, userId, id,
                language);

        if (movie is null)
            return NotFoundResponse("Movie not found");

        List<PlaylistResponseDto> playlist =
        [
            new PlaylistResponseDto(movie)
        ];
        
        return Ok(playlist);
    }

    [HttpPost]
    [Route("like")]
    public async Task<IActionResult> Like(int id, [FromBody] LikeRequestDto request)
    {
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to like movies");

        await using MediaContext mediaContext = new();
        var movie = await mediaContext.Movies
            .AsNoTracking()
            .Where(movie => movie.Id == id)
            .FirstOrDefaultAsync();

        if (movie is null)
            return UnprocessableEntityResponse("Movie not found");

        if (request.Value)
        {
            await mediaContext.MovieUser.Upsert(new MovieUser(movie.Id, userId))
                .On(m => new { m.MovieId, m.UserId })
                .WhenMatched(m => new MovieUser
                {
                    MovieId = m.MovieId,
                    UserId = m.UserId
                })
                .RunAsync();
        }
        else
        {
            var movieUser = await mediaContext.MovieUser
                .Where(movieUser => movieUser.MovieId == movie.Id && movieUser.UserId == userId)
                .FirstOrDefaultAsync();

            if (movieUser is not null) mediaContext.MovieUser.Remove(movieUser);

            await mediaContext.SaveChangesAsync();
        }

        return Ok(new StatusResponseDto<string>
        {
            Status = "ok",
            Message = "{0}: {1}",
            Args = new object[]
            {
                movie.Title,
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
        var movies = await mediaContext.Movies
            .AsNoTracking()
            .Where(movie => movie.Id == id)
            .Include(movie => movie.Library)
            .ToArrayAsync();

        if (movies.Length == 0)
            return UnprocessableEntityResponse("Movie not found");

        foreach (var movie in movies)
            try
            {
                var findMediaFilesJob = new FindMediaFilesJob(movie.Id, movie.Library);
                JobDispatcher.Dispatch(findMediaFilesJob, "queue", 6);
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
        var movies = await mediaContext.Movies
            .AsNoTracking()
            .Where(movie => movie.Id == id)
            .Include(movie => movie.Library)
            .ToArrayAsync();

        if (movies.Length == 0)
            return UnprocessableEntityResponse("Movie not found");
        
        try
        {
            TmdbMovieJob tmdbMovieJob = new(id);
            JobDispatcher.Dispatch(tmdbMovieJob, "queue", 10);
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