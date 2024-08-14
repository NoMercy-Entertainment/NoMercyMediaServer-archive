#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.Helpers;
using NoMercy.Networking;
using NoMercy.Server.app.Helper;
using NoMercy.Server.app.Http.Controllers.Api.V1.Dashboard.DTO;
using NoMercy.Server.app.Http.Controllers.Api.V1.DTO;
using NoMercy.Server.app.Http.Middleware;
using NoMercy.Server.Logic;

namespace NoMercy.Server.app.Http.Controllers.Api.V1.Dashboard;

[ApiController]
[Tags("Dashboard Libraries")]
[ApiVersion("1")]
[Authorize]
[Route("api/v{Version:apiVersion}/dashboard/libraries", Order = 10)]
public class LibrariesController: BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view libraries");

        await using MediaContext mediaContext = new();
        var libraries = await mediaContext.Libraries
            .AsNoTracking()
            
            .Include(library => library.LibraryUsers)
                .ThenInclude(libraryUser => libraryUser.User)
            
            .Include(library => library.LibraryTvs
                .Where(libraryTv => libraryTv.Library.Type == "anime" || libraryTv.Library.Type == "tv")
                .Where(libraryTv => libraryTv.Tv.Backdrop != null)
            )
                .ThenInclude(libraryTv => libraryTv.Tv)
            
            .Include(library => library.LibraryMovies
                .Where(libraryTv => libraryTv.Library.Type == "movie")
                .Where(libraryMovie => libraryMovie.Movie.Backdrop != null))
                .ThenInclude(libraryMovie => libraryMovie.Movie)
            
            .Include(library => library.FolderLibraries)
                .ThenInclude(folderLibrary => folderLibrary.Folder)
                    .ThenInclude(library => library.EncoderProfileFolder)
                        .ThenInclude(encoderProfileFolder => encoderProfileFolder.EncoderProfile)
            
            .Include(library => library.LanguageLibraries)
                .ThenInclude(languageLibrary => languageLibrary.Language)
            
            .Where(library => library.LibraryUsers
                .FirstOrDefault(u => u.UserId == userId) != null)
            
            .OrderBy(library => library.Order)
            .ToListAsync();

        return Ok(new LibrariesDto
        {
            Data = libraries.Select(library => new LibrariesResponseItemDto(library))
        });
    }

    [HttpPost]
    public async Task<IActionResult> Store()
    {
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsModerator())
            return UnauthorizedResponse("You do not have permission to create a new library");

        try
        {
            await using MediaContext mediaContext = new();
            var libraries = await mediaContext.Libraries.CountAsync();

            Library library = new()
            {
                Id = Ulid.NewUlid(),
                Title = $"Library {libraries}",
                AutoRefreshInterval = 30,
                ChapterImages = true,
                ExtractChapters = true,
                ExtractChaptersDuring = true,
                PerfectSubtitleMatch = true,
                Realtime = true,
                SpecialSeasonName = "Specials",
                Type = "",
                Order = 99
            };

            await mediaContext.Libraries.Upsert(library)
                .On(l => new { l.Id })
                .WhenMatched((ls, li) => new Library
                {
                    Title = li.Title,
                    AutoRefreshInterval = li.AutoRefreshInterval,
                    ChapterImages = li.ChapterImages,
                    ExtractChapters = li.ExtractChapters,
                    ExtractChaptersDuring = li.ExtractChaptersDuring,
                    PerfectSubtitleMatch = li.PerfectSubtitleMatch,
                    Realtime = li.Realtime,
                    SpecialSeasonName = li.SpecialSeasonName,
                    Type = li.Type,
                    Order = li.Order
                })
                .RunAsync();
            
            await mediaContext.LibraryUser.Upsert(new LibraryUser
                {
                    LibraryId = library.Id,
                    UserId = userId
                })
                .On(lu => new { lu.LibraryId, lu.UserId })
                .WhenMatched((lus, lui) => new LibraryUser
                {
                    LibraryId = lui.LibraryId,
                    UserId = lui.UserId
                })
                .RunAsync();

            return Ok(new StatusResponseDto<Library>
            {
                Status = "ok",
                Data = library,
                Message = "Successfully created a new library.",
                Args = []
            });
        }
        catch (Exception e)
        {
            return Ok(new StatusResponseDto<Library>()
            {
                Status = "error",
                Message = "Something went wrong creating a new library: {0}",
                Args = [e.Message]
            });
        }
    }

    [HttpPatch]
    [Route("{id:ulid}")]
    public async Task<IActionResult> Update(Ulid id, [FromBody] LibraryUpdateRequest request)
    {
        if (!HttpContext.User.IsModerator())
            return UnauthorizedResponse("You do not have permission to update the library");
        
        await using MediaContext mediaContext = new();
        var library = await mediaContext.Libraries
            .Include(library => library.LanguageLibraries)
            .Where(library => library.Id == id)
            .FirstOrDefaultAsync();

        if (library is null)
            return Ok(new StatusResponseDto<string>()
            {
                Status = "error",
                Message = "Library {0} does not exist.",
                Args = [id.ToString()]
            });

        try
        {
            library.Title = request.Title;
            library.PerfectSubtitleMatch = request.PerfectSubtitleMatch;
            library.Realtime = request.Realtime;
            library.SpecialSeasonName = request.SpecialSeasonName;
            library.Type = request.Type;
            
            library.LanguageLibraries.Clear();
            
            var languages = await mediaContext.Languages
                .ToListAsync();
            
            foreach (var subtitle in request.Subtitles)
            {
                var language = languages.FirstOrDefault(l => l.Iso6391 == subtitle);
                if (language is null) continue;
            
                library.LanguageLibraries.Add(new LanguageLibrary
                {
                    LibraryId = library.Id,
                    LanguageId = language.Id
                });
                
            }

            await mediaContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return Ok(new StatusResponseDto<string>()
            {
                Status = "error",
                Message = "Something went wrong updating the library: {0}",
                Args = [e.Message]
            });
        }

        try
        {
            var folders = await mediaContext.Folders
                .Where(folder => request.FolderLibrary.Select(f => f.FolderId).Contains(folder.Id))
                .ToListAsync();

            var folderLibraries = folders.Select(folder => new FolderLibrary
            {
                LibraryId = library.Id,
                FolderId = folder.Id
            }).ToArray();

            await mediaContext.FolderLibrary.UpsertRange(folderLibraries)
                .On(fl => new { fl.LibraryId, fl.FolderId })
                .WhenMatched((fls, fli) => new FolderLibrary
                {
                    LibraryId = fli.LibraryId,
                    FolderId = fli.FolderId
                })
                .RunAsync();
        }
        catch (Exception e)
        {
            return Ok(new StatusResponseDto<string>()
            {
                Status = "error",
                Message = "Something went wrong updating the library folders: {0}",
                Args = [e.Message]
            });
        }

        try
        {
            var languages = await mediaContext.LanguageLibrary
                .Where(language => request.Subtitles.Contains(language.Language.Iso6391))
                .ToListAsync();

            var languageLibraries = languages.Select(language => new LanguageLibrary
            {
                LibraryId = library.Id,
                LanguageId = language.LanguageId
            }).ToArray();

            await mediaContext.LanguageLibrary.UpsertRange(languageLibraries)
                .On(ll => new { ll.LibraryId, ll.LanguageId })
                .WhenMatched((lls, lli) => new LanguageLibrary
                {
                    LibraryId = lli.LibraryId,
                    LanguageId = lli.LanguageId
                })
                .RunAsync();
        }
        catch (Exception e)
        {
            return Ok(new StatusResponseDto<string>()
            {
                Status = "error",
                Message = "Something went wrong updating the library subtitles: {0}",
                Args = [e.Message]
            });
        }

        try
        {
            var folders = request.FolderLibrary
                .Select(f => f.Folder)
                .ToList();

            foreach (var folder in folders)
            {
                var encoderProfileFolders = folder.EncoderProfiles
                    .Select(profile => new EncoderProfileFolder
                    {
                        FolderId = folder.Id,
                        EncoderProfileId = profile
                    })
                    .ToArray();

                await mediaContext.EncoderProfileFolder.UpsertRange(encoderProfileFolders)
                    .On(epl => new { epl.FolderId, epl.EncoderProfileId })
                    .WhenMatched((epls, epli) => new EncoderProfileFolder()
                    {
                        FolderId = epli.FolderId,
                        EncoderProfileId = epli.EncoderProfileId
                    })
                    .RunAsync();
            }
        }
        catch (Exception e)
        {
            Logger.App(e);
            return Ok(new StatusResponseDto<string>()
            {
                Status = "error",
                Message = "Something went wrong updating the library encoder profiles: {0}",
                Args = [e.Message]
            });
        }

        return Ok(new StatusResponseDto<string>()
        {
            Status = "ok",
            Message = "Successfully updated {0} library.",
            Args = [library.Title]
        });
    }

    [HttpDelete]
    [Route("{id:ulid}")]
    public async Task<IActionResult> Delete(Ulid id)
    {
        if (!HttpContext.User.IsModerator())
            return UnauthorizedResponse("You do not have permission to delete the library");
        
        try
        {
            await using MediaContext mediaContext = new();
            var library = await mediaContext.Libraries.FindAsync(id);

            if (library is null)
                return Ok(new StatusResponseDto<string>()
                {
                    Status = "error",
                    Message = "Library {0} does not exist.",
                    Args = [id.ToString()]
                });

            mediaContext.Libraries.Remove(library);
            await mediaContext.SaveChangesAsync();
            
            return Ok(new StatusResponseDto<string>()
            {
                Status = "ok",
                Message = "Successfully deleted {0} library.",
                Args = [library.Title]
            });
        }
        catch (Exception e)
        {
            return Ok(new StatusResponseDto<string>()
            {
                Status = "error",
                Message = "Something went wrong deleting the library: {0}",
                Args = [e.Message]
            });
        }
    }

    [HttpPatch]
    [Route("sort")]
    public async Task<IActionResult> Sort(Ulid id, [FromBody] LibrarySortRequest request)
    {
        if (!HttpContext.User.IsModerator())
            return UnauthorizedResponse("You do not have permission to sort the libraries");
        
        await using MediaContext mediaContext = new();
        var libraries = await mediaContext.Libraries
            .AsTracking()
            .ToListAsync();

        if (libraries.Count == 0)
            return Ok(new StatusResponseDto<string>()
            {
                Status = "error",
                Message = "No libraries exist.",
                Args = []
            });

        try
        {
            foreach (var item in request.Libraries)
            {
                var lib = libraries.FirstOrDefault(l => l.Id == item.Id);
                if (lib is null) continue;
                lib.Order = item.Order;
            }

            await mediaContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return Problem(
                title: "Internal Server Error.",
                detail: $"Something went wrong sorting the libraries {e}",
                instance:HttpContext.Request.Path,
                statusCode: StatusCodes.Status403Forbidden,
                type: "/docs/errors/internal-server-error");
        }

        return Ok(new StatusResponseDto<string>()
        {
            Status = "ok",
            Message = "Successfully sorted libraries.",
            Args = []
        });
    }

    [HttpPost]
    [Route("rescan")]
    public async Task<IActionResult> RescanAll()
    {
        if (!HttpContext.User.IsModerator())
            return UnauthorizedResponse("You do not have permission to rescan all libraries");
        
        await using MediaContext mediaContext = new();
        var librariesList = await mediaContext.Libraries
            .Include(library => library.FolderLibraries)
            .ThenInclude(folderLibrary => folderLibrary.Folder)
            .ToListAsync();

        if (librariesList.Count == 0)
            return NotFound(new StatusResponseDto<List<string?>>()
            {
                Status = "error",
                Message = "No libraries exist."
            });

        // const int depth = 1;

        List<string?> titles = new();

        foreach (var library in librariesList)
        {
            LibraryLogic libraryLogic = new(library.Id);
            await libraryLogic.Process();

            // List<MediaFolder> folders = new();
            // MediaScan mediaScan = new();
            //
            // string[] paths = library.FolderLibraries
            //     .Select(folderLibrary => folderLibrary.Folder.Path)
            //     .ToArray();
            //
            // foreach (var path in paths)
            // {
            //     var list = await mediaScan
            //         .Process(path, depth);
            //
            //     folders.AddRange(list);
            // }
            //
            // mediaScan.Dispose();
            //
            // foreach (var folder in folders)
            // {
            //     if (folder.Parsed is null) continue;
            //
            //     switch (library.Type)
            //     {
            //         case "movie":
            //         {
            //             SearchClient searchClient = new();
            //
            //             var paginatedMovieResponse = await searchClient.Movie(folder.Parsed.Title!, folder.Parsed.Year!);
            //
            //             if (paginatedMovieResponse?.Results.Length <= 0) continue;
            //         
            //             // List<Movie> res = Str.SortByMatchPercentage(paginatedMovieResponse.Results, m => m.Title, folder.Parsed.Title);
            //             List<Movie> res = paginatedMovieResponse?.Results.ToList() ?? [];
            //             if (res.Count is 0) continue;
            //         
            //             titles.Add(res[0].Title);
            //
            //             AddMovieJob addMovieJob = new AddMovieJob(id:res[0].Id, libraryId:library.Id.ToString());
            //             JobDispatcher.Dispatch(addMovieJob, "queue", 5);
            //             break;
            //         }
            //         case "tv":
            //         {                
            //             SearchClient searchClient = new();
            //
            //             var paginatedTvShowResponse = await searchClient.TvShow(folder.Parsed.Title!, folder.Parsed.Year!);
            //
            //             if (paginatedTvShowResponse?.Results.Length <= 0) continue;
            //         
            //             // List<TvShow> res = Str.SortByMatchPercentage(paginatedTvShowResponse.Results, m => m.Name, folder.Parsed.Title);
            //             List<TvShow> res = paginatedTvShowResponse?.Results.ToList() ?? [];
            //             if (res.Count is 0) continue;
            //
            //             titles.Add(res[0].Name);
            //
            //             AddShowJob addShowJob = new AddShowJob(id:res[0].Id, libraryId:library.Id.ToString());
            //             JobDispatcher.Dispatch(addShowJob, "queue", 5);
            //             break;
            //         }
            //         case "music":
            //         {
            //             Logger.App(folders);
            //             Logger.App("Music library rescan not implemented.");
            //             break;
            //         }
            //     }
            // }
        }

        return Ok(new StatusResponseDto<List<string?>>()
        {
            Status = "ok",
            Data = titles,
            Message = "Rescanning all libraries."
        });
    }

    [HttpPost]
    [Route("{id:ulid}/rescan")]
    public async Task<IActionResult> Rescan(Ulid id)
    {
        if (!HttpContext.User.IsModerator())
            return UnauthorizedResponse("You do not have permission to rescan the library");
        
        LibraryLogic libraryLogic = new(id);

        if (await libraryLogic.Process())
            return Ok(new StatusResponseDto<List<dynamic>>()
            {
                Status = "ok",
                Data = libraryLogic.Titles,
                Message = "Rescanning {0} library.",
                Args = [libraryLogic.Id]
            });

        return NotFound(new StatusResponseDto<List<dynamic>>()
        {
            Status = "error",
            Message = "Library {0} does not exist.",
            Args = [id]
        });
    }

    [HttpPost]
    [Route("{id:ulid}/folders")]
    public async Task<IActionResult> AddFolder(Ulid id, [FromBody] FolderRequest request)
    {
        if (!HttpContext.User.IsModerator())
            return UnauthorizedResponse("You do not have permission to add a new folder to the library");
        
        await using MediaContext mediaContext = new();
        var library = await mediaContext.Libraries.FindAsync(id);

        if (library is null)
            return NotFound(new StatusResponseDto<string>()
            {
                Status = "error",
                Message = "Library {0} does not exist.",
                Args = [id.ToString()]
            });

        try
        {
            var folder = new Folder()
            {
                Id = Ulid.NewUlid(),
                Path = request.Path
            };

            await mediaContext.Folders.Upsert(folder)
                .On(f => new { f.Path })
                .WhenMatched((fs, fi) => new Folder()
                {
                    Path = fi.Path
                })
                .RunAsync();
            
            DynamicStaticFilesMiddleware.AddPath(library.Id, folder.Path);

        }
        catch (Exception e)
        {
            return UnprocessableEntity($"Something went wrong adding a new folder {id} to {library} library: {e.Message}");
        }

        try
        {
            var folder = await mediaContext.Folders
                .Where(folder => folder.Path == request.Path)
                .FirstOrDefaultAsync();

            if (folder is null)
                return NotFound(new StatusResponseDto<string>()
                {
                    Status = "error",
                    Message = "Folder {0} does not exist.",
                    Args = [id.ToString()]
                });

            var folderLibrary = new FolderLibrary()
            {
                LibraryId = library.Id,
                FolderId = folder.Id
            };

            await mediaContext.FolderLibrary.Upsert(folderLibrary)
                .On(fl => new { fl.LibraryId, fl.FolderId })
                .WhenMatched((fls, fli) => new FolderLibrary()
                {
                    LibraryId = fli.LibraryId,
                    FolderId = fli.FolderId
                })
                .RunAsync();
        }
        catch (Exception e)
        {
            return UnprocessableEntity(new StatusResponseDto<string>()
            {
                Status = "error",
                Message = "Something went wrong adding a new folder to {0} library: {1}",
                Args = [id.ToString(), e.Message]
            });
        }

        return Ok(new StatusResponseDto<string>()
        {
            Status = "ok",
            Message = "Successfully added folder to {0} library.",
            Args = [id.ToString()]
        });
    }
    
    [HttpPatch]
    [Route("{id:ulid}/folders/{folderId:ulid}")]
    public async Task<IActionResult> UpdateFolder(Ulid id, Ulid folderId, [FromBody] FolderRequest request)
    {
        if (!HttpContext.User.IsModerator())
            return UnauthorizedResponse("You do not have permission to update the library folder");
        
        await using MediaContext mediaContext = new();
        var folder = await mediaContext.Folders
            .Where(folder => folder.Id == folderId)
            .FirstOrDefaultAsync();

        if (folder is null)
            return NotFound(new StatusResponseDto<string>()
            {
                Status = "error",
                Message = "Folder {0} does not exist.",
                Args = [id.ToString()]
            });

        try
        {
            folder.Path = request.Path;

            await mediaContext.SaveChangesAsync();
            
            DynamicStaticFilesMiddleware.RemovePath(folder.Id);
            DynamicStaticFilesMiddleware.AddPath(id, folder.Path);
        }
        catch (Exception e)
        {
            return UnprocessableEntity(new StatusResponseDto<string>()
            {
                Status = "error",
                Message = "Something went wrong updating the library folder: {0}",
                Args = [e.Message]
            });
        }

        return Ok(new StatusResponseDto<string>()
        {
            Status = "ok",
            Message = "Successfully updated folder {0}.",
            Args = [folder.Path]
        });
    }

    [HttpDelete]
    [Route("{id:ulid}/folders/{folderId:ulid}")]
    public async Task<IActionResult> DeleteFolder(Ulid id, Ulid folderId)
    {
        if (!HttpContext.User.IsModerator())
            return UnauthorizedResponse("You do not have permission to delete the library folder");
        
        await using MediaContext mediaContext = new();
        var folder = await mediaContext.Folders
            .Where(folder => folder.Id == folderId)
            .FirstOrDefaultAsync();

        if (folder is null)
            return NotFound(new StatusResponseDto<string>()
            {
                Status = "error",
                Message = "Folder {0} does not exist.",
                Args = [id.ToString()]
            });

        try
        {
            mediaContext.Folders.Remove(folder);

            await mediaContext.SaveChangesAsync();
            
            DynamicStaticFilesMiddleware.RemovePath(folder.Id);

        }
        catch (Exception e)
        {
            return UnprocessableEntity(new StatusResponseDto<string>()
            {
                Status = "error",
                Message = "Something went wrong deleting the library folder: {0}",
                Args = [e.Message]
            });
        }

        return Ok(new StatusResponseDto<string>()
        {
            Status = "ok",
            Message = "Successfully deleted folder {0}.",
            Args = [folder.Path]
        });
    }

    [HttpPost]
    [Route("{id:ulid}/folders/{folderId:ulid}/encoder_profiles")]
    public async Task<IActionResult> AddEncoderProfile(Ulid id, Ulid folderId, [FromBody] ProfilesRequest request)
    {
        if (!HttpContext.User.IsModerator())
            return UnauthorizedResponse("You do not have permission to add a new encoder profile to the folder");
        
        await using MediaContext mediaContext = new();
        var folder = await mediaContext.Folders
            .Where(folder => folder.Id == folderId)
            .FirstOrDefaultAsync();

        if (folder is null)
            return NotFound(new StatusResponseDto<string>()
            {
                Status = "error",
                Message = "Folder {0} does not exist.",
                Args = [id.ToString()]
            });

        try
        {
            var encoderProfileFolder = request.Profiles
                .Select(profile => new EncoderProfileFolder
                {
                    FolderId = folder.Id,
                    EncoderProfileId = Ulid.Parse(profile)
                })
                .ToArray();

            await mediaContext.EncoderProfileFolder.UpsertRange(encoderProfileFolder)
                .On(epf => new { epf.FolderId, epf.EncoderProfileId })
                .WhenMatched((epfs, epfi) => new EncoderProfileFolder()
                {
                    FolderId = epfi.FolderId,
                    EncoderProfileId = epfi.EncoderProfileId
                })
                .RunAsync();
        }
        catch (Exception e)
        {
            return UnprocessableEntity(new StatusResponseDto<string>()
            {
                Status = "error",
                Message = "Something went wrong adding a new encoder profile to {0} folder: {1}",
                Args = [id.ToString(), e.Message]
            });
        }

        return Ok(new StatusResponseDto<string>()
        {
            Status = "ok",
            Message = "Successfully added encoder profile to {0} folder.",
            Args = [id.ToString()]
        });
    }

    [HttpDelete]
    [Route("{id:ulid}/folders/{folderId:ulid}/encoder_profiles/{encoderProfileId:ulid}")]
    public async Task<IActionResult> DeleteEncoderProfile(Ulid id, Ulid profileId)
    {
        if (!HttpContext.User.IsModerator())
            return UnauthorizedResponse("You do not have permission to delete the encoder profile");
        
        await using MediaContext mediaContext = new();
        var encoderProfile = await mediaContext.EncoderProfiles
            .Where(folder => folder.Id == profileId)
            .FirstOrDefaultAsync();

        if (encoderProfile is null)
            return NotFound(new StatusResponseDto<string>()
            {
                Status = "error",
                Message = "Folder {0} does not exist.",
                Args = [id.ToString()]
            });

        try
        {
            mediaContext.EncoderProfiles.Remove(encoderProfile);

            await mediaContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return UnprocessableEntity(new StatusResponseDto<string>()
            {
                Status = "error",
                Message = "Something went wrong deleting the encoder profile: {0}",
                Args = [e.Message]
            });
        }

        return Ok(new StatusResponseDto<string>()
        {
            Status = "ok",
            Message = "Successfully deleted encoder profile {0}.",
            Args = [encoderProfile.Name]
        });
    }
}

