using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.Helpers;
using NoMercy.Networking;
using NoMercy.Server.app.Helper;
using NoMercy.Server.app.Http.Controllers.Api.V1.DTO;
using NoMercy.Server.app.Http.Controllers.Api.V1.Media.DTO;
using NoMercy.Server.app.Http.Middleware;

namespace NoMercy.Server.app.Http.Controllers.Api.V1.Media;

[ApiController]
[Tags("Media Specials")]
[ApiVersion("1")]
[Authorize]
[Route("api/v{Version:apiVersion}/specials")]
public class SpecialController: BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] PageRequestDto request)
    {
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view specials");
        
        List<SpecialsResponseItemDto> specials = [];
        
        var language = Language();
        
        await using MediaContext mediaContext = new();
        await foreach (var special in SpecialsResponseDto.GetSpecials(mediaContext, userId, language))
        {
            specials.Add(special);
        }
        
        if (specials.Count == 0)
            return NotFoundResponse("Specials not found");

        if (request.Version != "lolomo")
            return Ok(new SpecialsResponseDto
            {
                Data = specials
                    .OrderBy(libraryResponseDto => libraryResponseDto.TitleSort)
            });

        string[] numbers = ["*", "#", "'", "\"", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0"];
        string[] letters =
        [
            "#", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N",
            "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"
        ];

        return Ok(new LoloMoResponseDto<SpecialsResponseItemDto>
        {
            Data = letters.Select(genre => new LoloMoRowDto<SpecialsResponseItemDto>
            {
                Title = genre,
                Id = genre,

                Items = specials.Where(special => genre == "#"
                        ? numbers.Any(p => special.Title.StartsWith(p))
                        : special.Title.StartsWith(genre))
                    .OrderBy(libraryResponseDto => libraryResponseDto.TitleSort)
            })
        });
    }

    [HttpGet]
    [Route("{id:ulid}")]
    public async Task<IActionResult> Show(Ulid id)
    {
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view a special");
        
        var language = Language();
        var country = Country();

        await using MediaContext mediaContext = new();
        var special = await SpecialResponseDto.GetSpecial(mediaContext, userId, id, language, country);
        
        if (special is null)
            return NotFoundResponse("Special not found");

        var movieIds = special.Items
            .Where(item => item.MovieId is not null)
            .Select(item => item.MovieId ?? 0);

        var episodeIds = special.Items
            .Where(item => item.EpisodeId is not null)
            .Select(item => item.EpisodeId ?? 0);

        List<SpecialItemsDto> items = [];
        await foreach (var movie in SpecialResponseDto.GetSpecialMovies(mediaContext, userId, movieIds, language, country))
            items.Add(new SpecialItemsDto(movie));

        await foreach (var tv in SpecialResponseDto.GetSpecialTvs(mediaContext, userId, episodeIds, language, country))
            items.Add(new SpecialItemsDto(tv));

        return Ok(new DataResponseDto<SpecialResponseItemDto>
        {
            Data = new SpecialResponseItemDto(special, items)
        });
    }

    [HttpGet]
    [Route("{id:ulid}/available")]
    public async Task<IActionResult> Available(Ulid id)
    {
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view a special");

        await using MediaContext mediaContext = new();
        var special = await SpecialResponseDto
            .GetSpecialAvailable(mediaContext, userId, id);

        var hasFiles = special is not null && (
           special.Items
               .Select(movie => movie.Movie?.VideoFiles)
               .Any()
           || special.Items
               .Select(movie => movie.Episode?.VideoFiles)
               .Any()
       );

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
    [Route("{id:ulid}/watch")]
    public async Task<IActionResult> Watch(Ulid id)
    {
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view a special");
        
        var language = Language();
        
        await using MediaContext mediaContext = new();

        var special = await SpecialResponseDto
            .GetSpecialPlaylist(mediaContext, userId, id, language);
        
        if (special is null)
            return NotFoundResponse("Special not found");

        var items = special.Items
            .OrderBy(item => item.Order)
            .Select((item, index) => item.EpisodeId is not null
                ? new PlaylistResponseDto(item.Episode ?? new Episode(), index)
                : new PlaylistResponseDto(item.Movie ?? new Movie(), index)
            )
            .ToArray();
        
        if (items.Length == 0)
            return NotFoundResponse("Special not found");

        return Ok(items);
    }

    [HttpPost]
    [Route("{id:ulid}/like")]
    public async Task<IActionResult> Like(Ulid id, [FromBody] LikeRequestDto request)
    {
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to like a special");

        await using MediaContext mediaContext = new();
        var collection = await mediaContext.Specials
            .AsNoTracking()
            .Where(collection => collection.Id == id)
            .FirstOrDefaultAsync();

        if (collection is null)
            return NotFoundResponse("Special not found");

        if (request.Value)
        {
            await mediaContext.SpecialUser.Upsert(new SpecialUser(collection.Id, userId))
                .On(m => new { m.SpecialId, m.UserId })
                .WhenMatched(m => new SpecialUser
                {
                    SpecialId = m.SpecialId,
                    UserId = m.UserId
                })
                .RunAsync();
        }
        else
        {
            var collectionUser = await mediaContext.SpecialUser
                .Where(collectionUser => collectionUser.SpecialId == collection.Id && collectionUser.UserId == userId)
                .FirstOrDefaultAsync();

            if (collectionUser is not null) mediaContext.SpecialUser.Remove(collectionUser);

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