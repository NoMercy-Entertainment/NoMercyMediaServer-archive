using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace NoMercy.Database.Models;

/// <summary>
/// Represents an encoding job in the EncoderV2 system
/// Stored in QueueContext for unified queue management
/// </summary>
[PrimaryKey(nameof(Id))]
public class EncodingJob
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("input_file_path")]
    public string InputFilePath { get; set; } = string.Empty;

    [JsonProperty("output_folder")]
    public string OutputFolder { get; set; } = string.Empty;

    [JsonProperty("profile_id")]
    public Ulid? ProfileId { get; set; }

    [ForeignKey(nameof(ProfileId))]
    public EncoderProfile? Profile { get; set; }

    [JsonProperty("profile_snapshot_json")]
    public string ProfileSnapshotJson { get; set; } = "{}";

    [JsonProperty("state")]
    public string State { get; set; } = "queued";

    [JsonProperty("error_message")]
    public string? ErrorMessage { get; set; }

    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty("started_at")]
    public DateTime? StartedAt { get; set; }

    [JsonProperty("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [JsonProperty("execution_time_ms")]
    public long? ExecutionTimeMs { get; set; }

    public ICollection<EncodingTask> Tasks { get; set; } = [];
}
