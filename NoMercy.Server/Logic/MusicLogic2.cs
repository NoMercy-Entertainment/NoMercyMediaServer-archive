using System.Text.RegularExpressions;
using FFMpegCore;
using Microsoft.EntityFrameworkCore;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.Helpers;
using NoMercy.NmSystem;
using NoMercy.Providers.AcoustId.Client;
using NoMercy.Providers.AcoustId.Models;
using NoMercy.Providers.MusicBrainz.Client;
using NoMercy.Providers.MusicBrainz.Models;
using NoMercy.Server.app.Jobs;
using NoMercy.Server.system;

using Serilog.Events;
using Artist = NoMercy.Database.Models.Artist;
using Track = NoMercy.Database.Models.Track;

namespace NoMercy.Server.Logic;

public partial class MusicLogic2(Library library, string inputFile) : IAsyncDisposable
{
    private AcoustIdFingerprint? FingerPrint { get; set; }
    private ParsedTrack? ParsedTrack { get; set; }
    private IMediaAnalysis? MediaAnalysis { get; set; }
    private Library Library { get; } = library;
    private string InputFile { get; } = inputFile;

    private MediaContext MediaContext { get; } = new();
    private MusicBrainzRecordingAppends Music { get; } = new();

    private string FilePath { get; set; } = "";
    private string Folder { get; set; } = "";
    private string HostFolder { get; set; } = "";

    private string ArtistFolder { get; set; } = "";
    private string SanitizedArtistFolderName { get; set; } = "";

    private string AlbumFolder { get; set; } = "";
    private string SanitizedAlbumFolderName { get; set; } = "";

    private string FileName { get; set; } = "";
    private string SanitizedFileName { get; set; } = "";

    private string SanitizedFile { get; set; } = "";
    private Ulid FolderId { get; set; }

    private AcoustIdFingerprintReleaseGroups? Release { get; set; }
    private bool FingerPrintFailed { get; set; } = true;
    
    private List<string> Files { get; set; } = [];

    public async Task Process()
    {
        await Init();

        FingerPrintFailed = FindMatchByFingerprint();

        await FindMatchByParsedTrack();

        await ProcessMatch();

        await DispatchJobs();
    }

    private async Task ProcessMatch()
    {
        if (Release is null) return;
        if (Release.Id == Guid.Empty) return;

        MusicBrainzReleaseClient musicBrainzReleaseClient = new(Release.Id);
        var releaseAppends = await musicBrainzReleaseClient.WithAllAppends();

        if (releaseAppends is null) return;
        
        await StoreReleaseGroups(releaseAppends);

        await StoreRelease(releaseAppends);
        await LinkReleaseToLibrary(releaseAppends);

        var hasTracks = false;

        foreach (var media in releaseAppends.Media)
        {
            foreach (var track in media.Tracks)
            {
                if (await StoreTrack(releaseAppends, track, media) is null) continue;
                
                hasTracks = true;

                await LinkTrackToRelease(track, releaseAppends);

                foreach (var artist in track.ArtistCredit)
                {
                    await StoreArtist(artist.MusicBrainzArtist);
                    await LinkArtistToTrack(artist.MusicBrainzArtist, track);

                    await LinkArtistToAlbum(artist.MusicBrainzArtist, releaseAppends);
                    await LinkArtistToLibrary(artist.MusicBrainzArtist);
                    
                    await LinkArtistToReleaseGroup(releaseAppends, artist.MusicBrainzArtist.Id);
                }
            }
        }
        
        if (!hasTracks) await CleanupEmptyAlbum(releaseAppends);

        await Task.CompletedTask;
    }

