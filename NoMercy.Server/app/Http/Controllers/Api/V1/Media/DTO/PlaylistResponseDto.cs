using Newtonsoft.Json;
using NoMercy.Database.Models;
using NoMercy.NmSystem;
using NoMercy.Server.Logic;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace NoMercy.Server.app.Http.Controllers.Api.V1.Media.DTO;

public record PlaylistResponseDto
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("title")] public string? Title { get; set; }
    [JsonProperty("description")] public string? Description { get; set; }
    [JsonProperty("show")] public string? Show { get; set; }
    [JsonProperty("origin")] public Guid Origin { get; set; }
    [JsonProperty("uuid")] public int Uuid { get; set; }
    [JsonProperty("video_id")] public Ulid VideoId { get; set; }
    [JsonProperty("duration")] public string Duration { get; set; }
    [JsonProperty("tmdbid")] public int Tmdbid { get; set; }
    [JsonProperty("video_type")] public string VideoType { get; set; }
    [JsonProperty("playlist_type")] public string PlaylistType { get; set; }
    [JsonProperty("year")] public long Year { get; set; }
    [JsonProperty("progress")] public ProgressDto? Progress { get; set; }
    [JsonProperty("poster")] public string? Poster { get; set; }
    [JsonProperty("image")] public string? Image { get; set; }
    [JsonProperty("logo")] public string? Logo { get; set; }
    [JsonProperty("sources")] public SourceDto[] Sources { get; set; }
    [JsonProperty("fonts")] public List<FontDto?>? Fonts { get; set; }
    [JsonProperty("fontsFile")] public string FontsFile { get; set; }
    [JsonProperty("textTracks")] public List<TextTrackDto> TextTracks { get; set; }
    [JsonProperty("tracks")] public List<TrackDto> Tracks { get; set; }

    [JsonProperty("season")] public int? Season { get; set; }
    [JsonProperty("episode")] public int? Episode { get; set; }
    [JsonProperty("seasonName")] public string? SeasonName { get; set; }
    [JsonProperty("episode_id")] public int? EpisodeId { get; set; }

    public PlaylistResponseDto(Episode episode, int? index = null)
    {
        var videoFile = episode.VideoFiles.FirstOrDefault();
        if (videoFile is null) return;

        var userData = videoFile.UserData.FirstOrDefault();
        var baseFolder = $"/{videoFile.Share}{videoFile.Folder}";

        var logo = episode.Tv.Images.FirstOrDefault(image => image.Type == "logo")?.FilePath;

        var tvTitle = episode.Tv.Translations.FirstOrDefault()?.Title ?? episode.Tv.Title;

        var title = episode.Translations.FirstOrDefault()?.Title ?? episode.Title;
        var overview = episode.Translations.FirstOrDefault()?.Overview ?? episode.Overview;

        var specialTitle = index is not null
            ? $"{tvTitle} %S{episode.SeasonNumber} %E{episode.EpisodeNumber} - {title}"
            : title;

        var subs = Subtitles(videoFile);

        Id = episode.Id;
        Title = specialTitle;
        Description = overview;
        Show = index is not null
            ? null
            : tvTitle;
        Origin = Info.DeviceId;
        Uuid = episode.Tv.Id + episode.Id;
        VideoId = videoFile.Id;
        Duration = videoFile.Duration ?? "0";
        Tmdbid = episode.Tv.Id;
        VideoType = "tv";
        PlaylistType = "tv";
        Year = episode.Tv.FirstAirDate.ParseYear();
        Progress = userData?.UpdatedAt is not null
            ? new ProgressDto
            {
                Percentage =
                    (int)Math.Round((double)(100 * (userData.Time ?? 0)) / (videoFile.Duration?.ToSeconds() ?? 0)),
                Date = userData.UpdatedAt
            }
            : null;
        Poster = episode.Tv.Poster is not null ? "https://image.tmdb.org/t/p/w300" + episode.Tv.Poster : null;
        Image = episode.Still is not null ? "https://image.tmdb.org/t/p/w300" + episode.Still : null;
        Logo = logo is not null ? "https://image.tmdb.org/t/p/original" + logo : null;
        Sources =
        [
            new SourceDto
            {
                Src = $"{baseFolder}{videoFile.Filename}",
                Type = videoFile.Filename.Contains(".mp4")
                    ? "video/mp4"
                    : "application/x-mpegURL",
                Languages = JsonConvert.DeserializeObject<string?[]>(videoFile.Languages)
                    ?.Where(lang => lang != null).ToArray()
            }
        ];
        Fonts = subs.Fonts;
        FontsFile = subs.FontsFile;
        Tracks =
        [
            new TrackDto()
            {
                File = $"{baseFolder}/previews.vtt",
                Kind = "thumbnails"
            },
            new TrackDto()
            {
                File = $"{baseFolder}/chapters.vtt",
                Kind = "chapters"
            },
            new TrackDto()
            {
                File = $"{baseFolder}/skippers.vtt",
                Kind = "skippers"
            },
            new TrackDto()
            {
                File = $"{baseFolder}/sprite.webp",
                Kind = "sprite"
            },
            new TrackDto()
            {
                File = $"{baseFolder}/fonts.json",
                Kind = "fonts"
            },
        ];
        TextTracks = subs.TextTracks;

        Season = index is not null ? 0 : episode.SeasonNumber;
        Episode = index ?? episode.EpisodeNumber;
        SeasonName = episode.Season.Title;
        EpisodeId = episode.Id;
    }

    public PlaylistResponseDto(Movie movie, int? index = null, Collection? collection = null)
    {
        var videoFile = movie.VideoFiles.FirstOrDefault();
        if (videoFile is null) return;

        var logo = movie.Images.FirstOrDefault(image => image.Type == "logo")?.FilePath;
        var userData = videoFile.UserData.FirstOrDefault();
        var baseFolder = $"/{videoFile.Share}{videoFile.Folder}";

        var title = movie.Translations.FirstOrDefault()?.Title ?? movie.Title;
        var overview = movie.Translations.FirstOrDefault()?.Overview ?? movie.Overview;

        var subs = Subtitles(videoFile);

        Id = movie.Id;
        Title = title;
        Description = overview;
        Origin = Info.DeviceId;
        Uuid = movie.Id;
        VideoId = videoFile.Id;
        Duration = videoFile.Duration ?? "0";
        Tmdbid = collection?.Id ?? movie.Id;
        VideoType = "movie";
        PlaylistType = "movie";
        Year = movie.ReleaseDate.ParseYear();
        Progress = userData?.UpdatedAt is not null
            ? new ProgressDto
            {
                Percentage =
                    (int)Math.Round((double)(100 * (userData.Time ?? 0)) / (videoFile.Duration?.ToSeconds() ?? 0)),
                Date = userData.UpdatedAt
            }
            : null;
        Poster = movie.Poster is not null ? "https://image.tmdb.org/t/p/w300" + movie.Poster : null;
        Image = movie.Backdrop is not null ? "https://image.tmdb.org/t/p/w300" + movie.Backdrop : null;
        Logo = logo is not null ? "https://image.tmdb.org/t/p/original" + logo : null;
        Sources =
        [
            new SourceDto
            {
                Src = $"{baseFolder}{videoFile.Filename}",
                Type = videoFile.Filename.Contains(".mp4")
                    ? "video/mp4"
                    : "application/x-mpegURL",
                Languages = JsonConvert.DeserializeObject<string?[]>(videoFile.Languages)
                    ?.Where(lang => lang != null).ToArray()
            }
        ];
        Fonts = subs.Fonts;
        FontsFile = subs.FontsFile;
        Tracks =
        [
            new TrackDto()
            {
                File = $"{baseFolder}/thumbs_256x144.vtt",
                Kind = "thumbnails"
            },
            new TrackDto()
            {
                File = $"{baseFolder}/chapters.vtt",
                Kind = "chapters"
            },
            new TrackDto()
            {
                File = $"{baseFolder}/skippers.vtt",
                Kind = "skippers"
            },
            new TrackDto()
            {
                File = $"{baseFolder}/thumbs_256x144.webp",
                Kind = "sprite"
            },
            new TrackDto()
            {
                File = $"{baseFolder}/fonts.json",
                Kind = "fonts"
            },

            new TrackDto()
            {
                File = "",
                Kind = "thumbnails"
            }
        ];
        TextTracks = subs.TextTracks;

        if (index is null) return;
        SeasonName = "Collection";
        Season = 0;
        Episode = index;
        EpisodeId = movie.Id;
    }

    private record Subs
    {
        public List<TextTrackDto> TextTracks { get; set; }
        public List<FontDto?>? Fonts { get; set; }
        public string FontsFile { get; set; }
    }

    private static Subs Subtitles(VideoFile videoFile)
    {
        var baseFolder = $"/{videoFile.Share}{videoFile.Folder}";

        var subtitles = videoFile.Subtitles ?? "[]";
        var subtitleList = JsonConvert.DeserializeObject<List<Subtitle>>(subtitles);

        List<TextTrackDto> textTracks = [];
        var search = false;

        foreach (var sub in subtitleList ?? [])
        {
            var language = sub.Language;
            var type = sub.Type;
            var ext = sub.Ext;

            if (ext == "ass") search = true;

            textTracks.Add(new TextTrackDto
            {
                Label = type,
                Type = type,
                Src = $"{baseFolder}/subtitles{videoFile?.Filename
                    .Replace(".mp4", "")
                    .Replace(".m3u8", "")}.{language}.{type}.{ext}",
                SrcLang = $"languages:{language}",
                Ext = ext,
                Language = language,
                Kind = "subtitles"
            });
        }

        List<FontDto?>? fonts = [];
        var fontsFile = "";

        if (!search || !System.IO.File.Exists($"{videoFile?.HostFolder}fonts.json"))
            return new Subs
            {
                TextTracks = textTracks,
                Fonts = fonts,
                FontsFile = fontsFile
            };

        fontsFile = $"/{videoFile?.Share}/{videoFile?.Folder}fonts.json";
        fonts = JsonConvert.DeserializeObject<List<FontDto?>?>(
            System.IO.File.ReadAllText($"{videoFile?.HostFolder}fonts.json"));

        return new Subs
        {
            TextTracks = textTracks,
            Fonts = fonts,
            FontsFile = fontsFile
        };
    }
}

public record SourceDto
{
    [JsonProperty("src")] public string Src { get; set; }
    [JsonProperty("type")] public string Type { get; set; }
    [JsonProperty("languages")] public string?[]? Languages { get; set; }
}

public record TextTrackDto
{
    [JsonProperty("label")] public string Label { get; set; }
    [JsonProperty("type")] public string Type { get; set; }
    [JsonProperty("src")] public string Src { get; set; }
    [JsonProperty("srclang")] public string SrcLang { get; set; }
    [JsonProperty("ext")] public string Ext { get; set; }
    [JsonProperty("language")] public string Language { get; set; }
    [JsonProperty("kind")] public string Kind { get; set; }
}

public record TrackDto
{
    [JsonProperty("file")] public string File { get; set; }
    [JsonProperty("kind")] public string Kind { get; set; }
}

public record ProgressDto
{
    [JsonProperty("percentage")] public int? Percentage { get; set; }
    [JsonProperty("date")] public DateTime? Date { get; set; }
}

public record FontDto
{
    [JsonProperty("file")] public string File { get; set; }
    [JsonProperty("type")] public string Type { get; set; }
}