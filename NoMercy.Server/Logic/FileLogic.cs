#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.Helpers;
using NoMercy.Server.app.Helper;
using Serilog.Events;

namespace NoMercy.Server.Logic;

public partial class FileLogic(int id, Library library) : IDisposable, IAsyncDisposable
{
    private readonly MediaContext _mediaContext = new();

    private int Id { get; set; } = id;
    private Library Library { get; set; } = library;
    private Movie? Movie { get; set; }
    private Tv? Show { get; set; }

    private List<Folder> Folders { get; set; } = [];
    public List<MediaFolder> Files { get; set; } = [];
    public string Type { get; set; } = "";

    public async Task Process()
    {
        await MediaType();
        Paths();

        foreach (var folder in Folders)
        {
            var files = await GetFiles(folder.Path);

            if (!files.IsEmpty) Files.AddRange(files);
        }

        switch (Library.Type)
        {
            case "movie":
                await StoreMovie();
                break;
            case "tv":
            case "anime":
                await StoreTvShow();
                break;
            case "music":
                await StoreMusic();
                break;
            default:
                Logger.App("Unknown library type");
                break;
        }
    }

    private async Task StoreMusic()
    {
        var item = Files.FirstOrDefault(file => file.Parsed is not null)
            ?.Files?.FirstOrDefault(file => file.Parsed is not null);

        if (item == null) return;

        await StoreAudioItem(item);
    }

    private async Task StoreMovie()
    {
        var item = Files
            .SelectMany(file => file.Files ?? [])
            .FirstOrDefault(file => file.Parsed is not null);

        if (item == null) return;

        await StoreVideoItem(item);
    }

    private async Task StoreTvShow()
    {
        var items = Files
            .SelectMany(file => file.Files ?? [])
            .Where(mediaFolder => mediaFolder.Parsed is not null)
            .ToList();

        if (items.Count == 0) return;

        foreach (var item in items) await StoreVideoItem(item);
    }

    private async Task StoreAudioItem(MediaFile? item)
    {
        if (item?.Parsed is null) return;

        var folder = Folders.FirstOrDefault(folder => item.Path.Contains(folder.Path));
        if (folder == null) return;

        await Task.CompletedTask;
    }

    private async Task StoreVideoItem(MediaFile? item)
    {
        if (item?.Parsed is null) return;

        var folder = Folders.FirstOrDefault(folder => item.Path.Contains(folder.Path));
        if (folder == null) return;

        List<Subtitle> subtitles = [];

        var fileName = Path.DirectorySeparatorChar + Path.GetFileName(item.Path);
        var hostFolder = item.Path.Replace(fileName, "");
        var baseFolder = Path.DirectorySeparatorChar + (Movie?.Folder ?? Show?.Folder ?? "").Replace("/", "")
                                                     + item.Path.Replace(folder.Path, "")
                                                         .Replace(fileName, "");

        var subtitleFolder = Path.Combine(hostFolder, "subtitles");

        if (Directory.Exists(subtitleFolder))
        {
            var subtitleFiles = Directory.GetFiles(subtitleFolder);
            foreach (var subtitleFile in subtitleFiles)
            {
                var regex = SubtitleFileTagsRegex();
                var match = regex.Match(subtitleFile);
                
                if(match.Groups["type"].Value != "sign" && match.Groups["type"].Value != "song" && match.Groups["type"].Value != "full") continue;

                subtitles.Add(new Subtitle
                {
                    Language = match.Groups["lang"].Value,
                    Type = match.Groups["type"].Value,
                    Ext = match.Groups["ext"].Value
                });
            }
        }

        var episode = await _mediaContext.Episodes
            .Where(e => Show != null && e.TvId == Show.Id)
            .Where(e => e.SeasonNumber == item.Parsed.Season)
            .Where(e => e.EpisodeNumber == item.Parsed.Episode)
            .FirstOrDefaultAsync();

        try
        {
            VideoFile videoFile = new()
            {
                EpisodeId = episode?.Id,
                MovieId = Movie?.Id,
                Folder = baseFolder.Replace("\\", "/"),
                HostFolder = hostFolder.Replace("\\", "/"),
                Filename = fileName.Replace("\\", "/"),

                Share = folder.Id.ToString() ?? "",
                Duration = Regex.Replace(
                    Regex.Replace(item.FFprobe?.Duration.ToString() ?? "", "\\.\\d+", "")
                    , "^00:", ""),
                // Chapters = JsonConvert.SerializeObject(item.FFprobe?.Chapters ?? []),
                Chapters = "",
                Languages = JsonConvert.SerializeObject(item.FFprobe?.AudioStreams.Select(stream => stream.Language).Where(stream => stream != null && stream != "und")),
                Quality = item.FFprobe?.VideoStreams.FirstOrDefault()?.Width.ToString() ?? "",
                Subtitles = JsonConvert.SerializeObject(subtitles)
            };

            await _mediaContext.VideoFiles.Upsert(videoFile)
                .On(vf => vf.Filename)
                .WhenMatched((vs, vi) => new VideoFile
                {
                    Id = vi.Id,
                    EpisodeId = vi.EpisodeId,
                    MovieId = vi.MovieId,
                    Folder = vi.Folder,
                    HostFolder = vi.HostFolder,
                    Filename = vi.Filename,
                    Share = vi.Share,
                    Duration = vi.Duration,
                    Chapters = vi.Chapters,
                    Languages = vi.Languages,
                    Quality = vi.Quality,
                    Subtitles = vi.Subtitles
                })
                .RunAsync();
        }
        catch (Exception e)
        {
            Logger.App(e, LogEventLevel.Error);
        }
    }

