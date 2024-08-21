using NoMercy.Database.Models;

namespace NoMercy.MediaProcessing.People;

public interface IPersonRepository
{
    public Task StorePerson(Person person);
    public Task StorePersonTranslations(IEnumerable<Translation> translations);
    public Task StorePersonImages(IEnumerable<Database.Models.Image> images);
}