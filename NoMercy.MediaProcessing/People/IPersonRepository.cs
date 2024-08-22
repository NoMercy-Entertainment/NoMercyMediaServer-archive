using NoMercy.Database.Models;

namespace NoMercy.MediaProcessing.People;

public enum Type
{
    TvShow,
    Season,
    Episode,
    Movie
}

public interface IPersonRepository
{
    public Task StoreAsync(IEnumerable<Person> people);
    public Task StoreTranslationsAsync(IEnumerable<Translation> translations);
    public Task StoreImagesAsync(IEnumerable<Image> images);
    
    public Task StoreCastAsync(IEnumerable<Cast> cast, Type type);
    public Task StoreCrewAsync(IEnumerable<Crew> crew, Type type);
    public Task StoreCreatorAsync(Creator creator);
    public Task StoreGuestStarsAsync(IEnumerable<GuestStar> guestStars);
    
    public Task StoreRolesAsync(IEnumerable<Role> roles);
    public Task StoreJobsAsync(IEnumerable<Job> job);
    
    public Task StoreAggregateCreditsAsync();
    public Task StoreAggregateCastAsync();
    public Task StoreAggregateCrewAsync();
}