using Newtonsoft.Json;

namespace NoMercy.Providers.FanArt.Models;

public class FanArtAlbum
{
    [JsonProperty("name")] public string Name { get; set; }
    [JsonProperty("mbid_id")] public string MbId { get; set; }
    [JsonProperty("albums")] public Dictionary<Guid, Albums> Albums { get; set; } = [];
}
