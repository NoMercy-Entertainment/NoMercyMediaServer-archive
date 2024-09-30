using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NoMercy.Api.Controllers.V1.Dashboard.DTO;
using NoMercy.Api.Controllers.V1.DTO;
using NoMercy.Api.Controllers.V1.Music;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.MediaProcessing.Jobs.MediaJobs;
using NoMercy.Networking;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace NoMercy.Api.Controllers.V1.Dashboard;

[ApiController]
[Tags("Dashboard Tasks")]
[ApiVersion(1.0)]
[Authorize]
[Route("api/v{version:apiVersion}/dashboard/tasks", Order = 10)]
public class TasksController : BaseController
{
    [HttpGet]
    public IActionResult Index()
    {
        if (!User.IsModerator())
            return Unauthorized(new StatusResponseDto<string>
            {
                Status = "error",
                Message = "You do not have permission to view tasks"
            });

        List<TaskDto> list =
        [
            new TaskDto
            {
                Id = "pqiilkpnf8lmwrcxn0l8tngf",
                Title = "Scan media library",
                Value = 0,
                Type = "library",
                CreatedAt = DateTime.Parse("2024-01-25 09:26:56"),
                UpdatedAt = DateTime.Parse("2024-01-25 09:26:56")
            }
        ];

        return Ok(list);
    }

    [HttpPost]
    public IActionResult Store()
    {
        if (!User.IsModerator())
            return UnauthorizedResponse("You do not have permission to create tasks");

        return Ok(new PlaceholderResponse
        {
            Data = []
        });
    }

    [HttpPatch]
    public IActionResult Update()
    {
        if (!User.IsModerator())
            return UnauthorizedResponse("You do not have permission to update tasks");

        return Ok(new PlaceholderResponse
        {
            Data = []
        });
    }

    [HttpDelete]
    public IActionResult Destroy()
    {
        if (!User.IsModerator())
            return UnauthorizedResponse("You do not have permission to delete tasks");

        return Ok(new PlaceholderResponse
        {
            Data = []
        });
    }

    [HttpPost]
    [Route("pause")]
    public IActionResult PauseTask()
    {
        if (!User.IsModerator())
            return UnauthorizedResponse("You do not have permission to pause tasks");

        return Ok(new PlaceholderResponse
        {
            Data = []
        });
    }

    [HttpPost]
    [Route("resume")]
    public IActionResult ResumeTask()
    {
        if (!User.IsModerator())
            return UnauthorizedResponse("You do not have permission to resume tasks");

        return Ok(new PlaceholderResponse
        {
            Data = []
        });
    }

    [HttpGet]
    [Route("runners")]
    public IActionResult RunningTaskWorkers()
    {
        if (!User.IsModerator())
            return UnauthorizedResponse("You do not have permission to view task workers");

        return Ok(new PlaceholderResponse
        {
            Data = []
        });
    }

    [HttpGet]
    [Route("queue")]
    public async Task<IActionResult> EncoderQueue()
    {
        if (!User.IsModerator())
            return UnauthorizedResponse("You do not have permission to view encoder queue");

        await using MediaContext mediaContext = new();
        await using QueueContext queueContext = new();

        List<QueueJob> jobs = await queueContext.QueueJobs
            .Where(j => j.Queue == "encoder")
            .OrderByDescending(j => j.Priority)
            .ThenBy(j => j.CreatedAt)
            .ToListAsync();

        List<EncodeVideoJob> encoderJobs = [];
        foreach (QueueJob job in jobs)
        {
            EncodeVideoJob? encodeJob = JsonConvert.DeserializeObject<EncodeVideoJob>(job.Payload);
            if (encodeJob is not null) encoderJobs.Add(encodeJob);
        }

        // Load folders into memory first
        List<Folder> folders = await mediaContext.Folders
            .Include(f => f.EncoderProfileFolder)
                .ThenInclude(e => e.EncoderProfile)
            .Include(f => f.FolderLibraries)
                .ThenInclude(f => f.Library)
                    .ThenInclude(f => f.LibraryTvs)
                        .ThenInclude(libraryTv => libraryTv.Tv)
                            .ThenInclude(tv => tv.Episodes)
            .Include(f => f.FolderLibraries)
                .ThenInclude(f => f.Library)
                    .ThenInclude(f => f.LibraryMovies)
                        .ThenInclude(libraryMovie => libraryMovie.Movie)
            .ToListAsync();

        folders = folders
            .Where(f => encoderJobs.Any(j => j.FolderId == f.Id))
            .ToList();

        QueueJobDto[] queueJobs = encoderJobs
            .Select(j => new QueueJobDto
            {
                Id = j.Id,
                Title = GetTitle(folders, j),
                Type = j.GetType().Name,
                Status = j.Status.ToString(),
                InputFile = j.InputFile,
                Profile = folders.FirstOrDefault(f => f.Id == j.FolderId)?.EncoderProfileFolder.FirstOrDefault()?.EncoderProfile.Name
            }).ToArray();

        return Ok(new DataResponseDto<QueueJobDto[]>
        {
            Data = queueJobs
        });
    }

    private static string GetTitle(List<Folder> folders, EncodeVideoJob j)
    {
        Movie? movie = folders.FirstOrDefault(f => f.Id == j.FolderId)?.FolderLibraries
            .FirstOrDefault()?.Library.LibraryMovies.FirstOrDefault(m => m.MovieId == j.Id)?.Movie;

        Tv? tv = folders.FirstOrDefault(f => f.Id == j.FolderId)?.FolderLibraries
            .FirstOrDefault()?.Library.LibraryTvs.FirstOrDefault(m => m.Tv.Episodes.Any(e => e.Id == j.Id))?.Tv;

        Episode? episode = tv?.Episodes.FirstOrDefault(e => e.Id == j.Id);

        return movie?.CreateTitle() ?? episode?.CreateTitle() ?? string.Empty;
    }

    [HttpDelete]
    [Route("queue/{id:int}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        if (!User.IsModerator())
            return UnauthorizedResponse("You do not have permission to clear encoder queue");

        await using QueueContext queueContext = new();
        List<QueueJob> jobs = queueContext.QueueJobs
            .ToList();

        QueueJob? job = jobs.FirstOrDefault(j => JsonConvert.DeserializeObject<EncodeVideoJob>(j.Payload)?.Id == id);
        if (job is null)
            return NotFoundResponse("Job not found");

        queueContext.QueueJobs.Remove(job);
        await queueContext.SaveChangesAsync();

        return Ok(new PlaceholderResponse
        {
            Data = []
        });
    }
}

public class QueueJobDto
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("title")] public string Title { get; set; }
    [JsonProperty("type")] public string Type { get; set; }
    [JsonProperty("status")] public string Status { get; set; }
    [JsonProperty("input_file")] public string InputFile { get; set; }
    [JsonProperty("profile")] public string? Profile { get; set; }
}