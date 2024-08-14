using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoMercy.Helpers;
using NoMercy.Networking;
using NoMercy.Server.app.Helper;
using NoMercy.Server.app.Http.Controllers.Api.V1.DTO;
using NoMercy.Server.app.Http.Middleware;

namespace NoMercy.Server.app.Http.Controllers.Api.V1.Music;

[ApiController]
[ApiVersion("1")]
[Tags("Music Genres")]
[Authorize]
[Route("api/v{Version:apiVersion}/music/genres", Order = 4)]
public class GenresController: BaseController
{
    [HttpGet]
    public IActionResult Index()
    {
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view genres");

        return Ok(new PlaceholderResponse
        {
            Data = []
        });
    }

    [HttpGet]
    [Route("{id:guid}")]
    public IActionResult Show(Guid id)
    {
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view genres");

        return Ok(new PlaceholderResponse
        {
            Data = []
        });
    }
}