    private async Task MediaType()
    {
        switch (Library.Type)
        {
            case "movie":
                Movie = await _mediaContext.Movies
                    .Where(m => m.Id == Id)
                    .FirstOrDefaultAsync();
                Type = "movie";
                break;
            case "tv":
                Show = await _mediaContext.Tvs
                    .Where(t => t.Id == Id)
                    .FirstOrDefaultAsync();
                Type = "tv";
                break;
        }
    }

    private async Task<ConcurrentBag<MediaFolder>> GetFiles(string path)
    {
        MediaScan mediaScan = new();

        var depth = Library.Type switch
        {
            "movie" => 1,
            "tv" => 2,
            _ => 1
        };

        var folders = await mediaScan
            .EnableFileListing()
            .FilterByMediaType(Library.Type)
            .Process(path, depth);

        mediaScan.Dispose();

        return folders;
    }

    private void Paths()
    {
        var folder = Library.Type switch
        {
            "movie" => Movie?.Folder?.Replace("/", ""),
            "tv" => Show?.Folder?.Replace("/", ""),
            _ => ""
        };

        if (folder == null) return;

        var rootFolders = Library.FolderLibraries
            .Select(f => f.Folder)
            .ToArray();

        foreach (var rootFolder in rootFolders)
        {
            var path = Path.Combine(rootFolder.Path, folder);

            if (Directory.Exists(path))
                Folders.Add(new Folder
                {
                    Path = path,
                    Id = rootFolder.Id
                });
        }
    }

    [GeneratedRegex(@"(?<lang>\w{3}).(?<type>\w{3,4}).(?<ext>\w{3})$")]
    private static partial Regex SubtitleFileTagsRegex();

    public void Dispose()
    {
        _mediaContext.Dispose();
        GC.Collect();
        GC.WaitForFullGCComplete();
        GC.WaitForPendingFinalizers();
    }

    public async ValueTask DisposeAsync()
    {
        await _mediaContext.DisposeAsync();
        GC.Collect();
        GC.WaitForFullGCComplete();
        GC.WaitForPendingFinalizers();
    }
}

public class Subtitle
{
    [JsonProperty("language")] public string Language { get; set; }
    [JsonProperty("type")] public string Type { get; set; }
    [JsonProperty("ext")] public string Ext { get; set; }
}