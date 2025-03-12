using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NoMercy.Database;
using Collection = NoMercy.Database.Models.Collection;


namespace NoMercy.Api.Controllers.V1.Media.DTO;

public record CollectionResponseDto
{
    [JsonProperty("nextId")] public object NextId { get; set; } = null!;
    [JsonProperty("data")] public CollectionResponseItemDto? Data { get; set; }

}
