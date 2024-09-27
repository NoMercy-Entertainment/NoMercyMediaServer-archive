using Microsoft.EntityFrameworkCore;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.Encoder;
using NoMercy.MediaProcessing.Libraries;
using NoMercy.Encoder.Core;
using NoMercy.Encoder.Format.Audio;
using NoMercy.Encoder.Format.Container;
using NoMercy.Encoder.Format.Image;
using NoMercy.Encoder.Format.Rules;
using NoMercy.Encoder.Format.Subtitle;
using NoMercy.Encoder.Format.Video;
using NoMercy.NmSystem;

namespace NoMercy.MediaProcessing.Jobs.MediaJobs;

public class EncodeVideoJob : AbstractEncoderJob
{
    public override string QueueName => "encoder";
    public override int Priority => 10;

    public override async Task Handle()
    {
        await using MediaContext context = new();
        JobDispatcher jobDispatcher = new();

        LibraryRepository libraryRepository = new(context);
        LibraryManager libraryManager = new(libraryRepository, jobDispatcher);

        Folder? folder = await libraryRepository.GetLibraryFolder(FolderId);
        if (folder is null) return;

        List<EncoderProfile> profiles = folder.EncoderProfileFolder
            .Select(e => e.EncoderProfile)
            .ToList();

        if (profiles.Count == 0) return;
        
        if (await GetFileMetaData(folder, context) is not {success:true} values) return;
        (_, string folderName, string title, string fileName, string basePath) = values;

        foreach (EncoderProfile profile in profiles)
        {
            BaseContainer container = profile.Container switch
            {
                "mkv" => new Mkv(),
                "Mp4" => new Mp4(),
                "Hls" => new Hls().SetHlsFlags("independent_segments"),
                _ => new Hls().SetHlsFlags("independent_segments")
            };

            BuildVideoStreams(profile, ref container, fileName);
            BuildAudioStreams(profile, ref container, fileName);
            BuildSubtitleStreams(profile, ref container, fileName);

            BaseImage sprite = new Sprite()
                .SetScale(320)
                .SetFilename("thumbs_:framesize:");
            container.AddStream(sprite);

            VideoAudioFile ffmpeg = new FfMpeg()
                .Open(InputFile);

            ffmpeg.SetBasePath(basePath);
            ffmpeg.SetTitle(title);
            ffmpeg.ToFile(fileName);

            ffmpeg.AddContainer(container);

            ffmpeg.Build();

            string fullCommand = ffmpeg.GetFullCommand();
            Logger.Encoder(fullCommand);

            var progressMeta = new ProgressMeta
            {
                Id = Id,
                Title = title,
                BaseFolder = basePath,
                ShareBasePath = folder.Id + "/" + folderName,
                AudioStreams = container.AudioStreams.Select(x => $"{x.StreamIndex}:{x.Language}_{x.AudioCodec.SimpleValue}").ToList(),
                VideoStreams = container.VideoStreams.Select(x => $"{x.StreamIndex}:{x.Scale.W}x{x.Scale.H}_{x.VideoCodec.SimpleValue}").ToList(),
                SubtitleStreams = container.SubtitleStreams.Select(x => $"{x.StreamIndex}:{x.Language}_{x.SubtitleCodec.SimpleValue}").ToList(),
                HasGpu = container.VideoStreams.Any(x =>
                    x.VideoCodec.Value == VideoCodecs.H264Nvenc.Value || x.VideoCodec.Value == VideoCodecs.H265Nvenc.Value),
                IsHDR = container.VideoStreams.Any(x => x.IsHdr)
            };

            string result = await ffmpeg.Run(fullCommand, basePath, progressMeta);
            Logger.Encoder(result);

            await sprite.BuildSprite(progressMeta);

            container.BuildMasterPlaylist();
        }
    }
    
