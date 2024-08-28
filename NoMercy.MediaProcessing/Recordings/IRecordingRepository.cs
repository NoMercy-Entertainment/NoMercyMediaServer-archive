using NoMercy.Database.Models;

namespace NoMercy.MediaProcessing.Recordings;

public interface IRecordingRepository
{
    public Task StoreAsync(Track recording, bool update = false);
    public Task LinkToRelease(AlbumTrack trackRelease);
}