    private async Task CleanupEmptyAlbum(MusicBrainzReleaseAppends musicBrainzReleaseAppends)
    {
        await using MediaContext mediaContext = new();
        var album = mediaContext.Albums
            .Where(a => a.Id == musicBrainzReleaseAppends.Id)
            .Include(a => a.AlbumTrack)
                .ThenInclude(albumTrack => albumTrack.Track)
            .FirstOrDefault();

        if (album is null) return;

        var shouldRemove = album.AlbumTrack.All(t => t.Track.Filename is null && t.Track.Duration is null);
        if (!shouldRemove) return;

        var trackIds = album.AlbumTrack.Select(t => t.TrackId).ToArray();
        foreach (var trackId in trackIds)
        {
            var track = await mediaContext.Tracks.FindAsync(trackId);
            if (track is null) continue;

            mediaContext.Tracks.Remove(track);
            mediaContext.AlbumTrack.RemoveRange(album.AlbumTrack);
        }

        album.AlbumTrack.Clear();
        mediaContext.Albums.Remove(album);
        await mediaContext.SaveChangesAsync();
    }

    private async Task FindMatchByParsedTrack()
    {
        if (!FingerPrintFailed) return;

        await Task.CompletedTask;
    }

    private bool FindMatchByFingerprint()
    {
        if (FingerPrint is null) return false;

        var recordings = FingerPrint.Results
            .SelectMany(r => r.Recordings ?? [])
            .Where(rec =>
            {
                if (rec?.Title is null) return false;
                if (ParsedTrack?.Title is null) return false;

                if (rec.Title.ToLower().RemoveDiacritics() !=
                    MyRegex().Replace(ParsedTrack?.Title.ToLower() ?? "", "").RemoveDiacritics()) return false;
                if (rec.Artists.Length == 0) return false;

                if (Math.Abs(rec.Duration - (MediaAnalysis?.Duration.TotalSeconds ?? 0)) > 15) return false;

                if (rec.Releases?.Length == 0) return false;

                return true;
            });
        
        var releases = recordings
            .SelectMany(rec => rec?.Releases?
                .Where(rel =>
                {
                    if (rel.TrackCount is null) return false;
                    if (rel.Mediums.Length == 0) return false;
                    // if (rel.Date?.Year is not null && rel.Date?.Year != ParsedTrack?.Year) return false;

                    return true;
                }) ?? []).ToArray();

        if (releases.Any(r => r.TrackCount == Files.Count))
        {
            releases = releases
                .Where(r => r.TrackCount == Files.Count)
                .ToArray();
        }

        foreach (var release in releases)
        {
            if (release.Mediums.Length == 0) continue;
            // if (release.Date?.Year is not null && release.Date?.Year != ParsedTrack?.Year) continue;

            foreach (var medium in release.Mediums)
            {
                if (medium.Tracks.Length == 0) continue;
                if (medium.Position != ParsedTrack?.AlbumNumber) continue;

                foreach (var track in medium.Tracks)
                {
                    if (track.Position != ParsedTrack?.TrackNumber) continue;

                    Release = release;

                    return true;
                }
            }
        }
        
        return false;
    }

