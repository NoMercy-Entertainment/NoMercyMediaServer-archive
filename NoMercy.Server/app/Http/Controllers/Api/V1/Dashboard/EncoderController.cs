using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.Encoder.Format.Container;
using NoMercy.Helpers;
using NoMercy.Networking;
using NoMercy.Server.app.Helper;
using NoMercy.Server.app.Http.Controllers.Api.V1.DTO;
using NoMercy.Server.app.Http.Controllers.Api.V1.Music;
using NoMercy.Server.app.Http.Middleware;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace NoMercy.Server.app.Http.Controllers.Api.V1.Dashboard;

[ApiController]
[Tags("Dashboard Server Encoder Profiles")]
[ApiVersion("1")]
[Authorize]
[Route("api/v{Version:apiVersion}/dashboard/encoderprofiles", Order = 10)]
public class EncoderController: BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!HttpContext.User.IsModerator())
            return UnauthorizedResponse("You do not have permission to view encoder profiles");

        await using MediaContext mediaContext = new();
        var profiles = await mediaContext.EncoderProfiles.ToListAsync();

        List<EncoderProfileDto> encoderProfiles = [];

        foreach (var profile in profiles)
        {
            if (profile.Param == null) continue;

            var paramJson = JsonConvert.DeserializeObject<ParamsDto>(profile.Param);

            EncoderProfileDto profileDto = new()
            {
                Id = profile.Id.ToString(),
                Name = profile.Name,
                Container = profile.Container,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt,
                Params = paramJson is not null
                    ? new ParamsDto
                    {
                        Width = paramJson.Width,
                        Crf = paramJson.Crf,
                        Preset = paramJson.Preset,
                        Profile = paramJson.Profile,
                        Codec = paramJson.Codec,
                        Audio = paramJson.Audio
                    }
                    : null
            };
            encoderProfiles.Add(profileDto);
        }

        return Ok(encoderProfiles);
    }

    [HttpPost]
    public async Task<IActionResult> Create()
    {
        if (!HttpContext.User.IsModerator())
            return UnauthorizedResponse("You do not have permission to create encoder profiles");
        
        try
        {
            await using MediaContext mediaContext = new();
            var encoderProfiles = await mediaContext.EncoderProfiles.CountAsync();

            EncoderProfile profile = new()
            {
                Id = Ulid.NewUlid(),
                Name = $"Profile {encoderProfiles}",
                Container = "mp4",
                Param = JsonConvert.SerializeObject(new ParamsDto
                {
                    Width = 1920,
                    Crf = 23,
                    Preset = "medium",
                    Profile = "main",
                    Codec = "libx264",
                    Audio = "aac"
                })
            };

            await mediaContext.EncoderProfiles.Upsert(profile)
                .On(l => new { l.Id })
                .WhenMatched((ls, li) => new EncoderProfile
                {
                    Id = li.Id,
                    Name = li.Name,
                    Container = li.Container,
                    Param = li.Param
                })
                .RunAsync();
            
            return Ok(new StatusResponseDto<EncoderProfile>
            {
                Status = "ok",
                Data = profile,
                Message = "Successfully created a new encoder profile.",
                Args = []
            });
        }
        catch (Exception e)
        {
            return Ok(new StatusResponseDto<EncoderProfile>()
            {
                Status = "error",
                Message = "Something went wrong creating a new library: {0}",
                Args = [e.Message]
            });
        }
        
    }

    [HttpDelete]
    [Route("{id:ulid}")]
    public async Task<IActionResult> Destroy(Ulid id)
    {
        if (!HttpContext.User.IsModerator())
            return UnauthorizedResponse("You do not have permission to remove encoder profiles");
        
        await using MediaContext mediaContext = new();
        var profile = await mediaContext.EncoderProfiles
            .Where(profile => profile.Id == id)
            .FirstOrDefaultAsync();
        
        if (profile == null)
            return NotFound(new StatusResponseDto<string>
            {
                Status = "error",
                Data = "Encoder profile not found"
            });
        
        mediaContext.EncoderProfiles.Remove(profile);
        await mediaContext.SaveChangesAsync();

        return Ok(new StatusResponseDto<string>
        {
            Status = "ok",
            Data = "Profile removed"
        });
    }
    
    [HttpGet]
    [Route("containers")]
    public IActionResult Containers()
    {
        if (!HttpContext.User.IsModerator())
            return UnauthorizedResponse("You do not have permission to remove encoder profiles");
        
        var containers = BaseContainer.AvailableContainers
            .Select(container => new ContainerDto
            {
                Label = container.Name,
                Value = container.Name,
                Type = container.Type,
                IsDefault = container.IsDefault
            });

        return Ok(new DataResponseDto<ContainerDto[]>
        {
            Data = containers.ToArray()
        });
    }
}

public class EncoderProfileDto
{
    [JsonProperty("id")] public string Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; }
    [JsonProperty("container")] public string? Container { get; set; }
    [JsonProperty("created_at")] public DateTime CreatedAt { get; set; }
    [JsonProperty("updated_at")] public DateTime UpdatedAt { get; set; }
    [JsonProperty("params")] public ParamsDto? Params { get; set; }
}

public class ContainerDto
{
    [JsonProperty("label")] public string Label { get; set; }
    [JsonProperty("value")] public string Value { get; set; }
    [JsonProperty("type")] public string Type { get; set; }
    [JsonProperty("default")] public bool IsDefault { get; set; }
}

public class ParamsDto
{
    [JsonProperty("video")] public int Width { get; set; }
    [JsonProperty("crf")] public int Crf { get; set; }
    [JsonProperty("preset")] public string Preset { get; set; }
    [JsonProperty("profile")] public string Profile { get; set; }
    [JsonProperty("codec")] public string Codec { get; set; }
    [JsonProperty("audio")] public string Audio { get; set; }
    [JsonProperty("tune")] public string Tune { get; set; }
    [JsonProperty("level")] public string Level { get; set; }
}