using NoMercy.Providers.MusicBrainz.Models;

namespace NoMercy.MediaProcessing.Recordings;

public interface IRecordingManager
{
    public Task StoreRecordingAsync(MusicBrainzRecordingAppends recording);
}