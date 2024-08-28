using NoMercy.Providers.MusicBrainz.Models;

namespace NoMercy.MediaProcessing.ReleaseGroups;

public interface IReleaseGroupManager
{
    public Task StoreReleaseAsync(MusicBrainzReleaseGroupDetails releaseGroup);
}