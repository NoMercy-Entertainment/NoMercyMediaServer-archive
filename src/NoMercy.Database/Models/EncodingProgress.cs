using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace NoMercy.Database.Models;

/// <summary>
/// Stores progress updates for encoding tasks
/// Used for real-time monitoring and resume capabilities
/// </summary>
[PrimaryKey(nameof(Id))]
public class EncodingProgress
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("task_id")]
    public string TaskId { get; set; } = string.Empty;

    [ForeignKey(nameof(TaskId))]
    public EncodingTask Task { get; set; } = null!;

    [JsonProperty("progress_percentage")]
    public double ProgressPercentage { get; set; }

    [JsonProperty("current_frame")]
    public long CurrentFrame { get; set; }

    [JsonProperty("fps")]
    public double Fps { get; set; }

    [JsonProperty("speed")]
    public double Speed { get; set; }

    [JsonProperty("bitrate")]
    public string Bitrate { get; set; } = string.Empty;

    [JsonProperty("current_time")]
    public TimeSpan CurrentTime { get; set; }

    [JsonProperty("estimated_remaining")]
    public TimeSpan EstimatedRemaining { get; set; }

    [JsonProperty("recorded_at")]
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
