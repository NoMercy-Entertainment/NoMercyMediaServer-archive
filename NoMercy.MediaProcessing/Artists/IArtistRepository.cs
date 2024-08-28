using NoMercy.Database.Models;

namespace NoMercy.MediaProcessing.Artists;

public interface IArtistRepository
{
    public Task StoreAsync(Artist artist);
}