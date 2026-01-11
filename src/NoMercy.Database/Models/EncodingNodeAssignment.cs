using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace NoMercy.Database.Models;

/// <summary>
/// Tracks task assignments to encoder nodes
/// Used for monitoring and reassignment on node failure
/// </summary>
[PrimaryKey(nameof(Id))]
public class EncodingNodeAssignment
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("task_id")]
    public string TaskId { get; set; } = string.Empty;

    [JsonProperty("node_id")]
    public string NodeId { get; set; } = string.Empty;

    [JsonProperty("assigned_at")]
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty("completed_at")]
    public DateTime? CompletedAt { get; set; }
}
