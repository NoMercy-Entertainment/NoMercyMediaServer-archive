using NoMercy.NmSystem;
// using NoMercy.Providers.Discord;
using NoMercy.Providers.OpenSubtitles.Client;
using NoMercy.Providers.OpenSubtitles.Models;

namespace NoMercy.Server;

public class Dev
{
    public static void Run()
    {
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
