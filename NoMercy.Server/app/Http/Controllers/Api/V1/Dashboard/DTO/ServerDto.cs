using MovieFileLibrary;
using Newtonsoft.Json;
using NoMercy.Helpers.Monitoring;

namespace NoMercy.Server.app.Http.Controllers.Api.V1.Dashboard.DTO;

public class ServerInfoDto
{
    [JsonProperty("server")] public string Server { get; set; }
    [JsonProperty("cpu")] public string? Cpu { get; set; }
    [JsonProperty("gpu")] public string[]? Gpu { get; set; }
    [JsonProperty("os")] public string Os { get; set; }
    [JsonProperty("arch")] public string Arch { get; set; }
    [JsonProperty("version")] public string? Version { get; set; }
    [JsonProperty("bootTime")] public DateTime BootTime { get; set; }
}

public class ServerPathsDto
{
    [JsonProperty("key")] public string Key { get; set; }
    [JsonProperty("value")] public string Value { get; set; }
}

public class DirectoryRequest
{
    [JsonProperty("path")] public string Path { get; set; }
}

public class PathRequest
{
    [JsonProperty("folder")] public string Folder { get; set; }
}

public class Video
{
    [JsonProperty("index")] public int Index { get; set; }
    [JsonProperty("width")] public int? Width { get; set; }
    [JsonProperty("height")] public int? Height { get; set; }
}

public class Audio
{
    [JsonProperty("index")] public int Index { get; set; }
    [JsonProperty("language")] public string? Lanuage { get; set; }
}

public class Subtitle
{
    [JsonProperty("index")] public int Index { get; set; }
    [JsonProperty("language")] public string? Language { get; set; }
}

public class FileListRequest
{
    [JsonProperty("folder")] public string Folder { get; set; }
    [JsonProperty("type")] public string Type { get; set; }
}

public class DirectoryTreeDto
{
    [JsonProperty("path")] public string Path { get; set; }
    [JsonProperty("mode")] public int Mode { get; set; }
    [JsonProperty("size")] public int? Size { get; set; }
    [JsonProperty("type")] public string Type { get; set; }
    [JsonProperty("parent")] public string Parent { get; set; }
    [JsonProperty("full_path")] public string FullPath { get; set; }
}

public class SetupResponseDto
{
    [JsonProperty("setup_complete")] public bool SetupComplete { get; set; }
}

public class ServerUpdateRequest
{
    [JsonProperty("name")] public string Name { get; set; }
}

public class ResourceInfoDto
{
    [JsonProperty("cpu")] public Cpu Cpu { get; set; }
    [JsonProperty("gpu")] public List<Gpu> Gpu { get; set; }
    [JsonProperty("memory")] public Memory Memory { get; set; }
    [JsonProperty("storage")] public List<ResourceMonitorDto> Storage { get; set; }
}

public class FileListResponseDto
{
    [JsonProperty("status")] public string Status { get; set; }
    [JsonProperty("files")] public List<FileItemDto> Files { get; set; }
}

public class FileItemDto {
    [JsonProperty("name")] public string Name { get; set; }
    [JsonProperty("mode")] public int Mode { get; set; }
    [JsonProperty("parent")] public string? Parent { get; set; }
    [JsonProperty("size")] public long Size { get; set; }
    [JsonProperty("parsed")] public MovieFile Parsed { get; set; }
    [JsonProperty("match")] public MovieOrEpisode Match { get; set; }
    [JsonProperty("streams")] public Streams Streams { get; set; }
    [JsonProperty("file")] public string File { get; set; }
}

public class Streams
{
    [JsonProperty("video")] public IEnumerable<Video> Video { get; set; }
    [JsonProperty("audio")] public IEnumerable<Audio> Audio { get; set; }
    [JsonProperty("subtitle")] public IEnumerable<Subtitle> Subtitle { get; set; }
}

public class MovieOrEpisode
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("title")] public string Title { get; set; } = string.Empty;
    [JsonProperty("duration")] public TimeSpan Duration { get; set; }
    [JsonProperty("adult")] public bool Adult { get; set; }
    [JsonProperty("overview")] public string? Overview { get; set; }
    [JsonProperty("episode_number")] public int EpisodeNumber { get; set; }
    [JsonProperty("season_number")] public int SeasonNumber { get; set; }
    [JsonProperty("still")] public string? Still { get; set; }
}

public class AddFilesRequest
{
    [JsonProperty("library_id")] public Ulid LibraryId { get; set; }
    [JsonProperty("folder_id")] public Ulid FolderId { get; set; }
    [JsonProperty("files")] public string[] Files { get; set; }
}