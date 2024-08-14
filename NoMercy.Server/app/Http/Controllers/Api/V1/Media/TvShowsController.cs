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
[Tags("Media TV Shows")]
[ApiVersion("1")]
[Authorize]
[Route("api/v{Version:apiVersion}/tv/{id:int}")] // match themoviedb.org API
public class TvShowsController: BaseController
{
    [HttpGet]
    public async Task<IActionResult> Tv(int id)
    {
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view tv shows");

        var language = Language();
        
        await using MediaContext mediaContext = new();
        var tv = await InfoResponseDto.GetTv(mediaContext, userId, id,
            language);

        if (tv is not null)
            return Ok(new InfoResponseDto
            {
                Data = new InfoResponseItemDto(tv,language)
            });

        TmdbTvClient tmdbTvClient = new(id);
        var tvShowAppends = await tmdbTvClient.WithAllAppends(true);

        if (tvShowAppends is null)
            return NotFoundResponse("Tv show not found");

        TmdbShowJob tmdbShowJob = new(id);
        JobDispatcher.Dispatch(tmdbShowJob, "queue", 10);

        return Ok(new InfoResponseDto
        {
            Data = new InfoResponseItemDto(tvShowAppends,language)
        });
    }

    [HttpGet]
    [Route("available")]
    public async Task<IActionResult> Available(int id)
    {
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view tv shows");

        await using MediaContext mediaContext = new();
        var tv = await InfoResponseDto.GetTvAvailable(mediaContext, userId, id);

        var available = tv?.Episodes.Any(episode => episode.VideoFiles.Count > 0) ?? false;
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
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view tv shows");
        
        var language = Language();

        await using MediaContext mediaContext = new();
        var tv = await InfoResponseDto.GetTvPlaylist(mediaContext, userId, id, language);
        
        if (tv is null)
            return NotFoundResponse("Tv show not found");

        var episodes = tv.Seasons
            .Where(season => season.SeasonNumber > 0)
            .SelectMany(season => season.Episodes)
            .Select(episode => new PlaylistResponseDto(episode))
            .ToArray();

        var extras = tv.Seasons
            .Where(season => season.SeasonNumber == 0)
            .SelectMany(season => season.Episodes)
            .Select(episode => new PlaylistResponseDto(episode))
            .ToArray();

        return Ok(episodes.Concat(extras).ToArray());
    }

    [HttpPost]
    [Route("like")]
    public async Task<IActionResult> Like(int id, [FromBody] LikeRequestDto request)
    {
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to like tv shows");

        await using MediaContext mediaContext = new();
        var tv = await mediaContext.Tvs
            .AsNoTracking()
            .Where(tv => tv.Id == id)
            .FirstOrDefaultAsync();

        if (tv is null)
            return UnprocessableEntityResponse("Tv show not found");

        if (request.Value)
        {
            await mediaContext.TvUser.Upsert(new TvUser(tv.Id, userId))
                .On(m => new { m.TvId, m.UserId })
                .WhenMatched(m => new TvUser
                {
                    TvId = m.TvId,
                    UserId = m.UserId
                })
                .RunAsync();
        }
        else
        {
            var tvUser = await mediaContext.TvUser
                .Where(tvUser => tvUser.TvId == tv.Id && tvUser.UserId == userId)
                .FirstOrDefaultAsync();

            if (tvUser is not null) 
                mediaContext.TvUser.Remove(tvUser);

            await mediaContext.SaveChangesAsync();
        }

        return Ok(new StatusResponseDto<string>
        {
            Status = "ok",
            Message = "{0} {1}",
            Args = new object[]
            {
                tv.Title,
                request.Value ? "liked" : "unliked"
            }
        });
    }

    [HttpPost]
    [Route("rescan")]
    public async Task<IActionResult> Rescan(int id)
    {
        if (!HttpContext.User.IsModerator())
            return UnauthorizedResponse("You do not have permission to rescan tv shows");
        
        await using MediaContext mediaContext = new();
        var tv = await mediaContext.Tvs
            .AsNoTracking()
            .Where(tv => tv.Id == id)
            .Include(tv => tv.Library)
            .FirstOrDefaultAsync();

        if (tv is null)
            return UnprocessableEntityResponse("Tv show not found");

        try
        {
            FindMediaFilesJob findMediaFilesJob = new(tv.Id, tv.Library);
            JobDispatcher.Dispatch(findMediaFilesJob, "queue", 6);
        }
        catch (Exception e)
        {
            Logger.Encoder(e, LogEventLevel.Error);
            return InternalServerErrorResponse(e.Message);
        }

        return Ok(new StatusResponseDto<string>
        {
            Status = "ok",
            Message = "Rescanning {0} for files",
            Args = new object[]
            {
                tv.Title
            }
        });
    }

    [HttpPost]
    [Route("refresh")]
    public async Task<IActionResult> Refresh(int id)
    {
        if (!HttpContext.User.IsModerator())
            return UnauthorizedResponse("You do not have permission to refresh tv shows");
        
        await using MediaContext mediaContext = new();
        var tv = await mediaContext.Tvs
            .AsNoTracking()
            .Where(tv => tv.Id == id)
            .Include(tv => tv.Library)
            .FirstOrDefaultAsync();

        if (tv is null)
            return UnprocessableEntityResponse("Tv show not found");

        TmdbShowJob tmdbShowJob = new(id);
        JobDispatcher.Dispatch(tmdbShowJob, "queue", 10);

        return Ok(new StatusResponseDto<string>
        {
            Status = "ok",
            Message = "Rescanning {0} for files",
            Args = new object[]
            {
                tv.Title
            }
        });
    }
}