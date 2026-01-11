using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace NoMercy.Database.Models;

/// <summary>
/// Represents a distributed encoder node
/// Nodes can be local or remote machines running encoding tasks
/// </summary>
[PrimaryKey(nameof(Id))]
[Index(nameof(NodeId), IsUnique = true)]
[Index(nameof(IsHealthy))]
public class EncoderNode : Timestamps
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [JsonProperty("id")]
    public Ulid Id { get; set; } = Ulid.NewUlid();

    [JsonProperty("node_id")]
    public string NodeId { get; set; } = string.Empty;

    [JsonProperty("node_name")]
    public string NodeName { get; set; } = string.Empty;

    [JsonProperty("network_address")]
    public string NetworkAddress { get; set; } = string.Empty;

    [JsonProperty("network_port")]
    public int NetworkPort { get; set; } = 9626;

    [JsonProperty("use_https")]
    public bool UseHttps { get; set; } = false;

    [JsonProperty("is_active")]
    public bool IsActive { get; set; } = true;

    [JsonProperty("is_healthy")]
    public bool IsHealthy { get; set; } = true;

    [JsonProperty("last_heartbeat")]
    public DateTime? LastHeartbeat { get; set; }

    [JsonProperty("version")]
    public string? Version { get; set; }

    [JsonProperty("has_gpu")]
    public bool HasGPU { get; set; }

    [JsonProperty("gpu_model")]
    public string? GPUModel { get; set; }

    [JsonProperty("cpu_cores")]
    public int CPUCores { get; set; }

    [JsonProperty("memory_gb")]
    public int MemoryGB { get; set; }
}