    private async Task<(bool success, string folderName, string title, string fileName, string basePath)> GetFileMetaData(Folder folder, MediaContext context) {
        Movie? movie = folder.FolderLibraries.Any(x => x.Library.Type == "movie")
            ? await context.Movies
                .FirstOrDefaultAsync(x => x.Id == Id)
            : null;

        Episode? episode = folder.FolderLibraries.Any(x => x.Library.Type == "tv" || x.Library.Type == "anime")
            ? await context.Episodes
                .Include(x => x.Tv)
                .FirstOrDefaultAsync(x => x.Id == Id)
            : null;

        if (movie is null && episode is null) return (false, string.Empty, string.Empty, string.Empty, string.Empty);

        string folderName = (movie?.CreateFolderName().Replace("/", "")
                ?? episode!.Tv.CreateFolderName().Replace("/", "") + episode.CreateFolderName())
            .Replace("/", Path.DirectorySeparatorChar.ToString());

        // int id = movie?.Id ?? episode!.Id;
        string title = movie?.CreateTitle() ?? episode!.CreateTitle();
        string fileName = movie?.CreateFileName() ?? episode!.CreateFileName();
        string basePath = Path.Combine(folder.Path, folderName);
        return (true, folderName, title, fileName, basePath);
    }

    private static void BuildVideoStreams(EncoderProfile encoderProfile, ref BaseContainer container, string fileName)
    {
        foreach (IVideoProfile profile in encoderProfile.VideoProfiles)
        {
            BaseVideo stream = BaseVideo.Create(profile.Codec)
                .SetScale(profile.Width, profile.Height) // FrameSizes._1080p.Width
                .SetConstantRateFactor(profile.Crf) //20
                .SetFrameRate(profile.Framerate) //24
                .SetKiloBitrate(profile.Bitrate) // 5000
                .ConvertHdrToSdr()
                .SetHlsSegmentFilename(profile.SegmentName) //":type:_:framesize:_SDR/:type:_:framesize:_SDR"
                .SetHlsPlaylistFilename(profile.PlaylistName) //":type:_:framesize:_SDR/:type:_:framesize:_SDR"
                .SetColorSpace(profile.ColorSpace) //ColorSpaces.Yuv420p
                .SetPreset(profile.Preset) //VideoPresets.Fast
                .SetTune(profile.Tune) //VideoTunes.Hq
                .AddOpts("keyint", profile.Keyint);  //"keyint", 48

            foreach (string opt in profile.Opts)
            {
                stream.AddOpts(opt); //"no-scenecut"
            }

            foreach ((string key, string val) in profile.CustomArguments)
            {
                stream.AddCustomArgument(key, val); //"-x264opts", "no-scenecut"
            }

            container.AddStream(stream);
        }
    }

    private static void BuildAudioStreams(EncoderProfile encoderProfile, ref BaseContainer container, string fileName)
    {
        foreach (IAudioProfile profile in encoderProfile.AudioProfiles)
        {
            BaseAudio stream = BaseAudio.Create(profile.Codec)
                .SetAudioChannels(profile.Channels) // 2
                .SetAllowedLanguages(profile.AllowedLanguages) //[
                //     Languages.Dut, Languages.Eng, Languages.Jpn, Languages.Fre, Languages.Ger, Languages.Ita,
                //     Languages.Spa,  Languages.Por, Languages.Rus, Languages.Kor, Languages.Chi, Languages.Ara
                // ])
                .SetHlsSegmentFilename(profile.SegmentName) //":type:_:language:_:codec:/:type:_:language:_:codec:"
                .SetHlsPlaylistFilename(profile.PlaylistName); //":type:_:language:_:codec:/:type:_:language:_:codec:"

            container.AddStream(stream);
        }
    }

    private static void BuildSubtitleStreams(EncoderProfile encoderProfile, ref BaseContainer container, string fileName)
    {
        foreach (ISubtitleProfile profile in encoderProfile.SubtitleProfiles)
        {
            BaseSubtitle stream = BaseSubtitle.Create(profile.Codec)
                .SetAllowedLanguages(profile.AllowedLanguages) //[
                //     Languages.Dut, Languages.Eng, Languages.Jpn, Languages.Fre, Languages.Ger, Languages.Ita,
                //     Languages.Spa,  Languages.Por, Languages.Rus, Languages.Kor, Languages.Chi, Languages.Ara
                // ])
                .SetHlsSegmentFilename(profile.SegmentName) //":type:_:language:_:codec:/_:language:_:codec:"
                .SetHlsPlaylistFilename(profile.PlaylistName); //$"subtitles/{fileName}.:language:.:variant:";

            container.AddStream(stream);
        }
    }
}