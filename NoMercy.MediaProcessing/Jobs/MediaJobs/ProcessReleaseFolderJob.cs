// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.NmSystem;
using NoMercy.Providers.AcoustId.Client;
using NoMercy.Providers.AcoustId.Models;
using NoMercy.Providers.MusicBrainz.Client;
using NoMercy.Providers.MusicBrainz.Models;

namespace NoMercy.MediaProcessing.Jobs.MediaJobs;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[Serializable]
public partial class ProcessReleaseFolderJob : AbstractMusicFolderJob
{
    public override string QueueName => "queue";
    public override int Priority => 10;
    
    private bool _fromFingerprint = false;
    
    public override async Task Handle()
    {
        await using MediaContext context = new();
        JobDispatcher jobDispatcher = new();

        Library albumLibrary = await context.Libraries
            .Where(f => f.Id == LibraryId)
            .Include(f => f.FolderLibraries)
            .ThenInclude(f => f.Folder)
            .FirstAsync();

        Logger.App("Processing music folder " + FilePath);

        await using MediaScan mediaScan = new();
        ConcurrentBag<MediaFolderExtend> rootFolders = await mediaScan
            .DisableRegexFilter()
            .EnableFileListing()
            .Process(FilePath, 2);

        if (rootFolders.Count == 0)
        {
            Logger.App("No folders found in " + FilePath);
            return;
        }
        
        await ScanForReleases(rootFolders, albumLibrary, jobDispatcher);
    }

    private async Task ScanForReleases(
        ConcurrentBag<MediaFolderExtend> rootFolders, 
        Library albumLibrary, 
        JobDispatcher jobDispatcher
    )
    {
        using MusicBrainzReleaseClient musicBrainzReleaseClient = new();
        using MusicBrainzRecordingClient musicBrainzRecordingClient = new();
        foreach (MediaFolderExtend folder in rootFolders)
        {
            if(folder?.Files is null || folder.Files.IsEmpty || folder.Files.Count == 0)
            {
                continue;
            }
            
            Init(folder,
                out string artistName, 
                out string albumName, 
                out int year, 
                out string releaseType, 
                out string libraryFolder);
            
            if (!BaseFolderCheck(albumLibrary, libraryFolder, folder, out Folder baseFolder)) continue;
            
            Logger.App("Processing: " + baseFolder.Path + " - " + libraryFolder + " - " + artistName + " - " + albumName + " - " + year + " - " + releaseType);

            if(releaseType == "Singles")
            {
                Logger.App("Singles: " + folder.Path);
                continue;
            }
            
            MusicBrainzRelease? release = await MusicBrainzRelease(musicBrainzReleaseClient, musicBrainzRecordingClient, folder, albumName, artistName, year);

            if (release is null) continue;
            
            Logger.App("Matched: " + release.Title + " - " + release.Id);
            jobDispatcher.DispatchJob<AddReleaseJob>(LibraryId, release.Id, baseFolder, folder);
        }
    }

    private bool BaseFolderCheck(Library albumLibrary, string libraryFolder, MediaFolderExtend folder,
        out Folder baseFolder)
    {
        Folder? foundBaseFolder = albumLibrary.FolderLibraries
            .Select(folderLibrary => folderLibrary.Folder)
            .FirstOrDefault(f => f.Path == libraryFolder || f.Path == folder.Path);

        if (foundBaseFolder is null)
        {
            Logger.App("No base folder found for: " + folder.Path);
            baseFolder = null;
            return false;
        }
        baseFolder = foundBaseFolder;
        return true;
    }

    private async Task<MusicBrainzRelease?> MusicBrainzRelease(MusicBrainzReleaseClient musicBrainzReleaseClient,
        MusicBrainzRecordingClient musicBrainzRecordingClient, MediaFolderExtend folder, string albumName,
        string artistName, int year)
    {
        MusicBrainzRelease[] musicBrainzReleases = await SearchReleaseByQuery(
            musicBrainzReleaseClient, 
            musicBrainzRecordingClient, 
            folder, 
            albumName, 
            artistName, 
            year);

        MusicBrainzRelease? bestMatchedRelease = await GetBestMatchedRelease(
            musicBrainzReleaseClient, 
            folder, 
            albumName, 
            musicBrainzReleases);

        MusicBrainzRelease? realRelease = await CheckBestRelease(
            musicBrainzReleaseClient, 
            musicBrainzRecordingClient,
            folder,
            musicBrainzReleases,
            bestMatchedRelease,
            albumName);

        return realRelease;
    }

