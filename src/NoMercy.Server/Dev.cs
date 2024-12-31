using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using BDInfo;
using FFMpegCore;
using MediaBrowser;
using Microsoft.Extensions.Primitives;
using NoMercy.Encoder;
using NoMercy.Encoder.Core;
using NoMercy.NmSystem;
using NoMercy.NmSystem.Extensions;
// using NoMercy.Providers.Discord;
using NoMercy.Providers.OpenSubtitles.Client;
using NoMercy.Providers.OpenSubtitles.Models;

namespace NoMercy.Server;

public class Dev
{
    public static void Run()
    {
        // string fileName = "E:/";
        // string fileName = @"M:\Anime\Download\Bleach\[BDMV] Bleach [BD-BOX] [SET-1]\BLEACH SET 1 DISC 2";
        string fileName = @"H:\TV.Shows\Download\The.Pink.Panther\The Pink Panther - La Pantera Rosa Vol 2 (1966-1968) [Bluray 1080p AVC Eng DTS-HD MA 2.0]";
        string ffmpegExecutable = @"H:\C\Downloads\ffmpeg-build-windows\ffmpeg.exe";
        string ffprobeExecutable = @"H:\C\Downloads\ffmpeg-build-windows\ffprobe.exe";
        string metadataFile = Path.Combine(fileName, "BDMV", "META", "DL", "bdmt_eng.xml");
        
        string xmlContent = File.ReadAllText(metadataFile);
        
        BDROM bdrom = new(fileName);
        try
        {
            bdrom.Scan();
        }
        catch (Exception e)
        {
            //
        }
        
        XDocument doc = XDocument.Parse(xmlContent);
        XNamespace ns = "urn:BDA:bdmv;disclib";
        XNamespace di = "urn:BDA:bdmv;discinfo";

        string title = doc.Descendants(di + "name").FirstOrDefault()?.Value ?? bdrom.VolumeLabel;
        
        string playlistString = FfMpeg.Exec($" -hide_banner -v info -i \"bluray:{fileName}\"", executable: ffprobeExecutable).Result;
        
        string ffprobeString = HlsPlaylistGenerator.RunProcess(AppFiles.FfProbePath,
            $" -v quiet -show_programs -show_format -show_streams -show_data -show_chapters -sexagesimal -print_format json \"bluray:{fileName}\"");

        File.WriteAllText(Path.Combine(AppFiles.TempPath, "bdrom.json"), bdrom.ToJson());
        File.WriteAllText(Path.Combine(AppFiles.TempPath, "analysis.json"), ffprobeString);
        
        // string playlistString = HlsPlaylistGenerator.RunProcess(AppFiles.FfmpegPath,
        //     $" -v info \"bluray:{fileName}\"");
        
        Regex regex = new(@"\[bluray.*?playlist\s(?<playlist>\d+).mpls\s\((?<duration>\d{1,}:\d{1,}:\d{1,})\)");
        List<Match> matches = regex.Matches(playlistString).ToList();

        foreach (Match match in matches)
        {
            using MemoryStream memoryStream = new();
            StringBuilder sb = new();
            
            int matchIndex = matches.IndexOf(match);
            
            string matchTitle = $"{title} {matchIndex + 1}".Replace(":", "");
            string outputFile = Path.Combine(@"G:\TV.Shows\Download\The.Pink.Panther", $"{matchTitle}.mkv");
            string chaptersFile = Path.Combine(AppFiles.TempPath, $"{matchTitle}.txt");
            
            string playlist = match.Groups["playlist"].Value;
            
            TSPlaylistFile playlistFile = bdrom.PlaylistFiles.FirstOrDefault(c => c.Key.StartsWith(playlist)).Value;
            List<TSStream> streams = playlistFile.PlaylistStreams.Values.ToList();
            int duration = playlistFile.TotalLength.ToInt();
            List<double>? chapters = playlistFile.Chapters;
            
            // if (duration != playlistFile.TotalLength.ToInt())
            // {
            //     continue;
            // }
            
            Logger.Encoder(duration.ToString());
            Logger.Encoder(chapters);

            sb.Append(" -hide_banner -v info ");
            
            sb.Append($" -playlist {playlist} -i \"bluray:{fileName}\"");
            
            StringBuilder chapterSb = new();
            chapterSb.AppendLine(";FFMETADATA1");
            chapterSb.AppendLine($"title={matchTitle}");
            chapterSb.AppendLine("");
            
            foreach (double start in chapters)
            {
                int chapterIndex = chapters.IndexOf(start);
                double end = chapterIndex < chapters.Count - 1 ? chapters[chapterIndex + 1] : duration;
                
                chapterSb.AppendLine("[CHAPTER]");
                chapterSb.AppendLine("TIMEBASE=1/1000");
                chapterSb.AppendLine($"START={(int) start * 1000}");
                chapterSb.AppendLine($"END={(int) end * 1000 - 1}");
                chapterSb.AppendLine($"title=Chapter {chapterIndex + 1}");
                chapterSb.AppendLine("");
            }
            
            File.WriteAllText(chaptersFile, chapterSb.ToString());

            sb.Append($" -i \"{chaptersFile}\" -map_metadata 1");
            
            sb.Append(" -c copy");
            
            foreach (TSStream stream in streams.Where(s => s.CodecShortName != "IGS"))
            {
                int index = streams.IndexOf(stream);
                string languageCode = stream.LanguageCode;
                string language = stream.LanguageName;
                
                sb.Append($" -map 0:{index} -metadata:s:{index} language={languageCode} -metadata:s:{index} title=\"{language ?? matchTitle}\"");
            }

            sb.Append($" -f matroska \"{outputFile}\" -y");
            
            string command = sb.ToString();
            
            Logger.Encoder(command + "\"");
            
            FfMpeg.Exec(command, executable: @"H:\C\Downloads\ffmpeg-build-windows\ffmpeg.exe").Wait();
            // Logger.Encoder();
            
        }
        
        // Dictionary<string, TSPlaylistFile> streamFiles = bdrom.PlaylistFiles;
        // foreach (KeyValuePair<string, TSPlaylistFile> streamFile in streamFiles)
        // {
        //     if (streamFile.Key.Contains("00014"))
        //     {
        //         Logger.Encoder(streamFile.Value);
        //     }
        // }

        // FileStream fileStream = File.OpenRead(fileName);
        // BinaryReader fileReader = new(fileStream);
        // ulong streamLength = (ulong)fileStream.Length;
        //
        // byte[] data = new byte[streamLength];
        // int dataLength = fileReader.Read(data, 0, data.Length);
        //
        // int pos = 0;
        //
        // string fileType = ReadString(data, 8, ref pos);
        // Logger.Encoder(fileType);
        // if (fileType != "MPLS0100" && fileType != "MPLS0200" && fileType != "MPLS0300")
        // {
        //     throw new Exception(string.Format(
        //         "Playlist {0} has an unknown file type {1}.",
        //         fileName, fileType));
        // }
        //
        // int playlistOffset = ReadInt32(data, ref pos);
        // int chaptersOffset = ReadInt32(data, ref pos);
        // int extensionsOffset = ReadInt32(data, ref pos);
        //
        // // misc flags
        // pos = 0x38;
        // byte miscFlags = ReadByte(data, ref pos);
        //         
        // // MVC_Base_view_R_flag is stored in 4th bit
        // bool MVCBaseViewR = (miscFlags & 0x10) != 0;
        //
        // pos = playlistOffset;
        //
        // int playlistLength = ReadInt32(data, ref pos);
        // int playlistReserved = ReadInt16(data, ref pos);
        // int itemCount = ReadInt16(data, ref pos);
        // int subitemCount = ReadInt16(data, ref pos);
        //
        // Logger.Encoder(playlistLength.ToString());
        // Logger.Encoder(playlistReserved.ToString());
        // Logger.Encoder(itemCount.ToString());
        // Logger.Encoder(subitemCount.ToString());




        // DiscordBot discordBot = new();
        // await discordBot.MainAsync();


        // OpenSubtitlesClient client = new();
        // OpenSubtitlesClient subtitlesClient = await client.Login();
        // SubtitleSearchResponse? x = await subtitlesClient.SearchSubtitles("Black Panther Wakanda Forever (2022)", "dut");
        // Logger.OpenSubs(x);

        // await using MediaContext mediaContext = new();
        // var images = await mediaContext.Images
        //     .Where(image => image.Type == "backdrop" && image.FilePath != null && image.Width > 1920 && image.Height > 1080)
        //     .Where(image => image.VoteAverage > 5)
        //     .Where(image => image.MovieId != null || image.TvId != null)
        //     .Where(e => e.Iso6391 == null || e.Iso6391 == "en" || e.Iso6391 == "" ||
        //                 e.Iso6391 != CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
        //     // .GroupBy(image => image.MovieId ?? image.TvId)
        //     .ToListAsync();

        // Logger.LoggerConfig.Information(images.Take(5).ToJson());

        // await Task.Run(async () =>
        // {
        //     await using MediaContext mediaContext = new();
        //     var images = await mediaContext.Images
        //         .Where(image => image.Type == "backdrop" && image.FilePath != null && image.Width > 1920 && image.Height > 1080)
        //         .Where(image => image.VoteAverage > 5)
        //         .Where(image => image.MovieId != null || image.TvId != null)
        //         .Where(e => e.Iso6391 == null || e.Iso6391 == "en" || e.Iso6391 == "" ||
        //                     e.Iso6391 != CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
        //         // .GroupBy(image => image.MovieId ?? image.TvId)
        //         .ToListAsync();
        //
        //     Logger.App($"Processing {images.Count} Episode images");
        //
        //     var day = new List<ScreenSaver>();
        //
        //     foreach (var image in images)
        //     {
        //         day.Add(new ScreenSaver{
        //             Path = Path.Combine(AppFiles.ImagesPath, "original", image.FilePath!.Replace("/", "")),
        //         });
        //     }

        //     await Ticker(day.Shuffle().ToList());
        // });
    }

