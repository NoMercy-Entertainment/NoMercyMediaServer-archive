using Newtonsoft.Json;

namespace NoMercy.Providers.MusixMatch.Models;

public class MusixMatchUserBlobGet
{
    [JsonProperty("message")] public UserBlobGetMessage Message { get; set; }
    [JsonProperty("meta")] public UserBlobGetMeta Meta { get; set; }
}
