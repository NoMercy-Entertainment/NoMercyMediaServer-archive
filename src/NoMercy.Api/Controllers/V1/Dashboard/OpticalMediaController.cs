using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NoMercy.Helpers;
using NoMercy.MediaSources.OpticalMedia;
using NoMercy.MediaSources.OpticalMedia.Dto;
using NoMercy.NmSystem.SystemCalls;

namespace NoMercy.Api.Controllers.V1.Dashboard;

[ApiController]
[Tags("Dashboard Optical")]
[ApiVersion(1.0)]
[Authorize]
[Route("api/v{version:apiVersion}/dashboard/optical")]
public class OpticalMediaController : BaseController
{
    [HttpGet("drives")]
    public IActionResult GetOpticalDrives()
    {
        if (!User.IsModerator())
            return UnauthorizedResponse("You do not have permission to view optical drives");
        
        Dictionary<string, string> drives = Optical.GetOpticalDrives();
        
        return Ok(drives);
    }
    
    [HttpGet("{drivePath}")]
    public IActionResult GetDriveContents(string drivePath)
    {
        if (!User.IsModerator())
            return UnauthorizedResponse("You do not have permission to view drive contents");
        
        MetaData? metadata = DriveMonitor.GetDriveMetadata(drivePath);
        if (metadata == null)
        {
            return NotFound("Drive metadata not found.");
        }
        return Ok(metadata);
    }

    [HttpPost("{drivePath}/process")]
    public IActionResult ProcessMedia(string drivePath, [FromBody] MediaProcessingRequest request)
    {
        if (!User.IsModerator())
            return UnauthorizedResponse("You do not have permission to process media");
        
        if (string.IsNullOrWhiteSpace(drivePath))
        {
            return BadRequest("Drive path is required.");
        }
        
        if (request == null)
        {
            return BadRequest("Request is required.");
        }
        
        if (string.IsNullOrWhiteSpace(request.PlaylistId) && string.IsNullOrWhiteSpace(request.MovieId) && string.IsNullOrWhiteSpace(request.EpisodeId))
        {
            return BadRequest("PlaylistId, MovieId or EpisodeId is required.");
        }
        
        DriveMonitor.ProcessMedia(drivePath, request);
        
        return Ok("Processing started.");
    }
    
    [HttpPost("{drivePath}/open")]
    public IActionResult OpenDrive(string drivePath)
    {
        if (!User.IsModerator())
            return UnauthorizedResponse("You do not have permission to open drive");
        
        if (string.IsNullOrWhiteSpace(drivePath))
        {
            return BadRequest("Drive path is required.");
        }
        
        bool success = Optical.OpenDrive(drivePath);
        
        if (!success)
        {
            return BadRequest("Failed to open drive.");
        }
        
        return Ok("Drive opened.");
    }
    
    [HttpPost("{drivePath}/close")]
    public IActionResult CloseDrive(string drivePath)
    {
        if (!User.IsModerator())
            return UnauthorizedResponse("You do not have permission to close drive");
        
        if (string.IsNullOrWhiteSpace(drivePath))
        {
            return BadRequest("Drive path is required.");
        }
        
        bool success = Optical.CloseDrive(drivePath);
        
        if (!success)
        {
            return BadRequest("Failed to close drive.");
        }
        
        return Ok("Drive closed.");
    }
}