    private async Task Init()
    {
        using AcoustIdFingerprintClient acoustIdFingerprintClient = new();
        FingerPrint = await acoustIdFingerprintClient.Lookup(InputFile);

        MediaAnalysis = await FFProbe.AnalyseAsync(InputFile);
        // int duration = (int)mediaAnalysis.Format.Duration.TotalSeconds;

        var match = Regex.Match(InputFile,
            "(?<library_folder>.+?)[\\\\\\/]((?<letter>.{1})?|\\[(?<type>.+?)\\])[\\\\\\/](?<artist>.+?)?[\\\\\\/]?(\\[(?<year>\\d{4})\\]?\\s?(?<album>.+?)?)[\\\\\\/]((?<album_number>(\\d{1,}))-)?(?<track_number>\\d{1,})\\s(?<title>.+?)(\\s\\[(?<artists>.+?)\\])?(\\s\\((?<featuring>.+?)\\))?(\\s\\((?<duplicate>\\d+)\\))?\\.(?<extension>mp3|flac|aac|m4a|wav)");

        ParsedTrack = new ParsedTrack
        {
            LibraryFolder = (match.Groups["library_folder"].Success ? match.Groups["library_folder"].Value : null) ??
                            string.Empty,
            Letter = match.Groups["letter"].Success ? match.Groups["letter"].Value : null,
            Type = match.Groups["type"].Success ? match.Groups["type"].Value : null,
            Artist = match.Groups["artist"].Success ? match.Groups["artist"].Value : null,
            Artists = match.Groups["artists"].Success ? match.Groups["artists"].Value : null,
            Album = match.Groups["album"].Success ? match.Groups["album"].Value : null,
            Year = match.Groups["year"].Success ? Convert.ToInt32(match.Groups["year"].Value) : 1970,
            AlbumNumber =
                match.Groups["album_number"].Success ? Convert.ToInt32(match.Groups["album_number"].Value) : 1,
            TrackNumber =
                match.Groups["track_number"].Success ? Convert.ToInt32(match.Groups["track_number"].Value) : 1,
            Title = (match.Groups["title"].Success ? match.Groups["title"].Value : null) ?? string.Empty,
            Featuring = match.Groups["featuring"].Success ? match.Groups["featuring"].Value : null,
            Duplicate = match.Groups["duplicate"].Success ? match.Groups["duplicate"].Value : null,
            Extension = match.Groups["extension"].Success ? match.Groups["extension"].Value : ""
        };

        FolderId = MediaContext.Folders
            .AsNoTracking()
            .First(e => InputFile.Contains(e.Path)).Id;

        FilePath = InputFile.Replace(Library.FolderLibraries
            .FirstOrDefault()?.Folder.Path ?? "", "");

        var reg = Regex.Match(FilePath, "(([\\\\\\/].+)([\\\\\\/].+))([\\\\\\/].+)");
        Folder = reg.Groups[1].Value.Replace("\\", "/");
        ArtistFolder = reg.Groups[2].Value.Replace("\\", "/");
        AlbumFolder = reg.Groups[3].Value.Replace("\\", "/");
        FileName = reg.Groups[4].Value.Replace("\\", "/");

        HostFolder = (Library.FolderLibraries
            .FirstOrDefault()?.Folder.Path + Folder).PathName();

        SanitizedFile = MyRegex()
            .Replace(InputFile.ToLower(), "")
            .RemoveDiacritics();

        SanitizedAlbumFolderName = MyRegex()
            .Replace(AlbumFolder.ToLower(), "")
            .RemoveDiacritics();

        SanitizedArtistFolderName = MyRegex()
            .Replace(ArtistFolder.ToLower(), "")
            .Replace("/", "")
            .RemoveDiacritics();

        SanitizedFileName = MyRegex()
            .Replace(FileName.ToLower(), "")
            .RemoveDiacritics();
        
        Files = FilterMusicFiles(Directory.GetFiles(HostFolder));

    }

    [GeneratedRegex("([^\\s\\\\\\/\\.a-zA-Z0-9-])")]
    private static partial Regex MyRegex();

    [GeneratedRegex("^00:")]
    private static partial Regex HmsRegex();

    private static string MakeArtistFolder(string artist)
    {
        var artistName = artist.RemoveDiacritics().RemoveNonAlphaNumericCharacters();

        var artistFolder = char.IsNumber(artistName[0])
            ? "#"
            : artistName[0].ToString().ToUpper();

        return $"/{artistFolder}/{artistName}";
    }

    private async Task StoreReleaseGroups(MusicBrainzReleaseAppends musicBrainzRelease)
    {
        ReleaseGroup releaseGroupInsert = new()
        {
            Id = musicBrainzRelease.MusicBrainzReleaseGroup.Id,
            Title = musicBrainzRelease.MusicBrainzReleaseGroup.Title,
            Description = musicBrainzRelease.MusicBrainzReleaseGroup.Disambiguation,
            Year = musicBrainzRelease.MusicBrainzReleaseGroup.FirstReleaseDate.ParseYear(),
            LibraryId = Library.Id,
        };

        try
        {
            await using MediaContext mediaContext = new();
            await mediaContext.ReleaseGroups.Upsert(releaseGroupInsert)
                .On(e => e.Id)
                .WhenMatched((s, i) => new ReleaseGroup
                {
                    UpdatedAt = DateTime.UtcNow,
                    Id = i.Id,
                    Title = i.Title,
                    Description = i.Description,
                    Year = i.Year,
                    LibraryId = i.LibraryId
                })
                .RunAsync();

        }
        catch (Exception e)
        {
            Logger.App(e, LogEventLevel.Error);
        }
    }

