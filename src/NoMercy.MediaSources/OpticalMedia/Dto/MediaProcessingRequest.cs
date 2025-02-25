namespace NoMercy.MediaSources.OpticalMedia.Dto;

public class MediaProcessingRequest
{
    public string PlaylistId { get; set; } = string.Empty;
    public List<int> SelectedStreamIndexes { get; set; } = [];
    public string? MovieId { get; set; }
    public string? EpisodeId { get; set; }
}
