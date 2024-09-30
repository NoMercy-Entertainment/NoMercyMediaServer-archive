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
using NoMercy.MediaProcessing.Files;
using NoMercy.NmSystem;

namespace NoMercy.MediaProcessing.Jobs.MediaJobs;

public class EncodeVideoJob : AbstractEncoderJob
{
    public override string QueueName => "encoder";
    public override int Priority => 4;
    public string Status { get; set; } = "pending";

    public override async Task Handle()
    {
        await using MediaContext context = new();
        await using QueueContext queueContext = new();
        JobDispatcher jobDispatcher = new();

        // Status = "processing";
        // await queueContext.SaveChangesAsync();

        LibraryRepository libraryRepository = new(context);
        // LibraryManager libraryManager = new(libraryRepository, jobDispatcher);

        FileRepository fileRepository = new(context);
        FileManager fileManager = new(fileRepository, jobDispatcher);

        Folder? folder = await libraryRepository.GetLibraryFolder(FolderId);
        if (folder is null) return;

        List<EncoderProfile> profiles = folder.EncoderProfileFolder
            .Select(e => e.EncoderProfile)
            .ToList();

        if (profiles.Count == 0) return;
        
        if (await GetFileMetaData(folder, context) is not {success:true} values) return;
        (_, string folderName, string title, string fileName, string basePath, int baseId) = values;

        foreach (EncoderProfile profile in profiles)
        {
            BaseContainer container = BaseContainer.Create(profile.Container);

            BuildVideoStreams(profile, ref container);
            BuildAudioStreams(profile, ref container);
            BuildSubtitleStreams(profile, ref container);

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

            await ffmpeg.Run(fullCommand, basePath, progressMeta);

            await sprite.BuildSprite(progressMeta);

            container.BuildMasterPlaylist();

            await fileManager.FindFiles(baseId, folder.FolderLibraries.First().Library);
        }
    }
    
    private async Task<(bool success, string folderName, string title, string fileName, string basePath, int baseId)> GetFileMetaData(Folder folder, MediaContext context) {
        Movie? movie = folder.FolderLibraries.Any(x => x.Library.Type == "movie")
            ? await context.Movies
                .FirstOrDefaultAsync(x => x.Id == Id)
            : null;

        Episode? episode = folder.FolderLibraries.Any(x => x.Library.Type == "tv" || x.Library.Type == "anime")
            ? await context.Episodes
                .Include(x => x.Tv)
                .FirstOrDefaultAsync(x => x.Id == Id)
            : null;

        if (movie is null && episode is null)
        {
            return (false, string.Empty, string.Empty, string.Empty, string.Empty, 0);
        }

        string folderName = (movie?.CreateFolderName().Replace("/", "")
                ?? episode!.Tv.CreateFolderName().Replace("/", "") + episode.CreateFolderName())
            .Replace("/", Path.DirectorySeparatorChar.ToString());

        string title = movie?.CreateTitle() ?? episode!.CreateTitle();
        string fileName = movie?.CreateFileName() ?? episode!.CreateFileName();
        string basePath = Path.Combine(folder.Path, folderName);
        int baseId = movie?.Id ?? episode!.Tv.Id;

        return (true, folderName, title, fileName, basePath, baseId);
    }

    private static void BuildVideoStreams(EncoderProfile? encoderProfile, ref BaseContainer container)
    {
        foreach (IVideoProfile profile in encoderProfile.VideoProfiles)
        {
            BaseVideo stream = BaseVideo.Create(profile.Codec)
                .SetScale(profile.Width, profile.Height)
                .SetConstantRateFactor(profile.Crf)
                .SetFrameRate(profile.Framerate)
                .SetKiloBitrate(profile.Bitrate)
                .ConvertHdrToSdr()
                .SetHlsSegmentFilename(profile.SegmentName)
                .SetHlsPlaylistFilename(profile.PlaylistName)
                .SetColorSpace(profile.ColorSpace)
                .SetPreset(profile.Preset)
                .SetTune(profile.Tune)
                .AddOpt("keyint", profile.Keyint)
                .AddOpts(profile.Opts)
                .AddCustomArguments(profile.CustomArguments);

            container.AddStream(stream);
        }
    }

    private static void BuildAudioStreams(EncoderProfile? encoderProfile, ref BaseContainer container)
    {
        foreach (IAudioProfile profile in encoderProfile.AudioProfiles)
        {
            BaseAudio stream = BaseAudio.Create(profile.Codec)
                .SetAudioChannels(profile.Channels)
                .SetAllowedLanguages(profile.AllowedLanguages)
                .SetHlsSegmentFilename(profile.SegmentName)
                .SetHlsPlaylistFilename(profile.PlaylistName)
                .AddOpts(profile.Opts)
                .AddCustomArguments(profile.CustomArguments);

            container.AddStream(stream);
        }
    }

    private static void BuildSubtitleStreams(EncoderProfile? encoderProfile, ref BaseContainer container)
    {
        foreach (ISubtitleProfile profile in encoderProfile.SubtitleProfiles)
        {
            BaseSubtitle stream = BaseSubtitle.Create(profile.Codec)
                .SetAllowedLanguages(profile.AllowedLanguages)
                .SetHlsSegmentFilename(profile.SegmentName)
                .SetHlsPlaylistFilename(profile.PlaylistName)
                .AddOpts(profile.Opts)
                .AddCustomArguments(profile.CustomArguments);

            container.AddStream(stream);
        }
    }
}