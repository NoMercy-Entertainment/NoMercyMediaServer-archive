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
public class PersonPaletteJob : AbstractPaletteJob<Person> {
    public override string QueueName => "image";
    public override int Priority => 2;

    public override async Task Handle() {
        await using var context = new MediaContext();

        IEnumerable<Person> people = context.People
            .Where(x => string.IsNullOrEmpty(x._colorPalette))
            .AsEnumerable()
            .Where(x => Storage
                .Any(y => y.Profile == x.Profile));

        foreach (var person in people) {
            person._colorPalette = await MovieDbImage
                .ColorPalette("person", person.Profile);
        }

        await context.SaveChangesAsync();
    }
}
