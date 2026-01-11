using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using NoMercy.Networking;
using NoMercy.NmSystem.SystemCalls;

namespace NoMercy.Api.Controllers.Socket;

/// <summary>
/// SignalR hub for real-time encoding progress updates.
/// Clients can subscribe to specific job progress or all encoding events.
/// </summary>
public class EncodingHub : ConnectionHub
{
    public EncodingHub(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        Logger.Socket($"Encoding client connected: {Context.ConnectionId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        Logger.Socket($"Encoding client disconnected: {Context.ConnectionId}");
    }

    /// <summary>
    /// Subscribe to progress updates for a specific encoding job
    /// </summary>
    /// <param name="jobId">The job ID to subscribe to</param>
    public async Task SubscribeToJob(string jobId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"job-{jobId}");
        Logger.Socket($"Client {Context.ConnectionId} subscribed to job {jobId}");
    }

    /// <summary>
    /// Unsubscribe from progress updates for a specific encoding job
    /// </summary>
    /// <param name="jobId">The job ID to unsubscribe from</param>
    public async Task UnsubscribeFromJob(string jobId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"job-{jobId}");
        Logger.Socket($"Client {Context.ConnectionId} unsubscribed from job {jobId}");
    }

    /// <summary>
    /// Subscribe to all encoding events (for dashboard/monitoring)
    /// </summary>
    public async Task SubscribeToAllJobs()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "all-encoding-jobs");
        Logger.Socket($"Client {Context.ConnectionId} subscribed to all encoding jobs");
    }

    /// <summary>
    /// Unsubscribe from all encoding events
    /// </summary>
    public async Task UnsubscribeFromAllJobs()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "all-encoding-jobs");
        Logger.Socket($"Client {Context.ConnectionId} unsubscribed from all encoding jobs");
    }
}

/// <summary>
/// Static helper for broadcasting encoding progress from services
/// </summary>
public static class EncodingHubBroadcaster
{
    private static IHubContext<EncodingHub>? _hubContext;

    /// <summary>
    /// Initialize the broadcaster with the hub context (called during startup)
    /// </summary>
    public static void Initialize(IHubContext<EncodingHub> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// Broadcast progress update for a specific job
    /// </summary>
    public static async Task BroadcastJobProgressAsync(string jobId, EncodingProgressDto progress)
    {
        if (_hubContext == null) return;

        // Send to clients subscribed to this specific job
        await _hubContext.Clients.Group($"job-{jobId}").SendAsync("EncodingProgress", progress);

        // Also send to clients subscribed to all jobs
        await _hubContext.Clients.Group("all-encoding-jobs").SendAsync("EncodingProgress", progress);
    }

    /// <summary>
    /// Broadcast job state change (started, completed, failed, cancelled)
    /// </summary>
    public static async Task BroadcastJobStateChangeAsync(string jobId, JobStateChangeDto stateChange)
    {
        if (_hubContext == null) return;

        await _hubContext.Clients.Group($"job-{jobId}").SendAsync("JobStateChanged", stateChange);
        await _hubContext.Clients.Group("all-encoding-jobs").SendAsync("JobStateChanged", stateChange);
    }

    /// <summary>
    /// Broadcast task state change within a job
    /// </summary>
    public static async Task BroadcastTaskStateChangeAsync(string jobId, string taskId, TaskStateChangeDto stateChange)
    {
        if (_hubContext == null) return;

        await _hubContext.Clients.Group($"job-{jobId}").SendAsync("TaskStateChanged", stateChange);
        await _hubContext.Clients.Group("all-encoding-jobs").SendAsync("TaskStateChanged", stateChange);
    }
}

/// <summary>
/// DTO for encoding progress updates sent via SignalR
/// </summary>
public class EncodingProgressDto
{
    public string JobId { get; set; } = string.Empty;
    public string TaskId { get; set; } = string.Empty;
    public double ProgressPercentage { get; set; }
    public long CurrentFrame { get; set; }
    public double Fps { get; set; }
    public double Speed { get; set; }
    public string Bitrate { get; set; } = string.Empty;
    public string CurrentTime { get; set; } = string.Empty;
    public string EstimatedRemaining { get; set; } = string.Empty;
    public string TotalDuration { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO for job state changes sent via SignalR
/// </summary>
public class JobStateChangeDto
{
    public string JobId { get; set; } = string.Empty;
    public string PreviousState { get; set; } = string.Empty;
    public string NewState { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO for task state changes sent via SignalR
/// </summary>
public class TaskStateChangeDto
{
    public string JobId { get; set; } = string.Empty;
    public string TaskId { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty;
    public string PreviousState { get; set; } = string.Empty;
    public string NewState { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
