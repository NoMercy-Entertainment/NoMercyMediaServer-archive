using Newtonsoft.Json;

namespace NoMercy.Setup.Dto;
public class Download
{
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("path")] public string Path { get; set; } = string.Empty;
    [JsonProperty("url")] public Uri? Url { get; set; }
    [JsonProperty("filter")] public string Filter { get; set; } = string.Empty;
    [JsonProperty("last_updated")] public DateTime LastUpdated { get; set; }
}