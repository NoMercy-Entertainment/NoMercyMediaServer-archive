
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace NoMercy.Database.Models;

[PrimaryKey(nameof(FileId), nameof(LibraryId))]
[Index(nameof(FileId))]
[Index(nameof(LibraryId))]
public class FileLibrary
{
    [JsonProperty("file_id")] public Ulid FileId { get; set; }
    public File File { get; set; }

    [JsonProperty("library_id")] public Ulid LibraryId { get; set; }
    public Library Library { get; set; }

    public FileLibrary(Ulid fileId, Ulid libraryId)
    {
        FileId = fileId;
        LibraryId = libraryId;
    }
}