    private async Task StoreRelease(MusicBrainzReleaseAppends musicBrainzRelease)
    {
        var media = musicBrainzRelease.Media.FirstOrDefault(m => m.Tracks.Length > 0);
        if (media is null) return;

        var sanitizedAlbumName = MyRegex()
            .Replace(musicBrainzRelease.Title.ToLower(), "")
            .RemoveDiacritics();

        var shouldInsert = sanitizedAlbumName != "" && SanitizedAlbumFolderName.Contains(sanitizedAlbumName);
        
        await using MediaContext mediaContext = new();
        var hasAlbum = mediaContext.Albums
            .AsNoTracking()
            .Any(a => a.Id == musicBrainzRelease.Id && a.Cover != null);
        
        if (hasAlbum) return;

        Album albumInsert = new()
        {
            Id = musicBrainzRelease.Id,
            Name = musicBrainzRelease.Title,
            Country = musicBrainzRelease.Country,
            Disambiguation = musicBrainzRelease.Disambiguation,
            Year = musicBrainzRelease.DateTime?.ParseYear() ?? musicBrainzRelease.MusicBrainzReleaseGroup.FirstReleaseDate.ParseYear(),
            Tracks = media.Tracks.Length,
        };

        if (shouldInsert)
        {
            // albumInsert.LibraryId = Library.Id;
            albumInsert.FolderId = FolderId;
            albumInsert.Folder = Folder;
            albumInsert.HostFolder = HostFolder.PathName();
        }

        try
        {
            await mediaContext.Albums.Upsert(albumInsert)
                .On(e => e.Id)
                .WhenMatched((s, i) => new Album
                {
                    UpdatedAt = DateTime.UtcNow,
                    Id = i.Id,
                    Name = i.Name,
                    Disambiguation = i.Disambiguation,
                    Description = i.Description,
                    Year = i.Year,
                    Country = i.Country,
                    Tracks = i.Tracks,
                    
                    LibraryId = shouldInsert ? i.LibraryId : s.LibraryId,
                    Folder = shouldInsert ? i.Folder : s.Folder,
                    FolderId = shouldInsert ? i.FolderId : s.FolderId,
                    HostFolder = shouldInsert ? i.HostFolder : s.HostFolder
                })
                .RunAsync();
            
            var coverArtImageJob = new CoverArtImageJob(musicBrainzRelease);
            JobDispatcher.Dispatch(coverArtImageJob, "image", 3);
            
            var fanartImagesJob = new FanArtImagesJob(musicBrainzRelease);
            JobDispatcher.Dispatch(fanartImagesJob, "image", 2);
            
            // Networking.SendToAll("RefreshLibrary", "socket", new RefreshLibraryDto
            // {
            //     QueryKey = ["music", "album", albumInsert.Id.ToString()]
            // });

            await LinkReleaseToReleaseGroup(musicBrainzRelease);
        }
        catch (Exception e)
        {
            Logger.App(e, LogEventLevel.Error);
        }

    }

    private async Task StoreArtist(MusicBrainzArtist? artist)
    {
        if (artist == null) return;

        await using MediaContext mediaContext = new();
        
        var hasArtist = mediaContext.Artists
            .AsNoTracking()
            .Any(a => a.Id == artist.Id && a.Cover != null);
        
        if (hasArtist) return;
        
        Artist artistInsert = new()
        {
            Id = artist.Id,
            Name = artist.Name,
            Disambiguation = artist.Disambiguation,
            TitleSort = artist.SortName,
        };

        var artistFolder = MakeArtistFolder(artist.Name);

        artistInsert.Folder = artistFolder;
        artistInsert.HostFolder = Path.Join(Library.FolderLibraries.FirstOrDefault()?.Folder.Path, artistFolder).PathName();
        // artistInsert.LibraryId = Library.Id;
        artistInsert.FolderId = FolderId;

        try
        {
            await mediaContext.Artists.Upsert(artistInsert)
                .On(e => e.Id)
                .WhenMatched((s, i) => new Artist
                {
                    UpdatedAt = DateTime.UtcNow,
                    Id = i.Id,
                    Name = i.Name,
                    Disambiguation = i.Disambiguation,
                    Description = i.Description,
                    
                    Folder = i.Folder,
                    HostFolder = i.HostFolder,
                    // LibraryId = i.LibraryId,
                    FolderId = i.FolderId,
                })
                .RunAsync();
            
            var fanartImagesJob = new FanArtImagesJob(artist);
            JobDispatcher.Dispatch(fanartImagesJob, "image", 2);
            
            // Networking.SendToAll("RefreshLibrary", "socket", new RefreshLibraryDto
            // {
            //     QueryKey = ["music", "artist", artistInsert.Id.ToString()]
            // });
        }
        catch (Exception e)
        {
            Logger.App(e, LogEventLevel.Error);
        }
    }

