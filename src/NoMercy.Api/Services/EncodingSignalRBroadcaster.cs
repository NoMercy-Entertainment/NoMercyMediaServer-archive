using Microsoft.AspNetCore.SignalR;
using NoMercy.Api.Controllers.Socket;
using NoMercy.EncoderV2.Progress;

namespace NoMercy.Api.Services;

/// <summary>
/// SignalR broadcaster implementation for encoding progress updates.
/// Bridges the EncoderV2 layer with the API SignalR hub.
/// </summary>
public class EncodingSignalRBroadcaster(IHubContext<EncodingHub> hubContext) : ISignalRBroadcaster
{
    private readonly IHubContext<EncodingHub> _hubContext = hubContext;

    public async Task BroadcastProgressAsync(string jobId, string taskId, EncodingProgressInfo progress)
    {
        EncodingProgressDto dto = new()
        {
            JobId = jobId,
            TaskId = taskId,
            ProgressPercentage = progress.ProgressPercentage,
            CurrentFrame = progress.CurrentFrame,
            Fps = progress.Fps,
            Speed = progress.Speed,
            Bitrate = progress.Bitrate,
            CurrentTime = FormatTimeSpan(progress.CurrentTime),
            EstimatedRemaining = FormatTimeSpan(progress.EstimatedRemaining),
            TotalDuration = FormatTimeSpan(progress.TotalDuration),
            Timestamp = DateTime.UtcNow
        };

        // Send to clients subscribed to this specific job
        if (!string.IsNullOrEmpty(jobId))
        {
            await _hubContext.Clients.Group($"job-{jobId}").SendAsync("EncodingProgress", dto);
        }

        // Also send to clients subscribed to all jobs
        await _hubContext.Clients.Group("all-encoding-jobs").SendAsync("EncodingProgress", dto);
    }

    public async Task BroadcastJobStateChangeAsync(string jobId, string previousState, string newState, string? errorMessage)
    {
        JobStateChangeDto dto = new()
        {
            JobId = jobId,
            PreviousState = previousState,
            NewState = newState,
            ErrorMessage = errorMessage,
            Timestamp = DateTime.UtcNow
        };

        if (!string.IsNullOrEmpty(jobId))
        {
            await _hubContext.Clients.Group($"job-{jobId}").SendAsync("JobStateChanged", dto);
        }

        await _hubContext.Clients.Group("all-encoding-jobs").SendAsync("JobStateChanged", dto);
    }

    public async Task BroadcastTaskStateChangeAsync(string jobId, string taskId, string taskType, string previousState, string newState, string? errorMessage)
    {
        TaskStateChangeDto dto = new()
        {
            JobId = jobId,
            TaskId = taskId,
            TaskType = taskType,
            PreviousState = previousState,
            NewState = newState,
            ErrorMessage = errorMessage,
            Timestamp = DateTime.UtcNow
        };

        if (!string.IsNullOrEmpty(jobId))
        {
            await _hubContext.Clients.Group($"job-{jobId}").SendAsync("TaskStateChanged", dto);
        }

        await _hubContext.Clients.Group("all-encoding-jobs").SendAsync("TaskStateChanged", dto);
    }

    private static string FormatTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan.TotalHours >= 1)
        {
            return $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
        return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }
}
