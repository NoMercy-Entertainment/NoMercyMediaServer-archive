// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Microsoft.EntityFrameworkCore;
using NoMercy.Database;
using NoMercy.MediaProcessing.Images;
using Image=NoMercy.Database.Models.Image;

namespace NoMercy.MediaProcessing.Movies.PaletteJobs;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[Serializable]
public class ImagePaletteJob : AbstractPaletteJob<Image> {
    public override string QueueName => "image";
    public override int Priority => 2;

    public override async Task Handle() {
        await using var context = new MediaContext();

        List<Image> images = await context.Images
            .Where(x => Storage.Any(y => y.FilePath == x.FilePath))
            .ToListAsync();

        foreach (Image image in images) {
            if (image is not { _colorPalette: "" }) continue;

            image._colorPalette = await MovieDbImage.ColorPalette("image", image.FilePath);
        }
    }
}
