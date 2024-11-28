using Newtonsoft.Json;
using NoMercy.Database.Models;

namespace NoMercy.Api.Controllers.V1.Dashboard.DTO;
public record FolderDto
{
    [JsonProperty("id")] public Ulid Id { get; set; }
    [JsonProperty("path")] public string Path { get; set; }
    [JsonProperty("encoder_profiles")] public EncoderProfile[] EncoderProfiles { get; set; }
}