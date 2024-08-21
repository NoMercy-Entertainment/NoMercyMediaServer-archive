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
public class SimilarPaletteJob : AbstractPaletteJob<Similar> {
    public override string QueueName => "image";
    public override int Priority => 2;
    
    public override async Task Handle() {
        await using var context = new MediaContext();

        IEnumerable<Similar> similars = context.Similar
            .Where(x => string.IsNullOrEmpty(x._colorPalette))
            .AsEnumerable()
            .Where(x => Storage
                .Any(y => y.MovieFromId == x.MovieFromId));

        foreach (Similar similar in similars)
        {
            similar._colorPalette = await MovieDbImage
                .MultiColorPalette([
                    new BaseImage.MultiStringType("poster", similar.Poster),
                    new BaseImage.MultiStringType("backdrop", similar.Backdrop)
                ]);
        }
        
        await context.SaveChangesAsync();
    }
}