    private async Task<MusicBrainzTrack?> StoreTrack(MusicBrainzReleaseAppends musicBrainzRelease, MusicBrainzTrack track, MusicBrainzMedia musicBrainzMedia)
    {
        await using MediaContext mediaContext = new();
        var hasTrack = mediaContext.Tracks
            .AsNoTracking()
            .Any(t => t.Id == track.Id && t.Filename != null && t.Duration != null);
        
        if (hasTrack) return null;
        var sanitizedTrackName = MyRegex()
            .Replace(track.Title.ToLower(), "")
            .RemoveDiacritics();
        
        var shouldInsert = sanitizedTrackName != "" && SanitizedFileName.Contains(sanitizedTrackName);

        Track trackInsert = new()
        {
            Id = track.Id,
            Name = track.Title,
            Date = musicBrainzRelease.DateTime ?? Music.FirstReleaseDate,
            DiscNumber = musicBrainzMedia.Position,
            TrackNumber = track.Position,
        };
        
        var file = FileMatch(musicBrainzRelease, musicBrainzMedia, trackInsert);
        
        if (file is not null && shouldInsert)
        {
            var mediaAnalysis = await FFProbe.AnalyseAsync(file);
            trackInsert.Filename = "/" + Path.GetFileName(file);
            trackInsert.Quality = (int)Math.Floor((mediaAnalysis.Format.BitRate) / 1000.0);
            trackInsert.Duration =
                HmsRegex().Replace((mediaAnalysis.Duration).ToString("hh\\:mm\\:ss"), "");
            
            trackInsert.FolderId = FolderId;
            trackInsert.Folder = Folder;
            trackInsert.HostFolder = HostFolder.PathName();
        }
        else
        {
            return null;
        }

        try
        {
            await mediaContext.Tracks.Upsert(trackInsert)
                .On(e => e.Id)
                .WhenMatched((ts, ti) => new Track
                {
                    UpdatedAt = DateTime.UtcNow,
                    Id = ti.Id,
                    Name = ti.Name,
                    DiscNumber = ti.DiscNumber,
                    TrackNumber = ti.TrackNumber,
                    Date = ti.Date,

                    Folder = ti.Folder,
                    FolderId = ti.FolderId,
                    HostFolder = ti.HostFolder,
                    Duration = ti.Duration,
                    Filename = ti.Filename,
                    Quality = ti.Quality
                })
                .RunAsync();

            return track;
        }
        catch (Exception e)
        {
            Logger.App(e, LogEventLevel.Error);
        }

        return null;
    }
    
    private string? FileMatch(MusicBrainzReleaseAppends musicBrainzRelease, MusicBrainzMedia musicBrainzMedia, Track trackInsert)
    {
        var file = FindTrackWithAlbumNumberByNumberPadded(musicBrainzMedia, null, musicBrainzRelease.Media.Length, 
            trackInsert.TrackNumber, 4);
        file = FindTrackWithAlbumNumberByNumberPadded(musicBrainzMedia, file, musicBrainzRelease.Media.Length, 
            trackInsert.TrackNumber, 3);
        file = FindTrackWithAlbumNumberByNumberPadded(musicBrainzMedia, file,musicBrainzRelease.Media.Length, 
            trackInsert.TrackNumber);

        file = FindTrackWithoutAlbumNumberByNumberPadded(musicBrainzMedia, file, musicBrainzRelease.Media.Length,
            trackInsert.TrackNumber, 4);
        file = FindTrackWithoutAlbumNumberByNumberPadded(musicBrainzMedia, file, musicBrainzRelease.Media.Length,
            trackInsert.TrackNumber, 3);
        file = FindTrackWithoutAlbumNumberByNumberPadded(musicBrainzMedia, file, musicBrainzRelease.Media.Length,
            trackInsert.TrackNumber);

        return file;
    }

