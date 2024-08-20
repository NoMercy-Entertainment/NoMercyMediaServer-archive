// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Microsoft.EntityFrameworkCore;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.MediaProcessing.Images;

namespace NoMercy.MediaProcessing.Movies.PaletteJobs;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[Serializable]
public class RecommendationPaletteJob : AbstractPaletteJob<Recommendation> {
    public override string QueueName => "image";
    public override int Priority => 2;
    
    public override async Task Handle() {
        await using var context = new MediaContext();

        List<Recommendation> recommendations = await context.Recommendations
            .Where(x => Storage
                .Any(y => y.MovieFromId == x.MovieFromId))
            .ToListAsync();

        foreach (Recommendation recommendation in recommendations)
        {
            if (recommendation is not { _colorPalette: "" }) continue;

            recommendation._colorPalette = await MovieDbImage
                .MultiColorPalette([
                    new BaseImage.MultiStringType("poster", recommendation.Poster),
                    new BaseImage.MultiStringType("backdrop", recommendation.Backdrop)
                ]);
        }
    }
}
