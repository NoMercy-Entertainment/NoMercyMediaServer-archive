using NoMercy.Database.Models;

namespace NoMercy.MediaProcessing.People;

public class PersonRepository: IPersonRepository
{
    public Task StorePerson(Person person)
    {
        throw new NotImplementedException();
    }

    public Task StorePersonTranslations(IEnumerable<Translation> translations)
    {
        throw new NotImplementedException();
    }

    public Task StorePersonImages(IEnumerable<Image> images)
    {
        throw new NotImplementedException();
    }
}