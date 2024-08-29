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
using NoMercy.Providers.TMDB.Models.TV;
using Serilog.Events;

namespace NoMercy.Api.Controllers.V1.Media;

[ApiController]
[Tags(tags: "Media TV Shows")]
[ApiVersion(1.0)]
[Authorize]
[Route("api/v{version:apiVersion}/tv/{id:int}")] // match themoviedb.org API
public class TvShowsController : BaseController
{
    private readonly ITvShowRepository _tvShowRepository;

    public TvShowsController(ITvShowRepository tvShowRepository)
    {
        _tvShowRepository = tvShowRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Tv(int id)
    {
        Guid userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view tv shows");

        string language = Language();

        Tv? tv = await _tvShowRepository.GetTvAsync(userId, id, language);

        if (tv is not null)
            return Ok(new InfoResponseDto
            {
                Data = new InfoResponseItemDto(tv, language)
            });

        TmdbTvClient tmdbTvClient = new(id);
        TmdbTvShowAppends? tvShowAppends = await tmdbTvClient.WithAllAppends(true);

        if (tvShowAppends is null)
            return NotFoundResponse("Tv show not found");

        await _tvShowRepository.AddTvShowAsync(id);

        return Ok(new InfoResponseDto
        {
            Data = new InfoResponseItemDto(tvShowAppends, language)
        });
    }

    [HttpGet]
    [Route("available")]
    public async Task<IActionResult> Available(int id)
    {
        Guid userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view tv shows");

        bool available = await _tvShowRepository.GetTvAvailableAsync(userId, id);

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
            return UnauthorizedResponse("You do not have permission to view tv shows");

        string language = Language();

        Tv? tv = await _tvShowRepository.GetTvPlaylistAsync(userId, id, language);

        if (tv is null)
            return NotFoundResponse("Tv show not found");

        PlaylistResponseDto[] episodes = tv.Seasons
            .Where(season => season.SeasonNumber > 0)
            .SelectMany(season => season.Episodes)
            .Select(episode => new PlaylistResponseDto(episode))
            .ToArray();

        PlaylistResponseDto[] extras = tv.Seasons
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
        Guid userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to like tv shows");

        bool success = await _tvShowRepository.LikeTvAsync(id, userId, request.Value);

        if (!success)
            return UnprocessableEntityResponse("Tv show not found");

        return Ok(new StatusResponseDto<string>
        {
            Status = "ok",
            Message = "{1}",
            Args = new object[]
            {
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
        Tv? tv = await mediaContext.Tvs
            .AsNoTracking()
            .Where(tv => tv.Id == id)
            .Include(tv => tv.Library)
            .FirstOrDefaultAsync();

        if (tv is null)
            return UnprocessableEntityResponse("Tv show not found");

        try
        {
            // FindMediaFilesJob findMediaFilesJob = new(tv.Id, tv.Library);
            // jobDispatcher.Dispatch(findMediaFilesJob, "queue", 6);
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
        Tv? tv = await mediaContext.Tvs
            .AsNoTracking()
            .Where(tv => tv.Id == id)
            .Include(tv => tv.Library)
            .FirstOrDefaultAsync();

        if (tv is null)
            return UnprocessableEntityResponse("Tv show not found");

        // TmdbShowJob tmdbShowJob = new(id);
        // jobDispatcher.Dispatch(tmdbShowJob, "queue", 10);

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