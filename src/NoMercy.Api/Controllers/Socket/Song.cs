using Newtonsoft.Json;
using NoMercy.Database;

namespace NoMercy.Api.Controllers.Socket;
public class Song
{
    [JsonProperty("id")] public Guid Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("cover")] public string Cover { get; set; } = string.Empty;
    [JsonProperty("path")] public string Path { get; set; } = string.Empty;
    [JsonProperty("color_palette")] public IColorPalettes? ColorPalette { get; set; } = new();
    [JsonProperty("date")] public DateTime Date { get; set; }
    [JsonProperty("duration")] public string Duration { get; set; } = string.Empty;
    [JsonProperty("favorite")] public bool Favorite { get; set; }
    [JsonProperty("quality")] public int Quality { get; set; }
    [JsonProperty("disc")] public int Disc { get; set; }
    [JsonProperty("track")] public int Track { get; set; }

    [JsonProperty("lyrics")] public Lyric[]? Lyrics { get; set; }
    [JsonProperty("artist_track")] public Track[] ArtistTrack { get; set; } = [];
    [JsonProperty("album_track")] public Track[] AlbumTrack { get; set; } = [];
    [JsonProperty("full_cover")] public string FullCover { get; set; } = string.Empty;
}