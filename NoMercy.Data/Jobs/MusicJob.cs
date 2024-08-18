using NoMercy.Data.Logic;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.Helpers.system;
using NoMercy.NmSystem;

namespace NoMercy.Data.Jobs;

public class ParsedTrack
{
    public string LibraryFolder { get; set; } = "";
    public string? Letter { get; set; }
    public string? Type { get; set; }
    public string? Artist { get; set; }
    public int Year { get; set; }
    public int? AlbumNumber { get; set; }
    public int TrackNumber { get; set; }
    public string? Album { get; set; }
    public string Title { get; set; } = "";
    public string? Artists { get; set; }
    public string? Featuring { get; set; }
    public string? Duplicate { get; set; }
    public string Extension { get; set; } = "";
}

[Serializable]
public class MusicJob : IShouldQueue, IDisposable, IAsyncDisposable
{
    private readonly MediaContext _mediaContext = new();

    public string? Folder { get; set; }
    public Library? Library { get; set; }

    public MusicJob()
    {
        //
    }


    public MusicJob(string folder, Library library)
    {
        Folder = folder;
        Library = library;
    }

    public async Task Handle()
    {
        if (Folder is null) return;
        if (Library is null) return;

        await using MediaScan mediaScan = new();
        IEnumerable<MediaFolder> mediaFolder = await mediaScan
            .EnableFileListing()
            .DisableRegexFilter()
            .Process(Folder, 20);

        foreach (MediaFolder list in mediaFolder)
        {
            Logger.App($"Music {list.Path}: Processing");

            MusicLogic3 music = new(Library, list);
            await music.Process();
        }
    }

    ~MusicJob()
    {
        Dispose();
    }

    public void Dispose()
    {
        _mediaContext.Dispose();
        GC.Collect();
        GC.WaitForFullGCComplete();
        GC.WaitForPendingFinalizers();
    }

    public async ValueTask DisposeAsync()
    {
        await _mediaContext.DisposeAsync();
        GC.Collect();
        GC.WaitForFullGCComplete();
        GC.WaitForPendingFinalizers();
    }
}