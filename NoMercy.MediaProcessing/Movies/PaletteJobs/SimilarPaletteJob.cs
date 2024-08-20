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
public class SimilarPaletteJob : AbstractPaletteJob<Similar> {
    public override string QueueName => "image";
    public override int Priority => 2;
    
    public override async Task Handle() {
        await using var context = new MediaContext();

        List<Similar> similars = await context.Similar
            .Where(x => Storage
                .Any(y => y.MovieFromId == x.MovieFromId))
            .ToListAsync();

        foreach (Similar similar in similars)
        {
            if (similar is not { _colorPalette: "" }) continue;

            similar._colorPalette = await MovieDbImage
                .MultiColorPalette([
                    new BaseImage.MultiStringType("poster", similar.Poster),
                    new BaseImage.MultiStringType("backdrop", similar.Backdrop)
                ]);
        }
    }
}
