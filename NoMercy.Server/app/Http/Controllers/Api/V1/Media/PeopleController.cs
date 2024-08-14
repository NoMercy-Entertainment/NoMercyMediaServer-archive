using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NoMercy.Database;
using NoMercy.Helpers;
using NoMercy.Networking;
using NoMercy.Providers.TMDB.Client;
using NoMercy.Server.app.Helper;
using NoMercy.Server.app.Http.Controllers.Api.V1.DTO;
using NoMercy.Server.app.Http.Controllers.Api.V1.Media.DTO;
using NoMercy.Server.app.Http.Middleware;

namespace NoMercy.Server.app.Http.Controllers.Api.V1.Media;

[ApiController]
[Tags("Media People")]
[ApiVersion("1")]
[Authorize]
public class PeopleController: BaseController
{
    [HttpGet]
    [Route("api/v{Version:apiVersion}/person")] // match themoviedb.org API
    public async Task<IActionResult> Index([FromQuery] PageRequestDto request)
    {
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view people");
        
        var language = Language();

        await using MediaContext mediaContext = new();
        
        var people = await PeopleResponseDto
            .GetPeople(userId, language, request.Take + 1, request.Page);
        
        if (people.Count == 0)
            return NotFoundResponse("People not found");
        
        return GetPaginatedResponse(people, request);
    }

    [HttpGet]
    [Route("/api/v{Version:apiVersion}/person/{id:int}")] // match themoviedb.org API
    public async Task<IActionResult> Show(int id)
    {
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view a person");
        
        var country = Country();
        
        TmdbPersonClient tmdbPersonClient = new(id);
        var personAppends = await tmdbPersonClient.WithAllAppends(true);

        if (personAppends is null)
            return NotFoundResponse("Person not found");

        return Ok(new PersonResponseDto
        {
            Data = new PersonResponseItemDto(personAppends, country)
        });
    }
}

public class PageRequestDto
{
    [FromQuery(Name = "page")] public int Page { get; set; } = 0;
    [FromQuery(Name = "take")] public int Take { get; set; } = 10;
    [FromQuery(Name = "version")] public string? Version { get; set; }
}