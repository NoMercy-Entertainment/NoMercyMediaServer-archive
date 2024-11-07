using Newtonsoft.Json;
using NoMercy.Providers.Helpers;

namespace NoMercy.Providers.FanArt.Models;

public class Image
{
    private Uri __url;

    [JsonProperty("id")] public int Id { get; set; }

    [JsonProperty("url")]
    public Uri Url
    {
        get => __url.ToHttps();
        init => __url = value;
    }

    [JsonProperty("likes")] public int Likes { get; set; }
}
