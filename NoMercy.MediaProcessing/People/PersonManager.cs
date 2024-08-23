using NoMercy.Database.Models;
using NoMercy.MediaProcessing.Common;
using NoMercy.MediaProcessing.Jobs;
using NoMercy.MediaProcessing.Jobs.MediaJobs;
using NoMercy.MediaProcessing.Jobs.PaletteJobs;
using NoMercy.NmSystem;
using NoMercy.Providers.TMDB.Client;
using NoMercy.Providers.TMDB.Models.Movies;
using NoMercy.Providers.TMDB.Models.People;
using NoMercy.Providers.TMDB.Models.Shared;
using NoMercy.Providers.TMDB.Models.TV;
using Serilog.Events;
using TmdbGender = NoMercy.Database.Models.TmdbGender;

namespace NoMercy.MediaProcessing.People;

public class PersonManager(
    IPersonRepository personRepository,
    JobDispatcher jobDispatcher
) : BaseManager, IPersonManager
{
    public async Task StorePeoplesAsync(TmdbTvShowAppends show)
    {
        (List<int> peopleIds, List<Role> roles, List<Job> jobs) = CollectPeople(show);

        List<TmdbPersonAppends> peopleAppends = await FetchPeopleByIdsAsync(peopleIds);

        IEnumerable<Person> people = peopleAppends
            .Select(person => new Person
            {
                Id = person!.Id,
                Adult = person.Adult,
                AlsoKnownAs = person.AlsoKnownAs.Length > 0 ? person.AlsoKnownAs.ToJson() : null,
                Biography = person.Biography,
                BirthDay = person.BirthDay,
                DeathDay = person.DeathDay,
                TmdbGender = (TmdbGender)person.TmdbGender,
                _externalIds = person.ExternalIds.ToJson(),
                Homepage = person.Homepage?.ToString(),
                ImdbId = person.ImdbId,
                KnownForDepartment = person.KnownForDepartment,
                Name = person.Name,
                PlaceOfBirth = person.PlaceOfBirth,
                Popularity = person.Popularity,
                Profile = person.ProfilePath,
                TitleSort = person.Name
            });

        await personRepository.StoreAsync(people);
        Logger.MovieDb($"Show: {show.Name}: People stored");
        
        await personRepository.StoreRolesAsync(roles);
        Logger.MovieDb($"Show: {show.Name}: Roles stored", LogEventLevel.Debug);

        await personRepository.StoreJobsAsync(jobs);
        Logger.MovieDb($"Show: {show.Name}: Jobs stored", LogEventLevel.Debug);
        
        // jobDispatcher.DispatchJob<AddPersonExtraDataJob, TmdbPersonAppends>(peopleAppends, show.Name);
    }

    public async Task StorePeoplesAsync(TmdbMovieAppends movie)
    {
        (List<int> peopleIds, List<Role> roles, List<Job> jobs) = CollectPeople(movie);

        List<TmdbPersonAppends> peopleAppends = await FetchPeopleByIdsAsync(peopleIds);

        IEnumerable<Person> people = peopleAppends
            .Select(person => new Person
            {
                Id = person!.Id,
                Adult = person.Adult,
                AlsoKnownAs = person.AlsoKnownAs.Length > 0 ? person.AlsoKnownAs.ToJson() : null,
                Biography = person.Biography,
                BirthDay = person.BirthDay,
                DeathDay = person.DeathDay,
                TmdbGender = (TmdbGender)person.TmdbGender,
                _externalIds = person.ExternalIds.ToJson(),
                Homepage = person.Homepage?.ToString(),
                ImdbId = person.ImdbId,
                KnownForDepartment = person.KnownForDepartment,
                Name = person.Name,
                PlaceOfBirth = person.PlaceOfBirth,
                Popularity = person.Popularity,
                Profile = person.ProfilePath,
                TitleSort = person.Name
            });

        await personRepository.StoreAsync(people);
        Logger.MovieDb($"Movie: {movie.Title}: People stored");

        await personRepository.StoreRolesAsync(roles);
        Logger.MovieDb($"Movie: {movie.Title}: Roles stored", LogEventLevel.Debug);

        await personRepository.StoreJobsAsync(jobs);
        Logger.MovieDb($"Movie: {movie.Title}: Jobs stored", LogEventLevel.Debug);
        
        // jobDispatcher.DispatchJob<AddPersonExtraDataJob, TmdbPersonAppends>(peopleAppends, movie.Title);
    }

    public Task UpdatePeopleAsync(string showName, TmdbTvShowAppends show)
    {
        throw new NotImplementedException();
    }

    public Task RemovePeopleAsync(string showName, TmdbTvShowAppends show)
    {
        throw new NotImplementedException();
    }

    internal async Task StoreTranslationsAsync(string showName, TmdbPersonAppends person)
    {
        IEnumerable<Translation> translations = person.Translations.Translations
            .Where(translation => translation.TmdbPersonTranslationData.Overview != "")
            .Select(translation => new Translation
            {
                Iso31661 = translation.Iso31661,
                Iso6391 = translation.Iso6391,
                Name = translation.Name == "" ? null : translation.Name,
                EnglishName = translation.EnglishName,
                Biography = translation.TmdbPersonTranslationData.Overview,
                PersonId = person.Id
            });

        await personRepository.StoreTranslationsAsync(translations);
    }

    internal async Task StoreImagesAsync(string showName, TmdbPersonAppends person)
    {
        IEnumerable<Image> posters = person.Images.Profiles
            .Select(image => new Image
            {
                AspectRatio = image.AspectRatio,
                Height = image.Height,
                Iso6391 = image.Iso6391,
                FilePath = image.FilePath,
                Width = image.Width,
                VoteAverage = image.VoteAverage,
                VoteCount = image.VoteCount,
                PersonId = person.Id,
                Type = "poster",
                Site = "https://image.tmdb.org/t/p/"
            })
            .ToList();

        await personRepository.StoreImagesAsync(posters);

        IEnumerable<Image> posterJobItems = posters
            .Select(x => new Image { FilePath = x.FilePath })
            .ToArray();
        if (posterJobItems.Any())
            jobDispatcher.DispatchJob<ImagePaletteJob, Image>(person.Id, posterJobItems);
    }

    private (List<int> peopleIds, List<Role> roles, List<Job> jobs) CollectPeople(TmdbTvShowAppends show)
    {
        List<int> peopleIds = [];
        List<Role> roles = [];
        List<Job> jobs = [];

        foreach (TmdbTmdbAggregatedCast aggregateCast in show.AggregateCredits.Cast)
        {
            peopleIds.Add(aggregateCast.Id);

            roles.AddRange(aggregateCast.Roles.Select(r => new Role
            {
                // Id = aggregateCast.Id,
                CreditId = r.CreditId,
                Character = r.Character,
                Order = r.Order,
                EpisodeCount = r.EpisodeCount
            }));
        }

        foreach (TmdbTmdbAggregatedCrew aggregateCrew in show.AggregateCredits.Crew)
        {
            peopleIds.Add(aggregateCrew.Id);

            jobs.AddRange(aggregateCrew.Jobs.Select(j => new Job
            {
                // Id = aggregateCrew.Id,
                CreditId = j.CreditId,
                Task = j.Job,
                Order = j.Order,
                EpisodeCount = j.EpisodeCount
            }));
        }

        return (peopleIds, roles, jobs);
    }

    private (List<int> peopleIds, List<Role> roles, List<Job> jobs) CollectPeople(TmdbMovieAppends movie)
    {
        List<int> peopleIds = [];
        List<Role> roles = [];
        List<Job> jobs = [];

        foreach (TmdbCast aggregateCast in movie.Credits.Cast)
        {
            peopleIds.Add(aggregateCast.Id);

            roles.Add(new Role
            {
                // Id = aggregateCast.Id,
                CreditId = aggregateCast.CreditId,
                Character = aggregateCast.Character,
                Order = aggregateCast.Order,
                EpisodeCount = 1
            });
        }

        foreach (TmdbCrew aggregateCrew in movie.Credits.Crew)
        {
            peopleIds.Add(aggregateCrew.Id);

            jobs.Add(new Job
            {
                // Id = aggregateCrew.Id,
                CreditId = aggregateCrew.CreditId,
                Task = aggregateCrew.Job,
                Order = aggregateCrew.Order,
                EpisodeCount = 1
            });
        }

        return (peopleIds, roles, jobs);
    }

    private async Task<List<TmdbPersonAppends>> FetchPeopleByIdsAsync(List<int> ids)
    {
        List<TmdbPersonAppends> personAppends = [];

        await Parallel.ForEachAsync(ids, async (id, _) =>
        {
            try
            {
                using TmdbPersonClient personClient = new(id);
                TmdbPersonAppends? personTask = await personClient.WithAllAppends();
                if (personTask is null) return;

                personAppends.Add(personTask);
            }
            catch (Exception e)
            {
                Logger.MovieDb(e, LogEventLevel.Error);
            }
        });

        return personAppends
            .OrderBy(f => f.Name)
            .ToList();
    }
}