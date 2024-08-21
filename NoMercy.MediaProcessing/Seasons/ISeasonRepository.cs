using NoMercy.Database.Models;

namespace NoMercy.MediaProcessing.Seasons;

public interface ISeasonRepository
{
    public Task StoreSeasonsAsync(IEnumerable<Season> seasons);
    public Task StoreSeasonTranslationsAsync(IEnumerable<Translation> translations);
    public Task StoreSeasonImagesAsync(IEnumerable<Image> images);
    
    public Task<bool> RemoveSeasonAsync(int seasonId);
}