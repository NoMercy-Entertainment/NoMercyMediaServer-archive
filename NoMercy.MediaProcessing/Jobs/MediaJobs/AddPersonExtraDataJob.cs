// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------

using NoMercy.Database;
using NoMercy.MediaProcessing.Files;
using NoMercy.MediaProcessing.People;
using NoMercy.NmSystem;
using NoMercy.Providers.TMDB.Models.People;
using Serilog.Events;

namespace NoMercy.MediaProcessing.Jobs.MediaJobs;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[Serializable]
public class AddPersonExtraDataJob : AbstractShowExtraDataJob<TmdbPersonAppends, string>
{
    public override string QueueName => "queue";
    public override int Priority => 1;

    public override async Task Handle()
    {
        await using MediaContext context = new();
        JobDispatcher jobDispatcher = new();
        FileRepository fileRepository = new(context);

        PersonRepository personRepository = new(context);
        PersonManager personManager = new(personRepository, jobDispatcher);

        foreach (TmdbPersonAppends person in Storage)
        {
            await personManager.StoreTranslationsAsync(person);
            await personManager.StoreImagesAsync(person);
        }

        Logger.MovieDb($"Show: {Name}: People: Translations and Images stored", LogEventLevel.Debug);
    }
}