using Newtonsoft.Json;

namespace NoMercy.Providers.TMDB.Models.TV;

public class TmdbTvChanges
{
    [JsonProperty("key")] public string Key { get; set; }
    [JsonProperty("items")] public TmdbTvChangeItem[] Items { get; set; } = [];
}
