using Newtonsoft.Json;
using NoMercy.Database;

namespace NoMercy.Api.Controllers.Socket;
public class Track
{
    [JsonProperty("id")] public Guid Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("cover")] public string Cover { get; set; } = string.Empty;
    [JsonProperty("path")] public string Path { get; set; } = string.Empty;
    [JsonProperty("color_palette")] public IColorPalettes? ColorPalette { get; set; } = new();
    [JsonProperty("description")] public string Description { get; set; } = string.Empty;
}