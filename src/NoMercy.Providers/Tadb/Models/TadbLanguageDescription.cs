using Newtonsoft.Json;

namespace NoMercy.Providers.Tadb.Models;

public class TadbLanguageDescription
{
    [JsonProperty("iso_3166_1")] public string Iso31661 { get; set; }
    [JsonProperty("description")] public string Description { get; set; }
}
