using Newtonsoft.Json;

namespace NoMercy.Api.Controllers.V1.Dashboard.DTO;
public record SubtitleDto
{
    [JsonProperty("index")] public int Index { get; set; }
    [JsonProperty("language")] public string? Language { get; set; }
}