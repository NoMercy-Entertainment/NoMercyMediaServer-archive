// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.MediaProcessing.Releases;
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

    public override async Task Handle()
    {
        await using MediaContext context = new();
        JobDispatcher jobDispatcher = new();

        ReleaseRepository releaseRepository = new(context);
        ReleaseManager releaseManager = new(releaseRepository, jobDispatcher);

        Library albumLibrary = await context.Libraries
            .Where(f => f.Id == LibraryId)
            .Include(f => f.FolderLibraries)
            .ThenInclude(f => f.Folder)
            .FirstAsync();

        Logger.App("Processing music folder " + FilePath);

        await using MediaScan mediaScan = new();
        List<MediaFolderExtend> rootFolders = (await mediaScan
                .DisableRegexFilter()
                .EnableFileListing()
                .Process(FilePath, 2))
            .ToList();
        
        if (rootFolders.Count == 0) return;

        using MusicBrainzReleaseClient musicBrainzReleaseClient = new();

        foreach (MediaFolderExtend folder in rootFolders)
        {
            if(folder.Files?.Count == 0) continue;
            
            Match match = PathRegex().Match(folder.Path);
            string artistName = match.Groups["artist"].Success ? match.Groups["artist"].Value : string.Empty;
            string albumName = match.Groups["album"].Success ? match.Groups["album"].Value : string.Empty;
            int year = match.Groups["year"].Success ? Convert.ToInt32(match.Groups["year"].Value) : 0;
            string type2 = match.Groups["type2"].Success ? match.Groups["type2"].Value : string.Empty;
            string libraryFolder = (match.Groups["library_folder"].Success ? match.Groups["library_folder"].Value : null) ?? string.Empty;

            Folder? baseFolder = albumLibrary.FolderLibraries
                .Select(folderLibrary => folderLibrary.Folder)
                .FirstOrDefault(f => f.Path == libraryFolder);
            
            if (baseFolder is null) continue;
            
            if (string.IsNullOrEmpty(artistName) && string.IsNullOrEmpty(albumName) && year == 0 && string.IsNullOrEmpty(type2))
            {
                Logger.App("No match for: " + folder.Path);
                continue;
            }
            
            if(type2 == "Singles")
            {
                // TODO: handle ["Singles"]

                Logger.App("Singles: " + folder.Path);
                continue;
            }
            
            List<string> queryStringBuilder = [];
            if (!string.IsNullOrEmpty(artistName)) queryStringBuilder.Add($"artist:{artistName}");
            if (!string.IsNullOrEmpty(albumName)) queryStringBuilder.Add($"release:{albumName}");
            if (year != 0) queryStringBuilder.Add($"date:{year}");
            
            string query = string.Join("+", queryStringBuilder);

            MusicBrainzReleaseSearchResponse? releases = await musicBrainzReleaseClient.SearchReleases(query);

            // When the strict match fails to include the desired item we fall back to a more global search
            if (releases is null || !releases.Releases.Any(r => r.Title.Sanitize().Contains(albumName.Sanitize())))
            {
                List<string> queryStringBuilder2 = [];
                if (!string.IsNullOrEmpty(albumName)) queryStringBuilder2.Add($"release:{albumName}");
            
                string query2 = string.Join("+", queryStringBuilder2);

                releases = await musicBrainzReleaseClient.SearchReleases(query2);
            }
            
            if (releases is null) continue;

            List<MusicBrainzRelease?> matchedReleases = [];
            
            foreach (MusicBrainzRelease _release in releases.Releases)
            {
                if (_release.Title.Sanitize() != albumName.Sanitize()
                    && _release.Title.Sanitize() != "Various Artists"
                    && _release.Title.Sanitize() != "Soundtracks"
                    && _release.Title.Sanitize() != "Other") continue;
                
                if(_release.DateTime is not null && _release.DateTime.Value.Year != year) continue;
                
                matchedReleases.Add(_release);
            }

            if (matchedReleases.Count == 0)
            {
                foreach (MediaFile file in folder.Files ?? [])
                {
                    string title = Regex.Replace(Path.GetFileNameWithoutExtension(file.Name), @"^\d+((-|\s)\d+)?","");
                    
                    AcoustIdFingerprintClient acoustIdFingerprintClient = new();
                    AcoustIdFingerprint? fingerprint = await acoustIdFingerprintClient.Lookup(file.Path);
                    if (fingerprint is not null)
                    {
                        if (fingerprint.Results.Length == 0) continue;
                        AcoustIdFingerprintResult? acoustIdFingerprintResult = fingerprint.Results
                            .FirstOrDefault(print => print.Recordings != null && (bool)(print.Recordings ?? [])?
                                .Any(rec => rec?.Title != null && albumName.Sanitize().Contains(rec.Title?.Sanitize() ?? string.Empty)));
                        
                        if (acoustIdFingerprintResult is null)
                        {
                            acoustIdFingerprintResult = fingerprint.Results
                                .FirstOrDefault(print => print.Recordings != null && (bool)(print.Recordings ?? [])?
                                    .Any(rec => rec?.Title != null && title.Sanitize()
                                        .Contains(rec.Title?.Sanitize() ?? string.Empty)));
                        }

                        Guid id = acoustIdFingerprintResult?.Recordings?.FirstOrDefault()?.Releases?.FirstOrDefault()?.Id ?? Guid.Empty;
                        if(id != Guid.Empty)
                        {
                            matchedReleases.Add(new MusicBrainzRelease
                            {
                                Id = id,
                                Title = acoustIdFingerprintResult?.Recordings?.FirstOrDefault()?.Releases?.FirstOrDefault()?.Title ?? string.Empty,
                            });
                        }
                        else
                        {
                            if (matchedReleases.Count == 0)
                            {
                                Logger.App("No match for: " + folder.Path);
                            }
                        }
                    } 
                    // raw save
                }
            }
            
            MusicBrainzRelease? bestRelease = null;
            int highestScore = -1;
            
            foreach (MusicBrainzRelease? release in matchedReleases.Where(r => r.Title.Sanitize().Contains(albumName.Sanitize()) && r.Media is null))
            {
                // if (release is null || release.Media.Length == 0) continue;
                if (release is null) continue;

                var result = await musicBrainzReleaseClient.WithAppends(release.Id, ["recordings"]);
                if (result?.Media is null) continue;
                int score = CalculateMatchScore(result,  folder.Files ?? []);

                if (score > highestScore)
                {
                    highestScore = score;
                    bestRelease = release; // 
                }
            }
            
            if (bestRelease is not null)
            {
                Logger.App("Matched: " + folder.Path + " - " + bestRelease.Title);
                
                jobDispatcher.DispatchJob<AddReleaseJob>(LibraryId, bestRelease.Id, baseFolder, folder);
            }
            else
            {
                Logger.App("No match for: " + folder.Path);
                // fingerprint
                // if finrgerprint is not null
                // else raw save
            }
        }
    }

    private int CalculateMatchScore(MusicBrainzRelease release, ConcurrentBag<MediaFile> localTracks)
    {
        int score = 0;
        
        foreach (var medium in release.Media)
        {
            foreach (var track in medium?.Tracks ?? [])
            {  
                var matchingLocalTrack = localTracks.FirstOrDefault(t => 
                    t.Parsed!.Title!.Sanitize().Contains(track.Title.Sanitize()) && Math.Abs(track.Duration - t.FFprobe!.Duration.TotalSeconds) < 10);

                if (matchingLocalTrack != null)
                {
                    score++;
                }
            }
        }

        return score;
    }
    [GeneratedRegex(
        @"(?<library_folder>.+?)[\\\/]((?<letter>.{1})?|\[(?<type>.+?)\])[\\\/](?<artist>.+?)?[\\\/]?(\[(?<year>\d{4})\]|\[(?<type2>Singles)\])\s?(?<album>.*)?")]
    private static partial Regex PathRegex();

}