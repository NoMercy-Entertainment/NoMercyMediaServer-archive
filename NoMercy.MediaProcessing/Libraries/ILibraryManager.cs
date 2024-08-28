namespace NoMercy.MediaProcessing.Libraries;

public interface ILibraryManager
{
    Task ProcessLibraryAsync(Ulid id);
}