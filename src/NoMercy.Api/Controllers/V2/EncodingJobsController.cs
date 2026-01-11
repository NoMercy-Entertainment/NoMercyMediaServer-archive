using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NoMercy.Api.Controllers.V1;
using NoMercy.Database.Models;
using NoMercy.EncoderV2.Execution;
using NoMercy.EncoderV2.Profiles;
using NoMercy.EncoderV2.Repositories;
using NoMercy.EncoderV2.Tasks;

namespace NoMercy.Api.Controllers.V2;

/// <summary>
/// API endpoints for EncoderV2 job management
/// Handles job creation, execution, monitoring, and cancellation
/// Replaces V1 encoder system with new architecture
/// </summary>
[ApiController]
[ApiVersion(2.0)]
[Authorize]
[Route("api/v{version:apiVersion}/encoding")]
public class EncodingJobsController(
    IEncodingJobExecutor executor,
    IJobRepository jobRepository,
    IProfileRepository profileRepository) : BaseController
{
    // ============================================================================
    // JOB MANAGEMENT ENDPOINTS
    // ============================================================================

    /// <summary>
    /// Create a new encoding job
    /// </summary>
    [HttpPost("jobs")]
    public async Task<ActionResult<EncodingJob>> CreateJob([FromBody] CreateJobRequest request)
    {
        try
        {
            EncodingJob job = await executor.CreateJobAsync(
                request.InputFile,
                request.OutputFolder,
                request.ProfileId
            );

            return Ok(job);
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Create and immediately execute an encoding job
    /// </summary>
    [HttpPost("jobs/execute")]
    public async Task<ActionResult<EncodingJob>> CreateAndExecuteJob([FromBody] CreateJobRequest request)
    {
        try
        {
            EncodingJob job = await executor.CreateJobAsync(
                request.InputFile,
                request.OutputFolder,
                request.ProfileId
            );

            // Execute job asynchronously (fire and forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    await executor.ExecuteJobAsync(job.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EncoderV2] Job {job.Id} execution failed: {ex.Message}");
                }
            });

            return Ok(job);
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Execute an existing encoding job
    /// </summary>
    [HttpPost("jobs/{jobId}/execute")]
    public async Task<ActionResult> ExecuteJob(string jobId)
    {
        bool success = await executor.ExecuteJobAsync(jobId);

        if (!success)
        {
            return BadRequest(new { error = "Failed to execute job or job not found" });
        }

        return Ok(new { message = "Job execution started", jobId });
    }

    /// <summary>
    /// Get job status and progress
    /// </summary>
    [HttpGet("jobs/{jobId}/status")]
    public async Task<ActionResult<EncodingJobStatus>> GetJobStatus(string jobId)
    {
        try
        {
            EncodingJobStatus status = await executor.GetJobStatusAsync(jobId);
            return Ok(status);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get detailed job information
    /// </summary>
    [HttpGet("jobs/{jobId}")]
    public async Task<ActionResult<EncodingJob>> GetJob(string jobId)
    {
        EncodingJob? job = await jobRepository.GetJobAsync(jobId);

        if (job == null)
        {
            return NotFound(new { error = $"Job not found: {jobId}" });
        }

        return Ok(job);
    }

    /// <summary>
    /// List all encoding jobs with optional filtering
    /// </summary>
    [HttpGet("jobs")]
    public async Task<ActionResult<ListJobsResponse>> ListJobs(
        [FromQuery] string? state = null,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0)
    {
        List<EncodingJob> jobs = await jobRepository.ListJobsAsync(state, limit, offset);
        int total = await jobRepository.GetJobCountAsync(state);

        return Ok(new ListJobsResponse
        {
            Jobs = jobs,
            Total = total,
            Limit = limit,
            Offset = offset
        });
    }

    /// <summary>
    /// Cancel an encoding job
    /// </summary>
    [HttpPost("jobs/{jobId}/cancel")]
    public async Task<ActionResult> CancelJob(string jobId)
    {
        await executor.CancelJobAsync(jobId);
        return Ok(new { message = "Job cancelled", jobId });
    }

    /// <summary>
    /// Delete an encoding job (only if queued, completed, failed, or cancelled)
    /// </summary>
    [HttpDelete("jobs/{jobId}")]
    public async Task<ActionResult> DeleteJob(string jobId)
    {
        EncodingJob? job = await jobRepository.GetJobAsync(jobId);

        if (job == null)
        {
            return NotFound(new { error = $"Job not found: {jobId}" });
        }

        if (job.State == "processing")
        {
            return BadRequest(new { error = "Cannot delete a job that is currently processing. Cancel it first." });
        }

        await jobRepository.DeleteJobAsync(jobId);
        return Ok(new { message = "Job deleted", jobId });
    }

    // ============================================================================
    // PROFILE MANAGEMENT ENDPOINTS
    // ============================================================================

    /// <summary>
    /// List all encoding profiles
    /// </summary>
    [HttpGet("profiles")]
    public async Task<ActionResult<List<EncoderProfile>>> ListProfiles()
    {
        List<EncoderProfile> profiles = await profileRepository.ListProfilesAsync();
        return Ok(profiles);
    }

    /// <summary>
    /// Get a specific encoding profile
    /// </summary>
    [HttpGet("profiles/{profileId:ulid}")]
    public async Task<ActionResult<EncoderProfile>> GetProfile(Ulid profileId)
    {
        EncoderProfile? profile = await profileRepository.GetProfileAsync(profileId);

        if (profile == null)
        {
            return NotFound(new { error = $"Profile not found: {profileId}" });
        }

        return Ok(profile);
    }

    /// <summary>
    /// Create a new encoding profile
    /// </summary>
    [HttpPost("profiles")]
    public async Task<ActionResult<EncoderProfile>> CreateProfile([FromBody] EncoderProfile profile)
    {
        try
        {
            EncoderProfile created = await profileRepository.CreateProfileAsync(profile);
            return CreatedAtAction(nameof(GetProfile), new { profileId = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing encoding profile
    /// </summary>
    [HttpPut("profiles/{profileId:ulid}")]
    public async Task<ActionResult<EncoderProfile>> UpdateProfile(Ulid profileId, [FromBody] EncoderProfile profile)
    {
        EncoderProfile? existing = await profileRepository.GetProfileAsync(profileId);

        if (existing == null)
        {
            return NotFound(new { error = $"Profile not found: {profileId}" });
        }

        profile.Id = profileId;
        EncoderProfile updated = await profileRepository.UpdateProfileAsync(profile);
        return Ok(updated);
    }

    /// <summary>
    /// Delete an encoding profile
    /// </summary>
    [HttpDelete("profiles/{profileId:ulid}")]
    public async Task<ActionResult> DeleteProfile(Ulid profileId)
    {
        EncoderProfile? profile = await profileRepository.GetProfileAsync(profileId);

        if (profile == null)
        {
            return NotFound(new { error = $"Profile not found: {profileId}" });
        }

        await profileRepository.DeleteProfileAsync(profileId);
        return Ok(new { message = "Profile deleted", profileId });
    }

    /// <summary>
    /// Validate an encoding profile
    /// </summary>
    [HttpPost("profiles/validate")]
    public async Task<ActionResult<ProfileValidationResponse>> ValidateProfile([FromBody] EncoderProfile profile)
    {
        IProfileValidator validator = HttpContext.RequestServices.GetRequiredService<IProfileValidator>();
        ProfileValidationResult result = await validator.ValidateAsync(profile);

        return Ok(new ProfileValidationResponse
        {
            IsValid = result.IsValid,
            Errors = result.Errors,
            Warnings = result.Warnings
        });
    }

    // ============================================================================
    // TASK DISTRIBUTION ENDPOINTS
    // ============================================================================

    /// <summary>
    /// Get available task distribution strategies
    /// </summary>
    [HttpGet("strategies")]
    public ActionResult<List<StrategyInfo>> GetStrategies()
    {
        List<StrategyInfo> strategies =
        [
            new() { Name = "SingleTask", Description = "Encode entire file as one task (no splitting)" },
            new() { Name = "ByResolution", Description = "Split by resolution profile (1080p, 720p, 480p)" },
            new() { Name = "ByStreamType", Description = "Split by stream type (video, audio, subtitle)" },
            new() { Name = "BySegment", Description = "Split by HLS segments (10-second chunks)" }
        ];

        return Ok(strategies);
    }

    // ============================================================================
    // STATISTICS AND MONITORING ENDPOINTS
    // ============================================================================

    /// <summary>
    /// Get encoding statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<EncodingStats>> GetStats()
    {
        int totalJobs = await jobRepository.GetJobCountAsync(null);
        int queuedJobs = await jobRepository.GetJobCountAsync("queued");
        int processingJobs = await jobRepository.GetJobCountAsync("processing");
        int completedJobs = await jobRepository.GetJobCountAsync("completed");
        int failedJobs = await jobRepository.GetJobCountAsync("failed");

        return Ok(new EncodingStats
        {
            TotalJobs = totalJobs,
            QueuedJobs = queuedJobs,
            ProcessingJobs = processingJobs,
            CompletedJobs = completedJobs,
            FailedJobs = failedJobs
        });
    }
}

// ============================================================================
// REQUEST/RESPONSE MODELS
// ============================================================================

/// <summary>
/// Request model for creating an encoding job
/// </summary>
public class CreateJobRequest
{
    public required string InputFile { get; set; }
    public required string OutputFolder { get; set; }
    public required Ulid ProfileId { get; set; }
}

/// <summary>
/// Response model for listing jobs
/// </summary>
public class ListJobsResponse
{
    public List<EncodingJob> Jobs { get; set; } = [];
    public int Total { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
}

/// <summary>
/// Response model for profile validation
/// </summary>
public class ProfileValidationResponse
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
}

/// <summary>
/// Information about a task distribution strategy
/// </summary>
public class StrategyInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Encoding statistics
/// </summary>
public class EncodingStats
{
    public int TotalJobs { get; set; }
    public int QueuedJobs { get; set; }
    public int ProcessingJobs { get; set; }
    public int CompletedJobs { get; set; }
    public int FailedJobs { get; set; }
}
