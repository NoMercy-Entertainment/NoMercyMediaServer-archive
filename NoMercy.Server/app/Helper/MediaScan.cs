#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using FFMpegCore;
using MovieFileLibrary;
using NoMercy.Helpers;
using Serilog.Events;
using AppFiles = NoMercy.NmSystem.AppFiles;

namespace NoMercy.Server.app.Helper;

public class MovieFileExtend
{
    public string? Title { get; init; }
    public string? Year { get; init; }
    public bool IsSeries { get; set; }
    public int? Season { get; init; }
    public int? Episode { get; init; }
    public bool IsSuccess { get; set; }
    public string FilePath { get; set; }
}

public class MediaFolder
{
    public string Name { get; init; }
    public string Path { get; init; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
    public DateTime Accessed { get; set; }
    public string Type { get; set; }
    public ConcurrentBag<MediaFile>? Files { get; init; }
    public ConcurrentBag<MediaFolder>? SubFolders { get; init; }
    public MovieFileExtend? Parsed { get; init; }
}

public class MediaFile
{
    public string Name { get; init; }
    public string Path { get; init; }
    public string Extension { get; set; }
    public int Size { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
    public DateTime Accessed { get; set; }
    public string Type { get; set; }
    public MovieFileExtend? Parsed { get; init; }
    public IMediaAnalysis? FFprobe { get; init; }
    // public Fingerprint? FingerPint { get; init; }
}

public class MediaScan : IDisposable, IAsyncDisposable
{
    private readonly MovieDetector _movieDetector = new();

    private bool _fileListingEnabled;
    private bool _regexFilterEnabled = true;

    private readonly Regex _folderNameRegex =
        new(
            @"video_.*|audio_.*|subtitles|scans|cds.*|ost|album|music|original|fonts|thumbs|metadata|NCED|NCOP|\s\(\d\)\.|~",
            RegexOptions.IgnoreCase);

    private string[] _extensionFilter = [];
    

    public MediaScan()
    {
        GlobalFFOptions.Configure(options => options.BinaryFolder = Path.Combine(AppFiles.BinariesPath, "ffmpeg"));
    }

    public MediaScan EnableFileListing()
    {
        _fileListingEnabled = true;

        return this;
    }

    public MediaScan DisableRegexFilter()
    {
        _regexFilterEnabled = false;

        return this;
    }

    public MediaScan FilterByMediaType(string mediaType)
    {
        _extensionFilter = mediaType switch
        {
            "anime" or "tv" or "movie" or "video" => [".mp4", ".avi", ".mkv", ".m3u8"],
            "music" => [".mp3", ".flac", ".wav", ".m4a"],
            "subtitle" => [".srt", ".vtt", ".ass"],
            _ => throw new ArgumentOutOfRangeException(nameof(mediaType), mediaType, null)
        };

        return this;
    }

    public Task<ConcurrentBag<MediaFolder>> Process(string rootFolder, int depth = 0)
    {
        return !_fileListingEnabled
            ? Task.Run(() => ScanFoldersOnly(rootFolder, depth))
            : Task.Run(() => ScanFolder(rootFolder, depth));
    }

    private ConcurrentBag<MediaFolder> ScanFolder(string folderPath, int depth)
    {
        ConcurrentBag<MediaFolder> folders = [];

        if (depth < 0) return folders;

        var files = Files(folderPath);

        var movieFile1 = _movieDetector.GetInfo(folderPath);
        movieFile1.Year ??= NmSystem.Str.MatchYearRegex().Match(folderPath)
            .Value;

        folders.Add(new MediaFolder
        {
            Name = Path.GetFileName(folderPath),
            Path = folderPath,
            Created = Directory.GetCreationTime(folderPath),
            Modified = Directory.GetLastWriteTime(folderPath),
            Accessed = Directory.GetLastAccessTime(folderPath),
            Type = "folder",
            Parsed = new MovieFileExtend
            {
                Title = movieFile1.Title,
                Year = movieFile1.Year,
                FilePath = movieFile1.Path
            },

            Files = files.Count > 0
                ? files
                : null
        });

        try
        {
            var directories = Directory.GetDirectories(folderPath).OrderBy(f => f);

            foreach (var directory in directories)
            {
                var folderName = Path.GetFileName(directory);

                if ((_regexFilterEnabled && _folderNameRegex.IsMatch(folderName)) || depth == 0)
                {
                    files?.Add(new MediaFile
                    {
                        Name = folderName,
                        Path = directory,
                        Created = Directory.GetCreationTime(directory),
                        Modified = Directory.GetLastWriteTime(directory),
                        Accessed = Directory.GetLastAccessTime(directory),
                        Type = "folder"
                    });

                    continue;
                }

                var files2 = depth - 1 > 0 ? Files(directory) : [];

                var movieFile = _movieDetector.GetInfo(directory);
                movieFile.Year ??= NmSystem.Str.MatchYearRegex()
                    .Match(directory).Value;

                folders.Add(new MediaFolder
                {
                    Name = folderName,
                    Path = directory,
                    Created = Directory.GetCreationTime(directory),
                    Modified = Directory.GetLastWriteTime(directory),
                    Accessed = Directory.GetLastAccessTime(directory),
                    Type = "folder",
                    Parsed = new MovieFileExtend
                    {
                        Title = movieFile.Title,
                        Year = movieFile.Year,
                        FilePath = movieFile.Path
                    },

                    Files = files2 is { Count: > 0 }
                        ? files2
                        : null,

                    SubFolders = depth - 1 > 0
                        ? ScanFolder(directory, depth - 1)
                        : null
                });
            }

            ConcurrentBag<MediaFolder> response = new(folders
                .Where(f => f.Name is not "")
                .OrderByDescending(f => f.Name));

            return response;
        }
        catch (Exception)
        {
            return [];
        }
    }

