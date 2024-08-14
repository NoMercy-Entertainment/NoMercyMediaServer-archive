using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NoMercy.Database;
using NoMercy.Helpers;
using NoMercy.Networking;
using NoMercy.Server.app.Helper;
using NoMercy.Server.app.Http.Controllers.Api.V1.DTO;
using NoMercy.Server.app.Http.Controllers.Api.V1.Music;
using NoMercy.Server.app.Http.Middleware;

namespace NoMercy.Server.app.Http.Controllers.Api.V1.Dashboard;

[ApiController]
[Tags("Dashboard Configuration")]
[ApiVersion("1")]
[Authorize]
[Route("api/v{Version:apiVersion}/dashboard/configuration", Order = 10)]
public class ConfigurationController : BaseController
{
    [HttpGet]
    public IActionResult Index()
    {
        if (!HttpContext.User.IsOwner())
            return UnauthorizedResponse("You do not have permission to view configuration");

        return Ok(new PlaceholderResponse()
        {
            Data = []
        });
    }

    [HttpPost]
    public IActionResult Store()
    {
        if (!HttpContext.User.IsOwner())
            return UnauthorizedResponse("You do not have permission to store configuration");

        return Ok(new PlaceholderResponse
        {
            Data = []
        });
    }

    [HttpPatch]
    public IActionResult Update()
    {
        if (!HttpContext.User.IsOwner())
            return UnauthorizedResponse("You do not have permission to update configuration");

        return Ok(new PlaceholderResponse
        {
            Data = []
        });
    }

    [HttpGet]
    [Route("languages")]
    public async Task<IActionResult> Languages()
    {
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view languages");
        
        await using MediaContext context = new();
        var languages = await context.Languages
            .ToListAsync();

        return Ok(languages.Select(language => new LanguageDto
        {
            Id = language.Id,
            Iso6391 = language.Iso6391,
            EnglishName = language.EnglishName,
            Name = language.Name
        }).ToList());
    }

    [HttpGet]
    [Route("countries")]
    public async Task<IActionResult> Countries()
    {
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view countries");
        
        await using MediaContext context = new();
        var countries = await context.Countries
            .ToListAsync();

        return Ok(countries.Select(country => new CountryDto
        {
            Name = country.EnglishName,
            Code = country.Iso31661
        }).ToList());
    }
}

public class LanguageDto
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("iso_639_1")] public string? Iso6391 { get; set; }
    [JsonProperty("english_name")] public string? EnglishName { get; set; }
    [JsonProperty("name")] public string? Name { get; set; }
}

public class CountryDto
{
    [JsonProperty("name")] public string? Name { get; set; }
    [JsonProperty("code")] public string? Code { get; set; }
}