    // public static string GetDominantColor(string path)
    // {
    //     using var image = SixLabors.ImageSharp.Image.Load<Rgb24>(path);
    //     image.Mutate(
    //         x => x
    //             // Scale the image down preserving the aspect ratio. This will speed up quantization.
    //             // We use nearest neighbor as it will be the fastest approach.
    //             .Resize(new ResizeOptions()
    //             {
    //                 Sampler = KnownResamplers.NearestNeighbor,
    //                 Size = new SixLabors.ImageSharp.Size(100, 0)
    //             })
    //             // Reduce the color palette to 1 color without dithering.
    //             .Quantize(new OctreeQuantizer
    //             {
    //                 Options =
    //                 {
    //                     MaxColors = 1,
    //                     Dither = new OrderedDither(1),
    //                     DitherScale = 1
    //                 }
    //             }));

    //     var dominant = image[0, 0];

    //     return dominant.ToHexString();

    // }
    // private static int _day = 0;
    // // private const int TotalSecondsInDay = 24 * 60 * 60;

    // private class ScreenSaver
    // {
    //     public string Path { get; set; } = "";
    //     // public string Color { get; set; } = "#000000";
    // }

    // private static async Task Ticker(List<ScreenSaver> list)
    // {
    //     Logger.App("Starting Ticker");

    //     // var intervalSeconds = TotalSecondsInDay / list.Count;
    //     using var itemTimer = new PeriodicTimer(TimeSpan.FromSeconds(300));

    //     _day = new Random().Next(0, list.Count);

    //     do
    //     {
    //         if (_day >= list.Count)
    //         {
    //             _day = 0;
    //         }

    //         var path = list[_day];

    //         Logger.App("New Item: " + path.Path);

    //         var color = GetDominantColor(Path.Combine(AppFiles.ImagesPath, "original", path.Path.Replace("/", "")));

    //         Wallpaper.SilentSet(path.Path, WallpaperStyle.Fit, color);

    //         _day++;
    //     }
    //     while (await itemTimer.WaitForNextTickAsync());
    // }
}
