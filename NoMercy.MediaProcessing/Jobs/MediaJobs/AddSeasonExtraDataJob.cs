// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------

using NoMercy.Database;
using NoMercy.MediaProcessing.Seasons;
using NoMercy.NmSystem;
using NoMercy.Providers.TMDB.Models.Season;
using Serilog.Events;

namespace NoMercy.MediaProcessing.Jobs.MediaJobs;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[Serializable]
public class AddSeasonExtraDataJob : AbstractShowExtraDataJob<TmdbSeasonAppends, string>
{
    public override string QueueName => "queue";
    public override int Priority => 1;

    public override async Task Handle()
    {
        await using MediaContext context = new();
        JobDispatcher jobDispatcher = new();

        SeasonRepository seasonRepository = new(context);
        SeasonManager seasonManager = new(seasonRepository, jobDispatcher);

        foreach (TmdbSeasonAppends season in Storage)
        {
            await seasonManager.StoreImagesAsync(Name, season);
            await seasonManager.StoreTranslationsAsync(Name, season);
        }

        Logger.MovieDb($"Show: {Name}: Seasons: Images and Translations stored", LogEventLevel.Verbose);
    }
}