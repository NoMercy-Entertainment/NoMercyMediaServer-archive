using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace NoMercy.Database.Models;

/// <summary>
/// Represents a single encoding task within a job
/// Tasks can be distributed across multiple nodes
/// </summary>
[PrimaryKey(nameof(Id))]
public class EncodingTask
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("job_id")]
    public string JobId { get; set; } = string.Empty;

    [ForeignKey(nameof(JobId))]
    public EncodingJob Job { get; set; } = null!;

    [JsonProperty("task_type")]
    public string TaskType { get; set; } = string.Empty;

    [JsonProperty("weight")]
    public double Weight { get; set; }

    [JsonProperty("state")]
    public string State { get; set; } = "pending";

    [JsonProperty("assigned_node_id")]
    public Ulid? AssignedNodeId { get; set; }

    [ForeignKey(nameof(AssignedNodeId))]
    public EncoderNode? AssignedNode { get; set; }

    [JsonProperty("retry_count")]
    public int RetryCount { get; set; }

    [JsonProperty("dependencies_json")]
    public string DependenciesJson { get; set; } = "[]";

    [JsonProperty("error_message")]
    public string? ErrorMessage { get; set; }

    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty("started_at")]
    public DateTime? StartedAt { get; set; }

    [JsonProperty("completed_at")]
    public DateTime? CompletedAt { get; set; }

    public ICollection<EncodingProgress> ProgressUpdates { get; set; } = [];
}
