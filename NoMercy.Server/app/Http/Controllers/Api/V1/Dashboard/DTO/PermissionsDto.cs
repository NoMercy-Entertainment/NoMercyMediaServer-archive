using Newtonsoft.Json;

namespace NoMercy.Server.app.Http.Controllers.Api.V1.Dashboard.DTO;

public record PermissionsDto
{
    [JsonProperty("edit")] public bool Edit { get; set; }
}