using NoMercy.MediaProcessing.Common;
using NoMercy.MediaProcessing.Jobs;
using NoMercy.Providers.MusicBrainz.Models;

namespace NoMercy.MediaProcessing.Recordings;

public class RecordingManager(
    IRecordingRepository recordingRepository,
    JobDispatcher jobDispatcher
) : BaseManager, IRecordingManager
{
    public Task StoreRecordingAsync(MusicBrainzRecordingAppends release)
    {
        throw new NotImplementedException();
    }
}