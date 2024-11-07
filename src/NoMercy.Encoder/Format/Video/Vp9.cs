using NoMercy.Encoder.Format.Rules;

namespace NoMercy.Encoder.Format.Video;

public class Vp9 : BaseVideo
{
    protected internal override bool BFramesSupport => true;
    internal int Passes { get; set; } = 2;

    public Vp9(string videoCodec = "vp9")
    {
        // throw new NotImplementedException("Vp9 is not implemented yet.");
        if(HasGpu)
            SetVideoCodec(videoCodec);
        else
            SetVideoCodec(VideoCodecs.Vp9.Value);
    }

    protected override CodecDto[] AvailableCodecs =>
    [
        VideoCodecs.Vp9,
        VideoCodecs.Vp9Nvenc
    ];

    protected internal override string[] AvailableContainers =>
    [
        VideoContainers.Mkv, VideoContainers.Webm,
        VideoContainers.Flv, VideoContainers.Hls
    ];

    public override string[] AvailablePresets =>
    [
        VideoPresets.VeryFast, VideoPresets.Faster, VideoPresets.Fast,
        VideoPresets.Medium,
        VideoPresets.Slow, VideoPresets.Slower, VideoPresets.VerySlow
    ];

    public override string[] AvailableProfiles =>
    [
        VideoProfiles.Unknown, VideoProfiles.Profile0, VideoProfiles.Profile1,
        VideoProfiles.Profile2, VideoProfiles.Profile3
    ];

    public override string[] AvailableColorSpaces =>
    [
        ColorSpaces.Yuv420p, ColorSpaces.Yuv420p10le,
        ColorSpaces.Yuv422p, ColorSpaces.Yuv444p,
    ];

    public override string[] AvailableTune =>
    [
        VideoTunes.Hq, VideoTunes.Li,
        VideoTunes.Ull, VideoTunes.Lossless
    ];

    public override string[] AvailableLevels => [];

    public override int GetPasses()
    {
        return 0 == Bitrate ? 1 : Passes;
    }
}