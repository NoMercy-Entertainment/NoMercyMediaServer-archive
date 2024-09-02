// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------

using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.MediaProcessing.Releases;
using NoMercy.NmSystem;
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
    public override int Priority => 5;

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
        List<MediaFolder> rootFolders = (await mediaScan
                .DisableRegexFilter()
                .EnableFileListing()
                .Process(FilePath, 2))
            .ToList();
        
        if (rootFolders.Count == 0) return;

        using MusicBrainzReleaseClient musicBrainzReleaseClient = new();

        foreach (MediaFolder folder in rootFolders)
        {
            Match match = PathRegex().Match(folder.Path);

            string ArtistName = match.Groups["artist"].Success ? match.Groups["artist"].Value : string.Empty;
            string AlbumName = match.Groups["album"].Success ? match.Groups["album"].Value : string.Empty;
            int Year = match.Groups["year"].Success ? Convert.ToInt32(match.Groups["year"].Value) : 0;

            // if (string.IsNullOrEmpty(ArtistName) || string.IsNullOrEmpty(AlbumName)) return;
            
            List<string> queryStringBuilder = [];
            if (!string.IsNullOrEmpty(ArtistName)) queryStringBuilder.Add($"artist:{ArtistName}");
            if (!string.IsNullOrEmpty(AlbumName)) queryStringBuilder.Add($"release:{AlbumName}");
            if (Year != 0) queryStringBuilder.Add($"date:{Year}");
            
            string query = string.Join("+", queryStringBuilder);

            MusicBrainzReleaseSearchResponse? releases = await musicBrainzReleaseClient.SearchReleases(query);
            if (releases is null) return;

            List<MusicBrainzRelease?> matchedReleases = [];

            foreach (MusicBrainzRelease _release in releases.Releases)
            {
                if (_release.Title.Sanitize() != AlbumName.Sanitize()
                    && _release.Title.Sanitize() != "Various Artists"
                    && _release.Title.Sanitize() != "Soundtracks"
                    && _release.Title.Sanitize() != "Other") continue;
                
                if(_release.DateTime is not null && _release.DateTime.Value.Year != Year) continue;
                
                matchedReleases.Add(_release);
            }

            if (matchedReleases.Count == 0) return;

            foreach (MusicBrainzRelease? release in matchedReleases)
            {
                if (release is null || release.Media.Length == 0) continue;

                var result = await musicBrainzReleaseClient.WithAllAppends(release.Id);
                if (result?.Media is null) continue;

                var tracks = result.Media.SelectMany(m => m.Tracks).ToList();
                // if (tracks.Count != (folder.Files ?? []).Count) continue;

                foreach (MediaFile file in folder.Files ?? [])
                {
                    if (file.Parsed?.Title is null) continue;
                    
                    var track = tracks.FirstOrDefault(t => file.Parsed.Title.Sanitize().Contains(t.Title.Sanitize()));
                    if (track is null) continue;

                    if (!FileMatch(file, result, result.Media.First(), track.Position))
                    {
                        // fingerprint
                        Logger.App("Fingerprinting " + file.Path);
                        continue;
                    }

                    Logger.App(new 
                    {
                        File = file.Path,
                        AlbumName = result.Title,
                        Artist = track.ArtistCredit.FirstOrDefault()?.Name,
                        AlbumNumber = result.Media.First().Position,
                        TrackNumber = track.Position,
                        Track = track.Title,
                    });
                    
                    Logger.App("Matched " + file.Path + " to " + track.ArtistCredit.FirstOrDefault()?.Name + " _ " + track.Title);
                }
            }

            // AcoustIdFingerprintClient acoustIdFingerprintClient = new();
            // await acoustIdFingerprintClient.Lookup(file.Path);
            // acoustIdFingerprintClient.Dispose();
        }
    }

    [GeneratedRegex(
        @"(?<library_folder>.+?)[\\\/]((?<letter>.{1})?|\[(?<type>.+?)\])[\\\/](?<artist>.+?)?[\\\/]?(\[(?<year>\d{4})\]?\s?(?<album>.*)?)")]
    private static partial Regex PathRegex();

    private bool FileMatch(MediaFile inputFile, MusicBrainzReleaseAppends musicBrainzRelease,
        MusicBrainzMedia musicBrainzMedia, int trackNumber)
    {
        bool hasMatch = FindTrackWithAlbumNumberByNumberPadded(inputFile, musicBrainzMedia, false,
            musicBrainzRelease.Media.Length,
            trackNumber, 4);
        hasMatch = FindTrackWithAlbumNumberByNumberPadded(inputFile, musicBrainzMedia, hasMatch,
            musicBrainzRelease.Media.Length,
            trackNumber, 3);
        hasMatch = FindTrackWithAlbumNumberByNumberPadded(inputFile, musicBrainzMedia, hasMatch,
            musicBrainzRelease.Media.Length,
            trackNumber);

        hasMatch = FindTrackWithoutAlbumNumberByNumberPadded(inputFile, musicBrainzMedia, hasMatch,
            musicBrainzRelease.Media.Length,
            trackNumber, 4);
        hasMatch = FindTrackWithoutAlbumNumberByNumberPadded(inputFile, musicBrainzMedia, hasMatch,
            musicBrainzRelease.Media.Length,
            trackNumber, 3);
        hasMatch = FindTrackWithoutAlbumNumberByNumberPadded(inputFile, musicBrainzMedia, hasMatch,
            musicBrainzRelease.Media.Length,
            trackNumber);

        return hasMatch;
    }

    private bool FindTrackWithoutAlbumNumberByNumberPadded(MediaFile inputFile, MusicBrainzMedia musicBrainzMedia,
        bool hasMatch,
        int numberOfAlbums, int trackNumber, int padding = 2)
    {
        if (hasMatch) return true;
        if (numberOfAlbums > 1) return false;
        if (inputFile.Parsed is null) return false;

        string fileName = Path.GetFileName(inputFile.Parsed.FilePath).RemoveDiacritics()
            .RemoveNonAlphaNumericCharacters()
            .ToLower();

        string matchNumber = $"{trackNumber.ToString().PadLeft(padding, '0')} ";
        if (musicBrainzMedia.Tracks.Length < trackNumber) return false;
        string matchString = musicBrainzMedia.Tracks[trackNumber - 1].Title
            .RemoveDiacritics().RemoveNonAlphaNumericCharacters().ToLower().Replace(".mp3", "");

        return fileName.StartsWith(matchNumber)
               && fileName.Contains(matchString);
    }

    private bool FindTrackWithAlbumNumberByNumberPadded(MediaFile inputFile, MusicBrainzMedia musicBrainzMedia,
        bool hasMatch,
        int numberOfAlbums, int trackNumber, int padding = 2)
    {
        if (hasMatch) return true;
        if (numberOfAlbums == 1) return false;
        if (inputFile.Parsed is null) return false;

        string fileName = Path.GetFileName(inputFile.Parsed.FilePath).RemoveDiacritics()
            .RemoveNonAlphaNumericCharacters()
            .ToLower();

        string matchNumber = $"{musicBrainzMedia.Position}-{trackNumber.ToString().PadLeft(padding, '0')} ";
        if (musicBrainzMedia.Tracks.Length < trackNumber) return false;
        string matchString = musicBrainzMedia.Tracks[trackNumber - 1].Title
            .RemoveDiacritics().RemoveNonAlphaNumericCharacters().ToLower().Replace(".mp3", "");

        return fileName.StartsWith(matchNumber)
               && fileName.Contains(matchString);
    }
}