public class LibraryUpdateRequest
{
    [JsonProperty("id")] public Ulid Id { get; set; }
    [JsonProperty("title")] public string Title { get; set; }
    [JsonProperty("image")] public string? Image { get; set; }
    [JsonProperty("autoRefreshInterval")] public bool PerfectSubtitleMatch { get; set; }
    [JsonProperty("realtime")] public bool Realtime { get; set; }
    [JsonProperty("specialSeasonName")] public string SpecialSeasonName { get; set; }
    [JsonProperty("type")] public string Type { get; set; }
    [JsonProperty("folder_library")] public FolderLibraryDto[] FolderLibrary { get; set; }
    [JsonProperty("subtitles")] public string[] Subtitles { get; set; }
}


public class FolderLibraryDto
{
    [JsonProperty("folder_id")] public Ulid FolderId { get; set; }
    [JsonProperty("library_id")] public Ulid LibraryId { get; set; }
    [JsonProperty("folder")] public FolderDto Folder { get; set; }
}

public class FolderDto
{
    [JsonProperty("id")] public Ulid Id { get; set; }
    [JsonProperty("path")] public string Path { get; set; }
    [JsonProperty("encoder_profiles")] public Ulid[] EncoderProfiles { get; set; }
}

public class RescanLibraryRequest
{
    [JsonProperty("id")] public bool ForceUpdate { get; set; }
    [JsonProperty("synchronous")] public bool Synchronous { get; set; }
}

public class FolderRequest
{
    [JsonProperty("path")] public string Path { get; set; }
}

public class LibrarySortRequest
{
    [JsonProperty("libraries")] public LibrarySortRequestItem[] Libraries { get; set; }
}

public class LibrarySortRequestItem
{
    [JsonProperty("id")] public Ulid Id { get; set; }
    [JsonProperty("order")] public int Order { get; set; }
}

public class ProfilesRequest
{
    [JsonProperty("profiles")] public string[] Profiles { get; set; }
}