    private string? FindTrackWithoutAlbumNumberByNumberPadded(MusicBrainzMedia musicBrainzMedia, string? file, int numberOfAlbums, int trackNumber, int padding = 2)
    {
        if (file is not null) return file;
        if (numberOfAlbums > 1) return file;

        return Files.FirstOrDefault(f =>
            {
                var fileName = Path.GetFileName(f).RemoveDiacritics().RemoveNonAlphaNumericCharacters().ToLower();
                
                var matchNumber = $"{trackNumber.ToString().PadLeft(padding, '0')} ";
                var matchString = musicBrainzMedia.Tracks[trackNumber - 1].Title
                    .RemoveDiacritics().RemoveNonAlphaNumericCharacters().ToLower();
                
                return fileName.StartsWith(matchNumber)
                       && fileName.Contains(matchString);
            });
    }

    private string? FindTrackWithAlbumNumberByNumberPadded(MusicBrainzMedia musicBrainzMedia, string? file, int numberOfAlbums, int trackNumber, int padding = 2)
    {
        if (file is not null) return file;
        if (numberOfAlbums == 1) return file;
        
        return Files.FirstOrDefault(f =>
            {
                var fileName = Path.GetFileName(f).RemoveDiacritics().RemoveNonAlphaNumericCharacters().ToLower();
                
                var matchNumber = $"{musicBrainzMedia.Position}-{trackNumber.ToString().PadLeft(padding, '0')} ";
                var matchString = musicBrainzMedia.Tracks[trackNumber - 1].Title
                    .RemoveDiacritics().RemoveNonAlphaNumericCharacters().ToLower();
                
                return fileName.StartsWith(matchNumber)
                       && fileName.Contains(matchString);
            });
    }
    
    private static List<string> FilterMusicFiles(IEnumerable<string> files)
    {
        string[] allowedExtensions = [".mp3", ".flac", ".wav", ".m4a"];
        
        List<string> result = [];
        foreach (var file in files)
        {
            if (allowedExtensions.Contains(Path.GetExtension(file)))
            {
                result.Add(file);
            }
        }
        return result;
    }

    private static async Task LinkReleaseToReleaseGroup(MusicBrainzReleaseAppends musicBrainzRelease)
    {
        AlbumReleaseGroup albumReleaseGroup = new()
        {
            AlbumId = musicBrainzRelease.Id,
            ReleaseGroupId = musicBrainzRelease.MusicBrainzReleaseGroup.Id
        };

        await using MediaContext mediaContext = new();
        await mediaContext.AlbumReleaseGroup.Upsert(albumReleaseGroup)
            .On(e => new { e.AlbumId, e.ReleaseGroupId })
            .WhenMatched((s, i) => new AlbumReleaseGroup
            {
                AlbumId = i.AlbumId,
                ReleaseGroupId = i.ReleaseGroupId
            })
            .RunAsync();
    }

    private static async Task LinkArtistToReleaseGroup(MusicBrainzReleaseAppends musicBrainzRelease, Guid artistId)
    {
        ArtistReleaseGroup artistReleaseGroup = new()
        {
            ArtistId = artistId,
            ReleaseGroupId = musicBrainzRelease.MusicBrainzReleaseGroup.Id
        };

        await using MediaContext mediaContext = new();
        await mediaContext.ArtistReleaseGroup.Upsert(artistReleaseGroup)
            .On(e => new { e.ArtistId, e.ReleaseGroupId })
            .WhenMatched((s, i) => new ArtistReleaseGroup
            {
                ArtistId = i.ArtistId,
                ReleaseGroupId = i.ReleaseGroupId
            })
            .RunAsync();
    }

