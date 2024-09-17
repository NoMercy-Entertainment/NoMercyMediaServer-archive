using Newtonsoft.Json;

namespace NoMercy.Api.Controllers.V1.Dashboard.DTO;
public record StreamsDto
{
    [JsonProperty("video")] public IEnumerable<VideoDto> Video { get; set; } = new List<VideoDto>();
    [JsonProperty("audio")] public IEnumerable<AudioDto> Audio { get; set; } = new List<AudioDto>();
    [JsonProperty("subtitle")] public IEnumerable<SubtitleDto> Subtitle { get; set; } = new List<SubtitleDto>();
}