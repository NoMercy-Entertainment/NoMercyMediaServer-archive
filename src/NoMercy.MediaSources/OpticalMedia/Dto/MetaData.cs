using FFMpegCore.Builders.MetaData;
using Newtonsoft.Json;

namespace NoMercy.MediaSources.OpticalMedia.Dto;

public class MetaData
{
    [JsonProperty("title")] public string Title = string.Empty;
    [JsonProperty("playlists")] public  IEnumerable<PlaylistItem>? Playlists;
    [JsonProperty("ffprobe_data")] public FfprobeResult? FfProbeData { get; set; } = new();
    [JsonProperty("bluRay_playlists")] public List<BluRayPlaylist> BluRayPlaylists { get; set; } = [];
}

public class PlaylistItem
{
    [JsonProperty("playlist")] public string Playlist = string.Empty;
    [JsonProperty("streams")] public IEnumerable<Stream>? Streams;
    [JsonProperty("duration")] public string Duration = string.Empty;
    [JsonProperty("chapters")] public IEnumerable<ChapterData> Chapters { get; set; } = [];
}   