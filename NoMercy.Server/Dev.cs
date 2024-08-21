using Microsoft.EntityFrameworkCore;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.MediaProcessing.Jobs;
using NoMercy.MediaProcessing.Movies;
using NoMercy.MediaProcessing.Shows;
using NoMercy.NmSystem;
using NoMercy.Providers.File;
using Serilog.Events;

namespace NoMercy.Server;

public class Dev
{
    public static async void Run()
    {
        
        MediaContext mediaContext = new();
        JobDispatcher jobDispatcher = new(); 
        
        // IMovieRepository movieRepository = new MovieRepository(mediaContext); 
        // MovieManager movieManager = new(movieRepository, jobDispatcher);
        //
        // Library movieLibrary = await mediaContext.Libraries
        //     .Where(predicate: f => f.Type == "movie")
        //     .FirstAsync();
        //
        // List<int> movies = await mediaContext.Movies
        //     .Select(selector: f => f.Id)
        //     .ToListAsync();
        //
        // foreach (int id in movies)
        // {
        //     await movieManager.AddMovieAsync(id, movieLibrary);
        // }
        
        
        // IShowRepository showRepository = new ShowRepository(mediaContext); 
        // ShowManager showManager = new(showRepository, jobDispatcher);
        //
        // Library tvLibrary = await mediaContext.Libraries
        //     .Where(predicate: f => f.Type == "tv")
        //     .FirstAsync();
        //
        // List<int> tvs = await mediaContext.Tvs
        //     .Select(selector: f => f.Id)
        //     .ToListAsync();
        //
        // foreach (int id in tvs)
        // {
        //     await showManager.AddShowAsync(id, tvLibrary);
        // }
        
        // Logger.System("Starting FileSystem Watcher", LogEventLevel.Debug);
        //
        // MediaContext mediaContext = new();
        // List<Library> libraries = await mediaContext.Libraries
        //     .Include(library => library.FolderLibraries)
        //     .ThenInclude(folderLibrary => folderLibrary.Folder)
        //     .ToListAsync();

        // foreach (Library library in libraries)
        // {
        //     List<string> paths = library.FolderLibraries.Select(folderLibrary => folderLibrary.Folder.Path).ToList();
        //     await Task.Run(() =>
        //     {
        //         var watch = fs.Watch(paths);
        //         Logger.System($"Watching {paths.Count} paths", LogEventLevel.Debug);
        //         
        //         new Task(() =>
        //         {
        //             Task.Delay(20000).Wait();
        //             
        //             foreach (var dispose in watch)
        //             {
        //                 dispose();
        //             }
        //         }).Start();
        //     });
        // }

        // Parallel.ForEach(tasks, task => task.Start());

        // await using MediaContext mediaContext = new();
        //
        // var stream0 = new X264(VideoCodecs.H264Nvenc.Value)
        //     .SetScale(FrameSizes._4k.Width, -2)
        //     .SetConstantRateFactor(20)
        //     .AllowHdr()
        //     .SetHlsSegmentFilename(":type:_:framesize:/:type:_:framesize:")
        //     .SetHlsPlaylistFilename(":type:_:framesize:/:type:_:framesize:")
        //     .SetColorSpace(ColorSpaces.Yuv444p)
        //     .SetPreset(VideoPresets.Slow)
        //     .SetTune(VideoTunes.Hq)
        //     .AddOpts("no-scenecut")
        //     .AddOpts("keyint=48")
        //     .AddCustomArgument("-x264opts", "no-scenecut")
        //     .AddCustomArgument("-crf", 52);
        //
        // var stream1 = new X264(VideoCodecs.H264.Value)
        //     .SetScale(FrameSizes._1080p.Width)
        //     .SetConstantRateFactor(20)
        //     .AllowHdr()
        //     .SetHlsSegmentFilename(":type:_:framesize:/:type:_:framesize:")
        //     .SetHlsPlaylistFilename(":type:_:framesize:/:type:_:framesize:")
        //     .SetColorSpace(ColorSpaces.Yuv444p)
        //     .SetPreset(VideoPresets.Slow)
        //     .SetTune(VideoTunes.Film)
        //     .AddOpts("no-scenecut")
        //     .AddOpts("keyint", 48)
        //     .AddCustomArgument("-x264opts", "no-scenecut");
        //
        // var stream2 = new X264(VideoCodecs.H264Nvenc.Value)
        //     .SetScale(FrameSizes._4k.Width, -2)
        //     .SetConstantRateFactor(20)
        //     .ConvertHdrToSdr()
        //     .SetHlsSegmentFilename(":type:_:framesize:_SDR/:type:_:framesize:_SDR")
        //     .SetHlsPlaylistFilename(":type:_:framesize:_SDR/:type:_:framesize:_SDR")
        //     .SetColorSpace(ColorSpaces.Yuv420p)
        //     .SetPreset(VideoPresets.Slow)
        //     .SetTune(VideoTunes.Hq)
        //     .AddOpts("no-scenecut")
        //     .AddOpts("keyint=48")
        //     .AddCustomArgument("-x264opts", "no-scenecut");
        //
        // var stream3 = new X264(VideoCodecs.H264Nvenc.Value)
        //     .SetScale(FrameSizes._1080p.Width)
        //     .SetConstantRateFactor(20)
        //     .ConvertHdrToSdr()
        //     .SetHlsSegmentFilename(":type:_:framesize:_SDR/:type:_:framesize:_SDR")
        //     .SetHlsPlaylistFilename(":type:_:framesize:_SDR/:type:_:framesize:_SDR")
        //     .SetColorSpace(ColorSpaces.Yuv420p)
        //     .SetPreset(VideoPresets.Slow)
        //     .SetTune(VideoTunes.Hq)
        //     .AddOpts("no-scenecut")
        //     .AddOpts("keyint", 48)
        //     .AddCustomArgument("-x264opts", "no-scenecut");
        //
        // var stream4 = new Aac()
        //     .SetAudioChannels(2)
        //     .SetAllowedLanguages([Languages.Eng, Languages.Jpn, Languages.Fre, Languages.Ger, Languages.Ita])
        //     .SetHlsSegmentFilename(":type:_:language:_:codec:/:type:_:language:_:codec:")
        //     .SetHlsPlaylistFilename(":type:_:language:_:codec:/:type:_:language:_:codec:");
        //
        // var stream5 = new DolbyDigitalPlus()
        //     .SetAllowedLanguages([Languages.Eng])
        //     .SetHlsSegmentFilename(":type:_:language:_:codec:/:type:_:language:_:codec:")
        //     .SetHlsPlaylistFilename(":type:_:language:_:codec:/:type:_:language:_:codec:");
        //
        // var stream6 = new DolbyDigital()
        //     .SetAllowedLanguages([Languages.Eng])
        //     .SetHlsSegmentFilename(":type:_:language:_:codec:/:type:_:language:_:codec:")
        //     .SetHlsPlaylistFilename(":type:_:language:_:codec:/:type:_:language:_:codec:");
        //
        // var stream7 = new Sprite()
        //     .SetScale(256)
        //     .SetFilename("thumbs_:framesize:");
        //
        // var stream8 = new Vtt()
        //     .SetAllowedLanguages([Languages.Dut, Languages.Eng, Languages.Jpn, Languages.Fre, Languages.Ger, Languages.Ita])
        //     .SetHlsSegmentFilename(":type:_:language:_:codec:/:type:_:language:_:codec:")
        //     .SetHlsPlaylistFilename(":type:_:language:_:codec:/:type:_:language:_:codec:");
        //
        // var container = new Hls()
        //     .SetHlsFlags("independent_segments")
        //     .AddStream(stream0)
        //     .AddStream(stream1)
        //     .AddStream(stream2)
        //     .AddStream(stream3)
        //     .AddStream(stream4)
        //     // .AddStream(stream5)
        //     // .AddStream(stream6)
        //     .AddStream(stream7);
        //     // .AddStream(stream8);
        //
        // var ffmpeg = new FfMpeg()
        //     // .Open("G:\\Marvels\\Films\\Download\\Iron.Man.2.2010.2160p.US.BluRay.REMUX.HEVC.DTS-HD.MA.TrueHD.7.1.Atmos-FGT\\Iron.Man.2.2010.2160p.US.BluRay.REMUX.HEVC.DTS-HD.MA.TrueHD.7.1.Atmos-FGT.mkv");
        //     .Open("M:\\Films\\Films\\Sintel.(2010)\\original\\[SDR-HEVC] Sintel.mkv");
        //     // .Open("C:\\Users\\Stoney\\AppData\\Local\\NoMercy_C#\\cache\\transcode\\[HDR-HEVC] Cosmos.Laundromat.S01E01.First.Cycle.mkv");
        //
        // var movie = await mediaContext.Movies
        //     // .FirstOrDefaultAsync(x => x.Id == 10138); // Iron Man 2
        //     .FirstOrDefaultAsync(x => x.Id == 45745); // Sintel
        //     // .FirstOrDefaultAsync(x => x.Id == 358332); // Cosmos Laundromat
        // if (movie == null) return;
        //
        // var folder = await mediaContext.Folders
        //     .FirstOrDefaultAsync(x => x.Id == Ulid.Parse("01HQ5W67GRBPHJKNAZMDYKMVXA"));
        // if (folder == null) return;
        //
        // var folderName = movie.CreateFolderName();
        // var title = movie.CreateTitle();
        // var fileName = movie.CreateFileName();
        // var basePath = Path.Combine(folder.Path, folderName);
        // // var basePath = "C:\\Users\\Stoney\\AppData\\Local\\NoMercy_C#\\cache\\transcode\\ironman";
        // // var basePath = "C:\\Users\\Stoney\\AppData\\Local\\NoMercy_C#\\cache\\transcode\\sintel";
        //
        // ffmpeg.SetBasePath(basePath);
        // ffmpeg.SetTitle(title);
        // ffmpeg.ToFile(fileName);
        //
        // ffmpeg.AddContainer(container);
        //
        // ffmpeg.Build();
        //
        // var ffmpegFile = Path.Combine(AppFiles.CachePath, "ffmpeg.json");
        //
        // var fullCommand = ffmpeg.GetFullCommand();
        // await File.WriteAllTextAsync(ffmpegFile, fullCommand);
        //
        // var progressMeta = new ProgressMeta()
        // {
        //     Id = movie.Id,
        //     Title = title,
        //     BaseFolder = basePath,
        //     ShareBasePath = folder.Id + "/" + folderName,
        //     AudioStreams = container.AudioStreams.Select(x => $"{x.StreamIndex}:{x.Language}_{x.AudioCodec.SimpleValue}").ToList(),
        //     VideoStreams = container.VideoStreams.Select(x => $"{x.StreamIndex}:{x.Scale.W}x{x.Scale.H}_{x.VideoCodec.SimpleValue}").ToList(),
        //     SubtitleStreams = container.SubtitleStreams.Select(x => $"{x.StreamIndex}:{x.Language}_{x.SubtitleCodec.SimpleValue}").ToList(),
        //     HasGpu = container.VideoStreams.Any(x =>
        //         x.VideoCodec.Value == VideoCodecs.H264Nvenc.Value || x.VideoCodec.Value == VideoCodecs.H265Nvenc.Value),
        //     IsHDR = container.VideoStreams.Any(x => x.IsHdr),
        // };
        //
        // var result = await FfMpeg.Run(fullCommand, basePath, progressMeta);
        // Logger.Encoder(result);
        //
        // await stream7.BuildSprite(progressMeta);
        //
        // container.BuildMasterPlaylist();

        // await Task.Run(async () =>
        // {
        //     await Task.Delay(10000);
        //
        //     Process process = new();
        //     process.StartInfo = new ProcessStartInfo
        //     {
        //         WindowStyle = ProcessWindowStyle.Hidden,
        //         FileName = AppFiles.FfmpegPath,
        //         Arguments = "/C -h encoder=webp",
        //         RedirectStandardOutput = true,
        //         UseShellExecute = false,
        //     };
        //
        //     process.Start();
        // });


        // await using MediaContext mediaContext = new();
        // var images = await mediaContext.Images
        //     .Where(image => image.Type == "backdrop" && image.FilePath != null && image.Width > 1920 && image.Height > 1080)
        //     .Where(image => image.VoteAverage > 5)
        //     .Where(image => image.MovieId != null || image.TvId != null)
        //     .Where(e => e.Iso6391 == null || e.Iso6391 == "en" || e.Iso6391 == "" ||
        //                 e.Iso6391 != CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
        //     // .GroupBy(image => image.MovieId ?? image.TvId)
        //     .ToListAsync();
        //
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
        //     
        //     await Ticker(day.Shuffle().ToList());
        // });

        // await Task.Run(async () =>
        // {
        //     await using MediaContext mediaContext = new();
        //     var images = await mediaContext.Images
        //         .Where(image => image.Type == "backdrop" && image.FilePath != null)
        //         .Where(image => image.MovieId != null || image.TvId != null)
        //         .Where(e => e.Iso6391 != null && e.Iso6391 != "en" && e.Iso6391 != "" &&
        //                     e.Iso6391 != CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
        //         .ToListAsync();
        //
        //     Logger.App($"Processing {images.Count} Episode images");
        //
        //     foreach (var image in images)
        //     {
        //         if(image.FilePath == null) continue;
        //         
        //         var path = Path.Combine(AppFiles.ImagesPath, "original", image.FilePath.Replace("/", ""));
        //         // Logger.App(path);
        //         if (!Path.Exists(path))
        //         {
        //             Logger.App($"Image not found: {path}");
        //             mediaContext.Images.Remove(image);
        //             // File.Delete(path);
        //         }
        //     }
        //     
        //     await mediaContext.SaveChangesAsync();
        // });

        // var input = await process.StandardOutput.ReadToEndAsync();
        //     
        // await process.WaitForExitAsync();
        // input = input.Replace(Environment.NewLine, "\r\n");
        //     
        //     await File.WriteAllTextAsync("./output.txt", input);
        //
        // var pattern = @"Encoder (?<Encoder>\w+)\s\[(?<EncoderName>.*)\](?:.*[\r\n]{2}\s+)+?Supported pixel formats:\s(?<PixelFormats>.*)[\r\n]{2}.+\sAVOptions:[\r\n]{2}(?<Options>\s{2}(?<Options_flag>\-[\w-]+)\s+(?<Options_Type><\w*>|\d)\s+(?<Options_Codecs>[DEVASIL\.]{11})\s(?<Options_Description>.*)?[\r\n]{2}){2,}";
        // string pattern = @"Encoder (?<Encpder>\w+)\s\[(?<EncoderName>.*)\](?:.*[\r\n]\s+)+?Supported pixel formats:\s(?<PixelFormats>.*)[\r\n].+\sAVOptions:[\r\n](?<Options>\s{2}(?<Options_flag>\-[\w-]+)\s+(?<Options_Type><\w*>|\d)\s+(?<Options_Codecs>[DEVASIL\.]{11})\s(?<Options_Description>.*)[\r\n]((?<Options_Type_Values>\s{5}(?<Options_Type_Values_Name>[\w\d\.-]+)\s+(?<Options_Type_Values_Value>-\d+|\d+|\w+)\s+(?<Options_Type_Values_Codecs>[DEVASIL\.]{11})(?<Options_Type_Values_Description>.*)[\r\n])+)?)+";

        // var regex = new Regex(pattern, RegexOptions.Multiline,TimeSpan.FromMinutes(100));
        // var matches = regex.Matches(input);
        // Logger.Encoder(matches);

        //     var regex = new Regex(pattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline, TimeSpan.FromSeconds(300));
        //     var matches = regex.Matches(input);
        //     Logger.Encoder(input);
        // 
        // foreach (Match m in matches)
        // {
        //     Console.WriteLine("'{0}' found at index {1}.", m.Value, m.Index);
        // }
        // });

        // var artistClient = new ArtistClient();
        // var result = artistClient.ByMusicBrainzId(new Guid("056e4f3e-d505-4dad-8ec1-d04f521cbb56")).Result;
        // Logger.App(result?.Descriptions);
        //
        // var releaseGroupClient = new ReleaseGroupClient();
        // var result2 = releaseGroupClient.ByMusicBrainzId(new Guid("f9e8042a-674e-3f01-80ec-7f0ab1c537df")).Result;
        // Logger.App(result2?.Descriptions);

        // Task.Run(async () =>
        // {
        //     await using MediaContext mediaContext = new();
        //     var collections = mediaContext.Collections
        //         .Where(x => x._colorPalette == "")
        //         .ToList();
        //
        //     Logger.App($"Processing {collections.Count} Collection images");
        //
        //     foreach (var collection in collections)
        //     {
        //         if (collection is not { _colorPalette: "" }) continue;
        //
        //         Logger.Queue($"Fetching color palette for Collection Images {collection.Title}");
        //
        //         var palette =
        //             await ImageLogic2.GenerateColorPalette(collection.Poster, collection.Backdrop, download: true);
        //         collection._colorPalette = palette;
        //
        //         await mediaContext.SaveChangesAsync();
        //     }
        // });
        //
        // Task.Run(async () =>
        // {
        //     await using MediaContext mediaContext = new();
        //     var people = mediaContext.People
        //         .Where(x => x.Profile != null)
        //         .Where(x => x.Profile != "" && x.Profile != null)
        //         .ToList();
        //
        //     Logger.App($"Processing {people.Count} People images");
        //
        //     foreach (var person in people)
        //     {
        //         if (person is not { _colorPalette: "" }) continue;
        //
        //         var colorPaletteJob = new ColorPaletteJob(person.Id, "person");
        //         JobDispatcher.Dispatch(colorPaletteJob, "image", 5);
        //     }
        // });
        //
        // Task.Run(async () =>
        // {
        //     await using MediaContext mediaContext = new();
        //     var movies = mediaContext.Movies
        //         .Where(x => x._colorPalette == "")
        //         .ToList();
        //
        //     Logger.App($"Processing {movies.Count} Movie images");
        //
        //     foreach (var movie in movies)
        //     {
        //         if (movie is not { _colorPalette: "" }) continue;
        //
        //         var colorPaletteJob = new ColorPaletteJob(movie.Id, "movie");
        //         JobDispatcher.Dispatch(colorPaletteJob, "image", 5);
        //     }
        // });

        // Task.Run(async () =>
        // {
        //     await using MediaContext mediaContext = new();
        //     var recommendations = mediaContext.Recommendations
        //         .Where(x => x._colorPalette == "")
        //         .ToList();
        //
        //     Logger.App($"Processing {recommendations.Count} Recommendation images");
        //
        //     foreach (var recommendation in recommendations)
        //     {
        //         if (recommendation is not { _colorPalette: "" }) continue;
        //         
        //         var type = recommendation.MovieFromId.HasValue ? "movie" : "tv";
        //         var id = recommendation.MovieFromId ?? recommendation.TvFromId ?? 0;
        //
        //         var colorPaletteJob = new ColorPaletteJob(id: id, model: "recommendation", type: type);
        //         JobDispatcher.Dispatch(colorPaletteJob, "image", 5);
        //     }
        // });
        // Task.Run(async () =>
        // {
        //     await using MediaContext mediaContext = new();
        //     var similars = mediaContext.Similar
        //         .Where(x => x._colorPalette == "")
        //         .ToList();
        //
        //     Logger.App($"Processing {similars.Count} Similar images");
        //
        //     foreach (var similar in similars)
        //     {
        //         if (similar is not { _colorPalette: "" }) continue;
        //         
        //         var type = similar.MovieFromId.HasValue ? "movie" : "tv";
        //         var id = similar.MovieFromId ?? similar.TvFromId ?? 0;
        //
        //         var colorPaletteJob = new ColorPaletteJob(id: id, model: "similar", type: type);
        //         JobDispatcher.Dispatch(colorPaletteJob, "image", 5);
        //     }
        // });

        // Task.Run(async () =>
        // {
        //     await using MediaContext mediaContext = new();
        //     var tvs = mediaContext.Tvs
        //         .Where(x => x._colorPalette == "")
        //         .ToList();
        //
        //     Logger.App($"Processing {tvs.Count} TvShow images");
        //
        //     foreach (var tv in tvs)
        //     {
        //         if (tv is not { _colorPalette: "" }) continue;
        //
        //         var colorPaletteJob = new ColorPaletteJob(tv.Id, "tv");
        //         JobDispatcher.Dispatch(colorPaletteJob, "image", 4);
        //     }
        // });
        //
        // Task.Run(async () =>
        // {
        //     await using MediaContext mediaContext = new();
        //     var seasons = mediaContext.Seasons
        //         .Where(x => x._colorPalette == "")
        //         .ToList();
        //
        //     Logger.App($"Processing {seasons.Count} Season images");
        //
        //     foreach (var season in seasons)
        //     {
        //         if (season is not { _colorPalette: "" }) continue;
        //
        //         var colorPaletteJob = new ColorPaletteJob(season.Id, "season");
        //         JobDispatcher.Dispatch(colorPaletteJob, "image", 3);
        //     }
        // });
        //
        // Task.Run(async () =>
        // {
        //     await using MediaContext mediaContext = new();
        //     var episodes = mediaContext.Episodes
        //         .Where(x => x._colorPalette == "")
        //         .ToList();
        //
        //     Logger.App($"Processing {episodes.Count} Episode images");
        //
        //     foreach (var episode in episodes)
        //     {
        //         if (episode is not { _colorPalette: "" }) continue;
        //
        //         var colorPaletteJob = new ColorPaletteJob(episode.Id, "episode");
        //         JobDispatcher.Dispatch(colorPaletteJob, "image", 2);
        //     }
        // });

        // Task.Run(async () =>
        // {
        //     await using MediaContext mediaContext = new();
        //     var images = await mediaContext.Images
        //         .Where(x => x.Iso6391 == "en" || x.Iso6391 == null ||
        //                     x.Iso6391 == CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
        //         .Where(x => x._colorPalette == "" && !x.FilePath!.Contains(".svg"))
        //         .Where(x => !x.ArtistId.HasValue)
        //         .Where(x => !x.AlbumId.HasValue)
        //         .ToListAsync();
        //
        //     Logger.App($"Processing {images.Count} images");
        //
        //     foreach (var image in images)
        //     {
        //         if (image is not { _colorPalette: "" }) continue;
        //
        //         var colorPaletteJob =
        //             new ColorPaletteJob(image.FilePath, "image", image.Iso6391);
        //         JobDispatcher.Dispatch(colorPaletteJob, "image", 1);
        //     }
        // });
        //
        // Task.Run(async () =>
        // {
        //     await using MediaContext mediaContext = new();
        //     var albums = await mediaContext.Albums
        //         .Where(x => x.Cover != "" && x.Cover != null && x._colorPalette == "")
        //         .ToListAsync();
        //
        //     foreach (var album in albums)
        //     {
        //         if (album is not { _colorPalette: "" }) continue;
        //
        //         var colorPaletteJob = new MusicColorPaletteJob(album.Id.ToString(), "album");
        //         JobDispatcher.Dispatch(colorPaletteJob, "image", 2);
        //     }
        // });

        // Task.Run(async () =>
        // {
        //     await using MediaContext mediaContext = new();
        //     var artists = await mediaContext.Artists
        //         .Where(x => x.Cover != "" && x.Cover != null && x._colorPalette == "")
        //         .ToListAsync();
        //     
        //     foreach (var artist in artists)
        //     {
        //         if (artist is not {_colorPalette: "" }) continue;
        //         
        //         var colorPaletteJob = new MusicColorPaletteJob(id: artist.Id.ToString(), model: "artist");
        //         JobDispatcher.Dispatch(colorPaletteJob, "image", 2);
        //     }
        // });
        //
        // Task.Run(async () =>
        // {
        //     await using MediaContext mediaContext = new();
        //     var tracks = await mediaContext.Tracks
        //         .Include(x => x.AlbumTrack)
        //         .Where(x => x.Cover != "" && x.Cover != null && x._colorPalette == "")
        //         .ToListAsync();
        //     
        //     foreach (var track in tracks)
        //     {
        //         if (track is not {_colorPalette: "" }) continue;
        //
        //         if (track.Cover is null) continue;
        //         
        //         var colorPaletteJob = new MusicColorPaletteJob(id: track.Id.ToString(), model: "track");
        //         JobDispatcher.Dispatch(colorPaletteJob, "image", 2);
        //     }
        // });
    }
    //
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
    //
    //     var dominant = image[0, 0];
    //         
    //     return dominant.ToHexString();
    //
    // }
    // private static int _day = 0;
    // // private const int TotalSecondsInDay = 24 * 60 * 60;
    //
    // private class ScreenSaver
    // {
    //     public string Path { get; set; } = "";
    //     // public string Color { get; set; } = "#000000";
    // }
    //
    // private static async Task Ticker(List<ScreenSaver> list)
    // {
    //     Logger.App("Starting Ticker");
    //
    //     // var intervalSeconds = TotalSecondsInDay / list.Count;
    //     using var itemTimer = new PeriodicTimer(TimeSpan.FromSeconds(300));
    //     
    //     _day = new Random().Next(0, list.Count);
    //
    //     do
    //     {
    //         if (_day >= list.Count)
    //         {
    //             _day = 0;
    //         }
    //
    //         var path = list[_day];
    //
    //         Logger.App("New Item: " + path.Path);
    //
    //         var color = GetDominantColor(Path.Combine(AppFiles.ImagesPath, "original", path.Path.Replace("/", "")));
    //
    //         Wallpaper.SilentSet(path.Path, WallpaperStyle.Fit, color);
    //
    //         _day++;
    //     }
    //     while (await itemTimer.WaitForNextTickAsync());
    // }
}