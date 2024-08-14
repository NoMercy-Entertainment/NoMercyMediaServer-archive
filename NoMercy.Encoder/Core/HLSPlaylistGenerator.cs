using System.Diagnostics;
using System.Text;
using NoMercy.NmSystem;
using static NoMercy.Encoder.Core.IsoLanguageMapper;

namespace NoMercy.Encoder.Core;

public static class HlsPlaylistGenerator
{
    public static void Build(string inputFilePath, string outputFilename, List<string>? priorityLanguages = null)
    {        
        priorityLanguages ??= ["eng", "jpn"];

        var folders = Directory.GetDirectories(inputFilePath)
            .Where(f => Path.GetFileName(f).StartsWith("audio_", StringComparison.InvariantCultureIgnoreCase) || Path.GetFileName(f).StartsWith("video_", StringComparison.InvariantCultureIgnoreCase))
            .ToArray();

        var videoFiles = folders
            .Where(f => Path.GetFileName(f).StartsWith("video_", StringComparison.InvariantCultureIgnoreCase))
            .SelectMany(f => Directory.GetFiles(f, "*.m3u8"));

        var audioFiles = folders
            .Where(f => Path.GetFileName(f).StartsWith("audio_", StringComparison.InvariantCultureIgnoreCase))
            .SelectMany(f => Directory.GetFiles(f, "*.m3u8"))
            .OrderBy(f =>
            {
                var folderName = Path.GetFileName(Path.GetDirectoryName(f));
                var parts = folderName?.Split('_') ?? ["eng", "aac"];
                var language = parts[1];

                var priorityIndex = priorityLanguages.IndexOf(language);
                return priorityIndex >= 0 ? priorityIndex : int.MaxValue;
            })
            .ThenBy(f =>
            {
                var folderName = Path.GetFileName(Path.GetDirectoryName(f));
                var parts = folderName?.Split('_') ?? ["eng", "aac"];
                return parts[1];
            })
            .ThenBy(f => GetTotalSize(Path.GetDirectoryName(f) ?? ""))
            .ToList();
        
        var masterPlaylist = new StringBuilder();
        masterPlaylist.AppendLine("#EXTM3U");
        masterPlaylist.AppendLine("#EXT-X-VERSION:6");
        masterPlaylist.AppendLine();

        var audioGroups = new Dictionary<string, List<string>>();
        foreach (var audioFile in audioFiles)
        {
            var folderName = Path.GetFileName(Path.GetDirectoryName(audioFile));
            var parts = folderName?.Split('_') ?? ["eng", "aac"];
            var language = parts[1];
            var codecName = parts[2];
            var index = audioFiles.IndexOf(audioFile);

            if (!audioGroups.ContainsKey(codecName))
            {
                audioGroups[codecName] = [];
            }
            audioGroups[codecName].Add(language);

            masterPlaylist.AppendLine($"#EXT-X-MEDIA:TYPE=AUDIO,GROUP-ID=\"audio_{codecName}\",LANGUAGE=\"{language}\",AUTOSELECT=YES,DEFAULT={(index == 0 ? "YES" : "NO")},URI=\"{folderName}/{folderName}.m3u8\",NAME=\"{IsoToLanguage[language].ToTitleCase()} {codecName}\"");
        }
        
        masterPlaylist.AppendLine();
        
        var videoGroups = videoFiles
            .Select(videoFile =>
            {
                var folderName = Path.GetFileName(Path.GetDirectoryName(videoFile));
                var parts = folderName?.Split('_', 'x') ?? ["1920", "1080", ""];
                var resolution = $"{parts[1]}x{parts[2]}";
                var isSdr = parts.Length == 4 && parts[3] == "SDR";

                var vCodec = RunProcess("ffprobe", $"-v error -select_streams v:0 -show_entries stream=codec_name -of default=noprint_wrappers=1:nokey=1 {videoFile}").Trim();
                var profile = RunProcess("ffprobe", $"-v error -select_streams v:0 -show_entries stream=profile -of default=noprint_wrappers=1:nokey=1 {videoFile}").Trim();
                var vCodecProfile = MapProfileToCodec(vCodec, profile);

                var duration = GetVideoDuration(videoFile) / 100000;
                var totalSize = GetTotalSize(Path.Combine(inputFilePath, folderName ?? ""));

                var bandwidth = (totalSize * 8) / duration;
                bandwidth = Math.Round(bandwidth);

                bandwidth += 128000; // Adding an estimated overhead for audio

                return new
                {
                    Resolution = resolution,
                    FolderName = folderName,
                    VideoFile = videoFile,
                    VCodecProfile = vCodecProfile,
                    Bandwidth = bandwidth,
                    VCodec = vCodec,
                    IsSdr = isSdr
                };
            })
            .GroupBy(v => new { v.Resolution });

        foreach (var group in videoGroups)
        {
            foreach (var video in group.OrderByDescending(v => v.IsSdr))
            {
                foreach (var audioGroup in audioGroups.Keys)
                {
                    var streamName = $"{video.Resolution} {(video.IsSdr ? "SDR" : "HDR")}";
                    masterPlaylist.AppendLine($"#EXT-X-STREAM-INF:BANDWIDTH={video.Bandwidth},RESOLUTION={video.Resolution},CODECS=\"{video.VCodecProfile},mp4a.40.2\",AUDIO=\"audio_{audioGroup}\"{(video.IsSdr ? ",VIDEO-RANGE=SDR" : ",VIDEO-RANGE=PQ")},NAME=\"{streamName}\"");
                    masterPlaylist.AppendLine($"{video.FolderName}/{Path.GetFileName(video.VideoFile)}");
                    masterPlaylist.AppendLine();
                }
            }
        }

        File.WriteAllText(Path.Combine(inputFilePath, outputFilename + ".m3u8"), masterPlaylist.ToString());
    }
    
    private static double GetTotalSize(string videoFolderPath)
    {
        var segmentFiles = Directory.GetFiles(videoFolderPath, "*.ts");
        var totalSize = segmentFiles.Sum(segmentFile => new FileInfo(segmentFile).Length);
        return totalSize;
    }

    private static double GetVideoDuration(string videoPath)
    {
        var output = RunProcess( "ffprobe", $"-v error -select_streams 0 -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{videoPath}\"");
        return double.Parse(output.Trim().Replace("N/A", "0"));
    }

    static string RunProcess(string command, string arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return result;
    }

    static string MapProfileToCodec(string codec, string profile)
    {
        return profile switch
        {
            "Baseline" => "avc1.42E01E",
            "Main" => "avc1.4D401E",
            "High" => "avc1.64001F",
            _ => "avc1.4D401E"
        };
    }
}