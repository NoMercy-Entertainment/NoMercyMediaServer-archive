using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using BDInfo;
using MediaInfo;
using NoMercy.Encoder;
using NoMercy.Encoder.Core;
using NoMercy.Encoder.Format.Rules;
using NoMercy.MediaSources.OpticalMedia.Dto;
using NoMercy.NmSystem.NewtonSoftConverters;
using NoMercy.NmSystem.SystemCalls;
using Serilog.Events;
using AppFiles = NoMercy.NmSystem.Information.AppFiles;
using Logger = NoMercy.NmSystem.SystemCalls.Logger;
using Shell = NoMercy.NmSystem.SystemCalls.Shell;
using DirectoryInfo = BDInfo.IO.DirectoryInfo;

namespace NoMercy.MediaSources.OpticalMedia;

public partial class DriveMonitor
{
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);
    private readonly HashSet<string> _knownDrives = [];

    public event Action<string, string>? OnMediaInserted;
    public event Action<string>? OnMediaEjected;

    public async Task StartPollingAsync()
    {
        while (true)
        {
            try
            {
                Dictionary<string, string> detectedDrives = Optical.GetOpticalDrives();
                HashSet<string> currentDrives = new(detectedDrives.Keys);

                // Check for newly inserted media
                foreach (KeyValuePair<string, string> drive in detectedDrives)
                {
                    if (!_knownDrives.Contains(drive.Key))
                    {
                        _knownDrives.Add(drive.Key);
                        OnMediaInserted?.Invoke(drive.Key, drive.Value);
                    }
                }

                // Check for ejected media
                foreach (string knownDrive in _knownDrives)
                {
                    if (!currentDrives.Contains(knownDrive))
                    {
                        OnMediaEjected?.Invoke(knownDrive);
                    }
                }

                // Update known drives
                _knownDrives.IntersectWith(currentDrives);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] OpticalDriveMonitor: {ex.Message}");
            }

            await Task.Delay(_pollingInterval);
        }

        // ReSharper disable once FunctionNeverReturns
    }

    public static MetaData? GetDriveMetadata(string drivePath)
    {
        try
        {
            DirectoryInfo directoryInfo = new(drivePath);
            
            BDROM bDRom = ScanBdRom(directoryInfo);
            
            string title = TryGetTitle(bDRom);

            string path = directoryInfo.FullName.TrimEnd(Path.DirectorySeparatorChar);
            string playlistString = Shell.ExecStdErrSync(AppFiles.FfProbePath, $" -hide_banner -v info -i \"bluray:{path}\"");
            FfprobeResult? ffProbeData = playlistString.FromJson<FfprobeResult>();
            
            File.WriteAllText(Path.Combine(AppFiles.TempPath, "bdrom.json"), bDRom.ToJson());
            
            MatchCollection matches = PlaylistRegex().Matches(playlistString);

            List<BluRayPlaylist> bluRayPlaylist = [];

            foreach (Match match in matches)
            {
                string playlist = match.Groups["playlist"].Value;

                string mplsPath = Path.Combine(directoryInfo.FullName, "BDMV", "PLAYLIST", $"{playlist}.mpls");

                MediaInfoList mediaList = new(false);
                mediaList.Open(mplsPath, InfoFileOptions.Max);

                string? inform = mediaList.Inform(0);

                BluRayPlaylist parsedPlaylist = BluRayPlaylist.Parse(inform);
                
                bluRayPlaylist.Add(parsedPlaylist);
            }

            return new()
            {
                Title = title,
                FfProbeData = ffProbeData,
                BluRayPlaylists = bluRayPlaylist,
            };
            
        }
        catch (Exception ex)
        {
            Logger.Ripper(ex, LogEventLevel.Error);
        }
        
        return null;
    }

    public static MetaData? ProcessMedia(string drivePath, MediaProcessingRequest request)
    {
        try
        {
            DirectoryInfo directoryInfo = new(drivePath);
            
            BDROM bDRom = ScanBdRom(directoryInfo);
            
            string title = TryGetTitle(bDRom);
            
            string path = directoryInfo.FullName.TrimEnd(Path.DirectorySeparatorChar);
            string playlistString = Shell.ExecStdErrSync(AppFiles.FfProbePath, $" -hide_banner -v info -i \"bluray:{path}\"");
            
            FfprobeResult? ffProbeData = playlistString.FromJson<FfprobeResult>();

            File.WriteAllText(Path.Combine(AppFiles.TempPath, "bdrom.json"), bDRom.ToJson());
            
            List<Match> matches = PlaylistRegex().Matches(playlistString).ToList();

            List<BluRayPlaylist> bluRayPlaylist = [];
            
            foreach (Match match in matches)
            {
                StringBuilder sb = new();

                int matchIndex = matches.IndexOf(match);

                string matchTitle = $"{title} {matchIndex + 1}".Replace(":", "");
                string outputFile = Path.Combine(AppFiles.TempPath, $"{matchTitle}.mkv");
                string chaptersFile = Path.Combine(AppFiles.TempPath, $"{matchTitle}.txt");

                string playlist = match.Groups["playlist"].Value;
                
                string mplsPath = Path.Combine(directoryInfo.FullName, "BDMV", "PLAYLIST", $"{playlist}.mpls");

                MediaInfoList mediaList = new(false);
                mediaList.Open(mplsPath, InfoFileOptions.Max);

                string? inform = mediaList.Inform(0);

                BluRayPlaylist parsedPlaylist = BluRayPlaylist.Parse(inform);
                bluRayPlaylist.Add(parsedPlaylist);

                sb.Append(" -hide_banner -progress - ");
                sb.Append($" -y -playlist {playlist} -i \"bluray:{path}\" ");

                StringBuilder metadata = new();
                metadata.AppendLine(";FFMETADATA1");
                metadata.AppendLine($"title={matchTitle}");
                metadata.AppendLine("");

                foreach (Chapter chapter in parsedPlaylist.Chapters)
                {
                    int chapterIndex = parsedPlaylist.Chapters.IndexOf(chapter);
                    int start = (int)chapter.Timestamp.TotalSeconds;
                    int end = (chapterIndex < parsedPlaylist.Chapters.Count - 1
                        ? (int)parsedPlaylist.Chapters[chapterIndex + 1].Timestamp.TotalSeconds
                        : (int)parsedPlaylist.Duration.TotalSeconds);
                    
                    // start can't be less than 0 or greater than the duration
                    start = Math.Max(0, start);
                    start = Math.Min(start, end);
                    
                    // end can't be less than start or greater than the duration
                    end = Math.Max(start, end);
                    end = Math.Min(end, (int)parsedPlaylist.Duration.TotalSeconds);
                    
                    // start must be less than end and longer than 5 seconds to be considered a valid chapter
                    if (start >= end || end - start < 5)  continue;

                    metadata.AppendLine("[CHAPTER]");
                    metadata.AppendLine("TIMEBASE=1");
                    metadata.AppendLine($"START={start}");
                    metadata.AppendLine($"END={end}");
                    metadata.AppendLine($"title=Chapter {chapterIndex + 1}");
                    metadata.AppendLine("");
                }

                File.WriteAllText(chaptersFile, metadata.ToString());

                sb.Append(" -c copy -map 0:v:0 ");
                
                foreach (AudioTrack stream in parsedPlaylist.AudioTracks)
                {
                    int index = parsedPlaylist.AudioTracks.IndexOf(stream);
                    string languageCode = IsoLanguageMapper.GetIsoCode(stream.Language) ?? "und";
                    string language = stream.Language;

                    sb.Append(
                        $" -map 0:a:{index} -metadata:s:a:{index} language={languageCode} -metadata:s:a:{index} title=\"{language}\"");
                }
                
                foreach (SubtitleTrack stream in parsedPlaylist.SubtitleTracks)
                {
                    int index = parsedPlaylist.SubtitleTracks.IndexOf(stream);
                    string languageCode = IsoLanguageMapper.GetIsoCode(stream.Language) ?? "und";
                    string language = stream.Language;

                    sb.Append(
                        $" -map 0:s:{index} -metadata:s:s:{index} language={languageCode} -metadata:s:s:{index} title=\"{language}\"");
                }

                sb.Append($" -f matroska \"{outputFile}\" ");

                string command = sb.ToString();

                Logger.Encoder(command + "\"");

                ProgressMeta progressMeta = new()
                {
                    Id = Guid.NewGuid(),
                    Title = matchTitle,
                    BaseFolder = path,
                    ShareBasePath = path,
                    AudioStreams = parsedPlaylist.AudioTracks.Select(x => $"{x.StreamIndex}:{x.Language}_{x.Format}").ToList(),
                    VideoStreams = parsedPlaylist.VideoTracks.Select(x => $"{x.StreamIndex}:{x.Width}x{x.Height}_{x.Format}").ToList(),
                    SubtitleStreams = parsedPlaylist.SubtitleTracks.Select(x => $"{x.StreamIndex}:{x.Language}_{x.Format}").ToList(),
                    HasGpu = parsedPlaylist.VideoTracks.Any(x =>
                        x.Format == VideoCodecs.H264Nvenc.Value || x.Format == VideoCodecs.H265Nvenc.Value),
                    IsHdr = false
                };
                
                FfMpeg.Run(command, AppFiles.TempPath, progressMeta).Wait();
                
                File.Delete(chaptersFile);
            }


            return new()
            {
                Title = title,
                FfProbeData = ffProbeData,
                BluRayPlaylists = bluRayPlaylist,
            };
            
        }
        catch (Exception ex)
        {
            Logger.Ripper(ex, LogEventLevel.Error);
        }
        
        return null;
    }

    private static BDROM ScanBdRom(DirectoryInfo directoryInfo)
    {
        BDROM bDRom = new(directoryInfo);
            
        bDRom.PlaylistFileScanError += (sender, args) =>
        {
            Logger.Ripper(args.Message, LogEventLevel.Error);
            return false;
        };
        bDRom.StreamClipFileScanError += (sender, args) =>
        {
            Logger.Ripper(args.Message, LogEventLevel.Error);
            return false;
        };
        bDRom.StreamFileScanError += (sender, args) =>
        {
            Logger.Ripper(args.Message, LogEventLevel.Error);
            return false;
        };
            
        bDRom.Scan();
        return bDRom;
    }

    private static string TryGetTitle(BDROM bDRom)
    {
        string metadataFile = Path.Combine(bDRom.DirectoryMETA.FullName, "DL", "bdmt_eng.xml");
        string title = bDRom.VolumeLabel;
            
        if (File.Exists(metadataFile)){
            string xmlContent = File.ReadAllText(metadataFile);
            XDocument doc = XDocument.Parse(xmlContent);
            XNamespace di = "urn:BDA:bdmv;discinfo";
            title = doc.Descendants(di + "name").FirstOrDefault()?.Value ?? bDRom.VolumeLabel;
        }

        return title;
    }

    [GeneratedRegex(@"\[bluray.*?playlist\s(?<playlist>\d+).mpls\s\((?<duration>\d{1,}:\d{1,}:\d{1,})\)")]
    private static partial Regex PlaylistRegex();
}
