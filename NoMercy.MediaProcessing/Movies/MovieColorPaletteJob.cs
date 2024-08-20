using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.MediaProcessing.Images;
using NoMercy.Queue;

namespace NoMercy.MediaProcessing.Movies;

[Serializable]
public class MovieColorPaletteJob : IShouldQueue, IDisposable
{
    public int Id { get; set; }
    
    public IEnumerable<Image>? Images { get; set; } = [];
    public IEnumerable<Similar>? Similar { get; set; } = [];
    public IEnumerable<Recommendation>? Recommendations { get; set; } = [];
    public IEnumerable<Person>? People { get; set; } = [];

    public MovieColorPaletteJob()
    {
    }
    
    public MovieColorPaletteJob(int id,  IEnumerable<Image> images)
    {
        Id = id;
        Images = images;
    }

    public MovieColorPaletteJob(int id, IEnumerable<Similar> similar)
    {
        Id = id;
        Similar = similar;
    }

    public MovieColorPaletteJob(int id, IEnumerable<Recommendation> recommendations)
    {
        Id = id;
        Recommendations = recommendations;
    }
    
    public MovieColorPaletteJob(int id, IEnumerable<Person> people)
    {
        Id = id;
        People = people;
    }

    public async Task Handle()
    {
        await using var context = new MediaContext();

        if (Images != null && Images.Any())
        {
            var images = context.Images
                .Where(x => Images
                    .Any(y => y.FilePath == x.FilePath))
                .ToList();

            foreach (var image in images)
            {
                if (image is not { _colorPalette: "" }) continue;

                image._colorPalette = await MovieDbImage
                    .ColorPalette("image", image.FilePath);
            }
        }

        if (Similar != null && Similar.Any())
        {
            var similars = context.Similar
                .Where(x => Similar
                    .Any(y => y.MovieFromId == x.MovieFromId))
                .ToList();

            foreach (var similar in similars)
            {
                if (similar is not { _colorPalette: "" }) continue;

                similar._colorPalette = await MovieDbImage
                    .MultiColorPalette([
                        new BaseImage.MultiStringType("poster", similar.Poster),
                        new BaseImage.MultiStringType("backdrop", similar.Backdrop)
                    ]);
            }
        }

        if (Recommendations != null && Recommendations.Any())
        {
            var recommendations = context.Recommendations
                .Where(x => Recommendations
                    .Any(y => y.MovieFromId == x.MovieFromId))
                .ToList();

            foreach (var recommendation in recommendations)
            {
                if (recommendation is not { _colorPalette: "" }) continue;

                recommendation._colorPalette = await MovieDbImage
                    .MultiColorPalette([
                        new BaseImage.MultiStringType("poster", recommendation.Poster),
                        new BaseImage.MultiStringType("backdrop", recommendation.Backdrop)
                    ]);
            }
        }

        if (People != null && People.Any())
        {
            var people = context.People
                .Where(x => People
                    .Any(y => y.Profile == x.Profile))
                .ToList();

            foreach(var person in people)
            {
                if (person is not { _colorPalette: "" }) continue;

                person._colorPalette = await MovieDbImage
                    .ColorPalette("person", person.Profile);
            }

            await context.SaveChangesAsync();

            await Task.CompletedTask;
        }
    }

    private void ReleaseUnmanagedResources()
    {
        Images = null;
        Similar = null;
        Recommendations = null;
        People = null;
        
        GC.Collect();
        GC.WaitForFullGCComplete();
        GC.WaitForPendingFinalizers();
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~MovieColorPaletteJob()
    {
        ReleaseUnmanagedResources();
    }
}