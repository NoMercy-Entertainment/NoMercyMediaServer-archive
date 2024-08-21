// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------

using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.MediaProcessing.Images;

namespace NoMercy.MediaProcessing.Jobs.PaletteJobs;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[Serializable]
public class ImagePaletteJob : AbstractPaletteJob<Image> {
    public override string QueueName => "image";
    public override int Priority => 2;

    public override async Task Handle() {
        await using var context = new MediaContext();

        IEnumerable<Image> images = context.Images
            .Where(x => string.IsNullOrEmpty(x._colorPalette))
            .AsEnumerable()
            .Where(x => Storage.Any(y => y.FilePath == x.FilePath));

        foreach (Image image in images) {
            image._colorPalette = await MovieDbImage.ColorPalette("image", image.FilePath);
        }
        
        await context.SaveChangesAsync();
    }
}
