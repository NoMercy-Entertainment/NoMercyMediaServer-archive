namespace NoMercy.Database;

public class IAudioProfile
{
    public string Codec { get; set; }
    public int Channels { get; set; }
    public string SegmentName { get; set; }
    public string PlaylistName { get; set; }
    public string[] AllowedLanguages { get; set; }
}