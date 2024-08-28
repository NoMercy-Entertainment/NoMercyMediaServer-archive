using NoMercy.Database.Models;

namespace NoMercy.MediaProcessing.ReleaseGroups;

public interface IReleaseGroupRepository
{
    public Task StoreAsync(ReleaseGroup releaseGroup);
}