    private async Task<MusicBrainzRelease?> CheckBestRelease(
        MusicBrainzReleaseClient musicBrainzReleaseClient,
        MusicBrainzRecordingClient musicBrainzRecordingClient,
        MediaFolderExtend folder,
        MusicBrainzRelease[] musicBrainzReleases,
        MusicBrainzRelease? bestMatchedRelease,
        string albumName)
    {
        if (bestMatchedRelease is not null) return bestMatchedRelease;
        if (!_fromFingerprint)
        {
            MusicBrainzRelease[] releases = await SearchRecordingByFingerprint(musicBrainzRecordingClient, folder, albumName);
            musicBrainzReleases = releases.Concat(musicBrainzReleases).ToArray();
            
            bestMatchedRelease = await GetBestMatchedRelease(
                musicBrainzReleaseClient, 
                folder, 
                albumName, 
                musicBrainzReleases);
            
            _fromFingerprint = true;
            
            return await CheckBestRelease(
                musicBrainzReleaseClient, 
                musicBrainzRecordingClient, 
                folder, 
                musicBrainzReleases, 
                bestMatchedRelease, 
                albumName);
        }
        
        // If no match found
        // Log and return null
        // This will prevent the job from being dispatched
        
        // Maybe we can alert the user for a manual check
        
        Logger.App("No match found for: " + folder.Path);
        return null;
    }

    private static void Init(MediaFolderExtend folder,
        out string artistName,
        out string albumName,
        out int year,
        out string releaseType,
        out string libraryFolder)
    {
        char separator = Path.DirectorySeparatorChar;
        string pattern = $@"\{separator}#\{separator}|\{separator}\[Other\]\{separator}|\{separator}\[Soundtrack\]\{separator}|\{separator}\[Various Artists\]\{separator}|\{separator}[A-Z]\{separator}";
        string rawFolderName = Regex.Replace(folder.Name, @"\[\d{4}\]\s?","");
        Match match = PathRegex().Match(folder.Path);
        artistName = match.Groups["artist"].Success ? match.Groups["artist"].Value : string.Empty;
        albumName = match.Groups["album"].Success ? match.Groups["album"].Value : rawFolderName;
        year = match.Groups["year"].Success ? Convert.ToInt32(match.Groups["year"].Value) : 0;
        releaseType = match.Groups["releaseType"].Success ? match.Groups["releaseType"].Value : string.Empty;
        libraryFolder = (match.Groups["library_folder"].Success ? match.Groups["library_folder"].Value : null) ?? 
                        Regex.Split(folder.Path, pattern)?[0] ?? string.Empty;
    }

    private async Task<MusicBrainzRelease?> GetBestMatchedRelease(
        MusicBrainzReleaseClient musicBrainzReleaseClient, 
        MediaFolderExtend folder, 
        string albumName, 
        MusicBrainzRelease[] matchedReleases)
    {
        int highestScore = 0;
        MusicBrainzRelease? bestRelease = null;
        if (matchedReleases.Length == 0) return null;
        foreach (MusicBrainzRelease? release in matchedReleases)
        {
            if (release.Title.Sanitize() != albumName.Sanitize()
                && release.Title.Sanitize() != "Various Artists"
                && release.Title.Sanitize() != "Soundtracks"
                && release.Title.Sanitize() != "Other") continue;
            
            MusicBrainzReleaseAppends? result = await musicBrainzReleaseClient.WithAppends(release.Id, ["recordings"]);
            if (result?.Media is null) continue;
            int score = CalculateMatchScore(result, folder?.Files ?? []);

            if (score <= highestScore) continue;
            highestScore = score;
            bestRelease = release;
        }

        return bestRelease;
    }
        
