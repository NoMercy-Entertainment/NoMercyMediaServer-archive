using Newtonsoft.Json;
using NoMercy.Providers.TMDB.Models.Movies;


namespace NoMercy.Providers.TMDB.Models.Collections;

public class TmdbCollectionDetails : TmdbCollection
{
    [JsonProperty("overview")] public string Overview { get; set; }
    [JsonProperty("parts")] public TmdbMovie[] Parts { get; set; }
}
