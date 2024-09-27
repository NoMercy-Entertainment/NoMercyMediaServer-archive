namespace NoMercy.Database.Models;

public class ISubtitleProfile
{
    public string Codec { get; set; }
    public string SegmentName { get; set; }
    public string PlaylistName { get; set; }
    public string[] AllowedLanguages { get; set; }
}