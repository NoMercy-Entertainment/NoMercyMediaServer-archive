using NoMercy.Database.Models;

namespace NoMercy.MediaProcessing.Libraries;

public interface ILibraryRepository : IDisposable, IAsyncDisposable
{
    Task<Library?> GetLibraryWithFoldersAsync(Ulid id);
}