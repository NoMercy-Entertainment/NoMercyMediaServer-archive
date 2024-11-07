using Newtonsoft.Json;

namespace NoMercy.Providers.FanArt.Models;

public class FanArtLabel
{
    [JsonProperty("name")] public string Name { get; set; }
    [JsonProperty("id")] public string Id { get; set; }
    [JsonProperty("musiclabel")] public MusicLabel[] Labels { get; set; }
}
