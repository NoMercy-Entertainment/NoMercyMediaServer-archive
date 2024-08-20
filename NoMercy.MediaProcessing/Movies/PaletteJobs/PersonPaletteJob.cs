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
public class PersonPaletteJob : AbstractPaletteJob<Person> {
    public override string QueueName => "image";
    public override int Priority => 2;

    public override async Task Handle() {
        await using var context = new MediaContext();

        List<Person> people = await context.People
            .Where(x => Storage
                .Any(y => y.Profile == x.Profile))
            .ToListAsync();

        foreach (var person in people) {
            if (person is not { _colorPalette: "" }) continue;

            person._colorPalette = await MovieDbImage
                .ColorPalette("person", person.Profile);
        }

        await context.SaveChangesAsync();
    }
}
