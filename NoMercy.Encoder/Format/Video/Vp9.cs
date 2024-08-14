using NoMercy.Encoder.Format.Rules;

namespace NoMercy.Encoder.Format.Video;

public class Vp9 : BaseVideo
{
    internal protected override bool BFramesSupport => true;
    internal int Passes { get; set; } = 2;

    public Vp9(string videoCodec = "vp9")
    {
        throw new NotImplementedException("Vp9 is not implemented yet.");
        SetVideoCodec(videoCodec);
    }

    protected override CodecDto[] AvailableCodecs => 
    [
        VideoCodecs.Vp9,
        VideoCodecs.Vp9Nvenc
    ];

    internal protected override string[] AvailableContainers =>
    [
        VideoContainers.Mkv, VideoContainers.Webm,
        VideoContainers.Flv, VideoContainers.Hls
    ];
    
    internal protected override string[] AvailablePresets =>
    [
        VideoPresets.VeryFast, VideoPresets.Faster, VideoPresets.Fast,
        VideoPresets.Medium,
        VideoPresets.Slow, VideoPresets.Slower, VideoPresets.VerySlow
    ];

    internal protected override string[] AvailableProfiles =>
    [
        VideoProfiles.Unknown, VideoProfiles.Profile0, VideoProfiles.Profile1, 
        VideoProfiles.Profile2, VideoProfiles.Profile3
    ];

    internal protected override string[] AvailableTune =>
    [
        VideoTunes.Hq, VideoTunes.Li, 
        VideoTunes.Ull, VideoTunes.Lossless
    ];

    internal protected override string[] AvailableLevels => [];

    public override int GetPasses()
    {
        return 0 == Bitrate ? 1 : Passes;
    }
}