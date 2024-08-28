using NoMercy.MediaProcessing.Common;
using NoMercy.MediaProcessing.Jobs;
using NoMercy.Providers.MusicBrainz.Models;

namespace NoMercy.MediaProcessing.ReleaseGroups;

public class ReleaseGroupManager(
    IReleaseGroupRepository releaseGroupRepository,
    JobDispatcher jobDispatcher
) : BaseManager, IReleaseGroupManager
{
    public Task StoreReleaseAsync(MusicBrainzReleaseGroupDetails releaseGroup)
    {
        throw new NotImplementedException();
    }
}