    private int CalculateMatchScore(MusicBrainzRelease release, ConcurrentBag<MediaFile> localTracks)
    {
        int score = 0;
        
        if (release?.Media is null) return score;
        foreach (MusicBrainzMedia medium in release.Media)
        {
            if (medium?.Tracks is null) continue;
            foreach (MusicBrainzTrack track in medium.Tracks)
            {
                if (track?.Title is null) continue;
                MediaFile? matchingLocalTrack = localTracks.FirstOrDefault(t =>
                    CompareTrackName(t, track)
                    || CompareTrackNumber(t, track)
                    || CompareTrackDuration(t,track));

                if (matchingLocalTrack != null)
                {
                    score++;
                }
            }
        }

        return score;
    }

    private bool CompareTrackDuration(MediaFile mediaFile, MusicBrainzTrack track)
    {
        return Math.Abs(track.Duration - mediaFile.FFprobe!.Duration.TotalSeconds) < 10;
    }

    private bool CompareTrackNumber(MediaFile mediaFile, MusicBrainzTrack track)
    {
        if (mediaFile.Parsed?.TrackNumber is null) return false;
        return mediaFile.Parsed.TrackNumber == track.Position;
    }

    private static bool CompareTrackName(MediaFile t, MusicBrainzTrack track)
    {
        string title = t.Parsed?.Title ?? string.Empty;
        if (string.IsNullOrEmpty(title)) title = Path.GetFileNameWithoutExtension(t.Name);
        title = Regex.Replace(title, @"^\d+((-|\s)\d+)?","");
        
        return title.ContainsSanitized(track.Title);
    }

    private async Task<MusicBrainzRelease[]> SearchRecordingByFingerprint(
        MusicBrainzRecordingClient musicBrainzRecordingClient, 
        MediaFolderExtend folder,
        string albumName)
    {
        List<MusicBrainzRelease> musicBrainzReleases = [];
        
        List<MusicBrainzRelease> matchedReleases = [];
        Logger.App("Fingerprinting: " + folder.Path);
        foreach (MediaFile file in folder?.Files ?? [])
        {
            if (file.FFprobe is null) continue;
            string title = Regex.Replace(
                Path.GetFileNameWithoutExtension(file.Name),
                @"^\d+((-|\s)\d+)?\s+?",
                "");
            
            matchedReleases = await Fingerprint(file, albumName, matchedReleases);
            musicBrainzReleases.AddRange(matchedReleases);
            
            if (!string.IsNullOrEmpty(title))
            {
                MusicBrainzRecordingAppends? brainzRecordingAppends = await musicBrainzRecordingClient.SearchRecordings(title);
                if (brainzRecordingAppends?.Releases is not null && brainzRecordingAppends.Releases.Length > 0)
                    musicBrainzReleases.AddRange(brainzRecordingAppends.Releases);
            }
        }
        
        return musicBrainzReleases.DistinctBy(r => r.Id).ToArray();
    }
    
    private async Task<List<MusicBrainzRelease>> Fingerprint(
        MediaFile file, 
        string title, 
        List<MusicBrainzRelease> matchedReleases,
        int retry = 0)
    {
        _fromFingerprint = true;
        AcoustIdFingerprint? fingerprint = null;
            
        if (string.IsNullOrEmpty(title)) return [];
        try
        {
            fingerprint = await AcoustIdFingerprintLookUp(file.Path);
        }
        catch (Exception e)
        {
            if (retry == 3)
            {
                Logger.App("Fingerprint Error: " + e.Message);
                return matchedReleases;
            }
            await Task.Delay(200);
            retry += 1;
            return await Fingerprint(file, title, matchedReleases, retry);
        }
        
        if (fingerprint is null)
        {
            Logger.App("Fingerprint Error: " + file.Path);
            return matchedReleases;
        }
        
        foreach (AcoustIdFingerprintResult fingerprintResult in fingerprint?.Results ?? [])
        {
            if (fingerprintResult.Id == Guid.Empty) continue;
            
            foreach (AcoustIdFingerprintRecording? fingerprintResultRecording in fingerprintResult?.Recordings ?? [])
            {
                if (fingerprintResultRecording?.Releases is null) continue;
                if (fingerprintResultRecording.Id == Guid.Empty) continue;
                
                AcoustIdFingerprintReleaseGroups[] fingerprintReleases = fingerprintResultRecording.Releases ?? [];
                foreach (AcoustIdFingerprintReleaseGroups fingerprintRelease in fingerprintReleases)
                {
                    if (fingerprintRelease.Id == Guid.Empty) continue;
                    if (matchedReleases.Any(r => r.Id == fingerprintRelease.Id)) continue;
                    matchedReleases.Add(new MusicBrainzRelease
                    {
                        Id = fingerprintRelease.Id,
                        Title = fingerprintRelease.Title ?? title,
                    });
                    Logger.App("Matched Fingerprint Release: " + fingerprintRelease.Title + " - " + fingerprintRelease.Id);
                }
            }
        }

        return matchedReleases;
    }
    