    private ConcurrentBag<MediaFolder> ScanFoldersOnly(string folderPath, int depth)
    {
        if (depth < 0) return [];

        try
        {
            ConcurrentBag<MediaFolder> folders = [];

            var directories = Directory.GetDirectories(folderPath).OrderBy(f => f);

            foreach (var directory in directories)
            {
                Logger.App($"Scanning {directory}");

                var folderName = Path.GetFileName(directory);

                if (_regexFilterEnabled && _folderNameRegex.IsMatch(folderName))
                {
                    folders.Add(new MediaFolder
                    {
                        Name = folderName,
                        Path = directory,
                        Created = Directory.GetCreationTime(directory),
                        Modified = Directory.GetLastWriteTime(directory),
                        Accessed = Directory.GetLastAccessTime(directory),
                        Type = "folder"
                    });

                    continue;
                }

                var movieFile = _movieDetector.GetInfo(directory);

                movieFile.Year ??= NmSystem.Str.MatchYearRegex()
                    .Match(directory).Value;

                folders.Add(new MediaFolder
                {
                    Name = folderName,
                    Path = directory,
                    Created = Directory.GetCreationTime(directory),
                    Modified = Directory.GetLastWriteTime(directory),
                    Accessed = Directory.GetLastAccessTime(directory),
                    Type = "folder",

                    Parsed = new MovieFileExtend
                    {
                        Title = movieFile.Title,
                        Year = movieFile.Year,
                        FilePath = movieFile.Path
                    },

                    SubFolders = depth - 1 > 0
                        ? ScanFoldersOnly(directory, depth - 1)
                        : null
                });
            }

            ConcurrentBag<MediaFolder> response = new(folders
                .Where(f => f.Name is not "")
                .OrderByDescending(f => f.Name));

            return response;
        }
        catch (Exception)
        {
            return [];
        }
    }

    private ConcurrentBag<MediaFile> Files(string folderPath)
    {
        try
        {
            ConcurrentBag<MediaFile> files = [];

            Parallel.ForEach(Directory.GetFiles(folderPath), (file, _) =>
            {
                var extension = Path.GetExtension(file).ToLower();

                if (_extensionFilter.Length > 0 && !_extensionFilter.Contains(extension)) return;

                var isVideoFile = extension is ".mp4" or ".avi" or ".mkv" or ".m3u8";
                var isAudioFile = extension is ".mp3" or ".flac" or ".wav" or ".m4a";
                var isSubtitleFile = extension is ".srt" or ".vtt" or ".ass" or ".sub";

                if (!isVideoFile && !isAudioFile && !isSubtitleFile) return;

                var movieFile = isVideoFile || isAudioFile ? _movieDetector.GetInfo(file) : null;
                var animeInfo = AnimeParser.ParseAnimeFilename(file);

                if (movieFile is not null)
                {
                    movieFile.Year ??= NmSystem.Str.MatchYearRegex()
                        .Match(file).Value;
                    movieFile.Title ??= animeInfo.Name;
                    movieFile.Season ??= animeInfo.Season;
                    movieFile.Episode ??= animeInfo.Episode;
                }

                MovieFileExtend movieFileExtend = new()
                {
                    FilePath = movieFile?.Path ?? file,
                    Episode = movieFile?.Episode,
                    Year = movieFile?.Year,
                    Season = movieFile?.Season,
                    Title = movieFile?.Title,
                    IsSeries = movieFile?.IsSeries ?? false,
                    IsSuccess = movieFile?.IsSuccess ?? false
                };

                IMediaAnalysis? ffprobe;
                try
                {
                    ffprobe = isVideoFile
                        ? FFProbe.Analyse(file)
                        : null;
                }
                catch (Exception e)
                {
                    Logger.App(e, LogEventLevel.Fatal);
                    throw;
                }
                
                // Fingerprint? fingerPrint;
                // try
                // {
                //     using FingerprintClient fingerprintClient = new();
                //     fingerPrint = isAudioFile
                //         ? fingerprintClient.Lookup(file).Result
                //         : null;
                // }
                // catch (Exception e)
                // {
                //     Console.WriteLine(e);
                //     throw;
                // }

                MediaFile res = new()
                {
                    Name = Path.GetFileName(file),
                    Path = file,
                    Extension = extension,
                    Size = (int)new FileInfo(file).Length,
                    Created = File.GetCreationTime(file),
                    Modified = File.GetLastWriteTime(file),
                    Accessed = File.GetLastAccessTime(file),
                    Type = "file",

                    Parsed = movieFileExtend,
                    FFprobe = ffprobe,
                    // FingerPint = fingerPrint
                };

                files.Add(res);
            });

            ConcurrentBag<MediaFile> response = new(files
                .Where(f => f.Name is not "")
                .OrderBy(f => f.Name));

            return response;
        }
        catch (Exception)
        {
            return [];
        }
    }

    public void Dispose()
    {
        GC.Collect();
        GC.WaitForFullGCComplete();
        GC.WaitForPendingFinalizers();
    }

    public ValueTask DisposeAsync()
    {
        GC.Collect();
        GC.WaitForFullGCComplete();
        GC.WaitForPendingFinalizers();
        
        return ValueTask.CompletedTask;
    }
}