    private async Task LinkReleaseToLibrary(MusicBrainzReleaseAppends musicBrainzRelease)
    {
        AlbumLibrary albumLibrary = new()
        {
            AlbumId = musicBrainzRelease.Id,
            LibraryId = Library.Id
        };

        await using MediaContext mediaContext = new();
        await mediaContext.AlbumLibrary.Upsert(albumLibrary)
            .On(e => new { e.AlbumId, e.LibraryId })
            .WhenMatched((s, i) => new AlbumLibrary
            {
                AlbumId = i.AlbumId,
                LibraryId = i.LibraryId
            })
            .RunAsync();
    }

    private async Task LinkArtistToLibrary(MusicBrainzArtist musicBrainzArtistMusicBrainzArtist)
    {
        ArtistLibrary artistLibrary = new()
        {
            ArtistId = musicBrainzArtistMusicBrainzArtist.Id,
            LibraryId = Library.Id
        };

        await using MediaContext mediaContext = new();
        await mediaContext.ArtistLibrary.Upsert(artistLibrary)
            .On(e => new { e.ArtistId, e.LibraryId })
            .WhenMatched((s, i) => new ArtistLibrary
            {
                ArtistId = i.ArtistId,
                LibraryId = i.LibraryId
            })
            .RunAsync();
    }

    private static async Task LinkTrackToRelease(MusicBrainzTrack? track, MusicBrainzReleaseAppends? release)
    {
        if (track == null || release == null) return;

        AlbumTrack albumTrack = new()
        {
            AlbumId = release.Id,
            TrackId = track.Id
        };

        await using MediaContext mediaContext = new();
        await mediaContext.AlbumTrack.Upsert(albumTrack)
            .On(e => new { e.AlbumId, e.TrackId })
            .WhenMatched((s, i) => new AlbumTrack
            {
                AlbumId = i.AlbumId,
                TrackId = i.TrackId
            })
            .RunAsync();

        // Networking.SendToAll("RefreshLibrary", "socket", new RefreshLibraryDto
        // {
        //     QueryKey = ["music", "album", albumTrack.AlbumId.ToString()]
        // });
    }

    private static async Task LinkArtistToAlbum(MusicBrainzArtist musicBrainzArtistMusicBrainzArtist, MusicBrainzReleaseAppends musicBrainzRelease)
    {
        AlbumArtist albumArtist = new()
        {
            AlbumId = musicBrainzRelease.Id,
            ArtistId = musicBrainzArtistMusicBrainzArtist.Id
        };

        await using MediaContext mediaContext = new();
        await mediaContext.AlbumArtist.Upsert(albumArtist)
            .On(e => new { e.AlbumId, e.ArtistId })
            .WhenMatched((s, i) => new AlbumArtist
            {
                AlbumId = i.AlbumId,
                ArtistId = i.ArtistId
            })
            .RunAsync();

        // Networking.SendToAll("RefreshLibrary", "socket", new RefreshLibraryDto
        // {
        //     QueryKey = ["music", "artist", albumArtist.ArtistId.ToString()]
        // });
    }

    private static async Task LinkArtistToTrack(MusicBrainzArtist musicBrainzArtistMusicBrainzArtist, MusicBrainzTrack track)
    {
        ArtistTrack artistTrack = new()
        {
            ArtistId = musicBrainzArtistMusicBrainzArtist.Id,
            TrackId = track.Id
        };

        await using MediaContext mediaContext = new();
        await mediaContext.ArtistTrack.Upsert(artistTrack)
            .On(e => new { e.ArtistId, e.TrackId })
            .WhenMatched((s, i) => new ArtistTrack
            {
                ArtistId = i.ArtistId,
                TrackId = i.TrackId
            })
            .RunAsync();

        // Networking.SendToAll("RefreshLibrary", "socket", new RefreshLibraryDto
        // {
        //     QueryKey = ["music", "artist", artistTrack.ArtistId.ToString()]
        // });
    }

    private Task DispatchJobs()
    {
        return Task.CompletedTask;
    }

    ~MusicLogic2()
    {
        DisposeAsync().AsTask().Wait();
    }

    public ValueTask DisposeAsync()
    {
        FingerPrint = default;
        ParsedTrack = default;
        MediaAnalysis = default;

        return ValueTask.CompletedTask;
    }
}