    private async Task<AcoustIdFingerprint?> AcoustIdFingerprintLookUp(string path, int retry = 0)
    {
        AcoustIdFingerprintClient acoustIdFingerprintClient = new();
        AcoustIdFingerprint? result = null;
        try
        {
            result = await acoustIdFingerprintClient.Lookup(path);
        }
        catch (Exception e)
        {
            if (retry == 3)
            {
                Logger.App("Fingerprint Error: " + e.Message);
                return null;
            }
            await Task.Delay(200);
            retry += 1;
            return await AcoustIdFingerprintLookUp(path, retry);
        }
        return result;
    }
    
    private async Task<MusicBrainzRelease[]> SearchReleaseByQuery(
        MusicBrainzReleaseClient musicBrainzReleaseClient, 
        MusicBrainzRecordingClient musicBrainzRecordingClient,
        MediaFolderExtend folder,
        string albumName, 
        string artistName, 
        int year)
    {
        List<MusicBrainzRelease> musicBrainzReleases = [];
        MusicBrainzRelease[] releases;
        string query = $" release:{albumName}";
        MusicBrainzReleaseSearchResponse? result = null;
        if (!string.IsNullOrEmpty(artistName))
        {
            string yearQuery = year > 0 ? $" year:{year}" : "";
            result = await musicBrainzReleaseClient.SearchReleases($"artist:{artistName}{query}{yearQuery}");
            if (result?.Releases != null)
            {
                releases = result.Releases.Where(r => r.Title.ContainsSanitized(albumName))
                    .DistinctBy(r => r.Id).ToArray();
                musicBrainzReleases.AddRange(releases);
            }
        }

        if (musicBrainzReleases.Count == 0 && !string.IsNullOrEmpty(artistName))
        {
            result = await musicBrainzReleaseClient.SearchReleases($"artist:{artistName}{query}");
            if (result?.Releases != null)
            {
                releases = result.Releases.Where(r => r.Title.ContainsSanitized(albumName)).ToArray();
                musicBrainzReleases.AddRange(releases.DistinctBy(r => r.Id));
            }
        }
        
        if (musicBrainzReleases.Count == 0)
        {
            result = await musicBrainzReleaseClient.SearchReleases(albumName);
            if (result?.Releases != null)
            {
                releases = result.Releases.Where(r => r.Title.ContainsSanitized(albumName)).ToArray();
                musicBrainzReleases.AddRange(releases.DistinctBy(r => r.Id));
            }
        }
                     
        if (musicBrainzReleases.Count == 0)
        {
            releases = await SearchRecordingByFingerprint(musicBrainzRecordingClient, folder, albumName);
            musicBrainzReleases.AddRange(releases);
        }
        
        MusicBrainzRelease[] matchedReleases = musicBrainzReleases
            .Where(r => r.Title.ContainsSanitized(albumName))
            .DistinctBy(r => r.Id).ToArray();
        musicBrainzReleases.Clear();
        return matchedReleases;
    }
    
    [GeneratedRegex(@"(?<library_folder>.+?)[\\\/]((?<letter>.{1})?|\[(?<type>.+?)\])[\\\/](?<artist>.+?)?[\\\/]?(\[(?<year>\d{4})\]|\[(?<releaseType>Singles)\])\s?(?<album>.*)?")]
    private static partial Regex PathRegex();
}