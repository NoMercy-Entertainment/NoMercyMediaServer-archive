// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.MediaProcessing.Images;

namespace NoMercy.MediaProcessing.Jobs.PaletteJobs;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[Serializable]
public class RecommendationPaletteJob : AbstractPaletteJob<Recommendation> {
    public override string QueueName => "image";
    public override int Priority => 2;
    
    public override async Task Handle() {
        await using var context = new MediaContext();

        IEnumerable<Recommendation> recommendations = context.Recommendations
            .Where(x => string.IsNullOrEmpty(x._colorPalette))
            .AsEnumerable()
            .Where(x => Storage
                .Any(y => y.MovieFromId == x.MovieFromId));

        foreach (Recommendation recommendation in recommendations)
        {
            recommendation._colorPalette = await MovieDbImage
                .MultiColorPalette([
                    new BaseImage.MultiStringType("poster", recommendation.Poster),
                    new BaseImage.MultiStringType("backdrop", recommendation.Backdrop)
                ]);
        }
        
        await context.SaveChangesAsync();
    }
}
