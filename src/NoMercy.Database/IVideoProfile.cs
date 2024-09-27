namespace NoMercy.Database;

public class IVideoProfile
{
    public string Codec { get; set; }
    public int Bitrate { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Framerate { get; set; }
    public string Preset { get; set; }
    public string Tune { get; set; }
    public string SegmentName { get; set; }
    public string PlaylistName { get; set; }
    public string ColorSpace { get; set; }
    public int Crf { get; set; }
    public int Keyint { get; set; }
    public string[] Opts { get; set; }
    public (string key, string Val)[] CustomArguments { get; set; }
}