using Newtonsoft.Json;

namespace NoMercy.MediaProcessing.Seeds;

public class FolderDto
{
    [JsonProperty("id")] public Ulid Id { get; set; }
}