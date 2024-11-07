using Microsoft.EntityFrameworkCore;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.NmSystem;

namespace NoMercy.MediaProcessing.Files;

public class FileRepository(MediaContext context) : IFileRepository
{
    public Task StoreVideoFile(VideoFile videoFile)
    {
        return context.VideoFiles.Upsert(videoFile)
            .On(vf => vf.Filename)
            .WhenMatched((vs, vi) => new VideoFile
            {
                EpisodeId = vi.EpisodeId,
                MovieId = vi.MovieId,
                Folder = vi.Folder,
                HostFolder = vi.HostFolder,
                Filename = vi.Filename,
                Share = vi.Share,
                Duration = vi.Duration,
                Chapters = vi.Chapters,
                Languages = vi.Languages,
                Quality = vi.Quality,
                Subtitles = vi.Subtitles,
                _tracks = vi._tracks,
                UpdatedAt = vi.UpdatedAt
            })
            .RunAsync();
    }

    public async Task<Episode?> GetEpisode(int? showId, MediaFile item)
    {
        if (item.Parsed == null) return null;

        return await context.Episodes
            .Where(e => e.TvId == showId)
            .Where(e => e.SeasonNumber == item.Parsed!.Season)
            .Where(e => e.EpisodeNumber == item.Parsed!.Episode)
            .FirstOrDefaultAsync();
    }

    public async Task<(Movie? movie, Tv? show, string type)> MediaType(int id, Library library)
    {
        Movie? movie = null;
        Tv? show = null;
        string type = "";

        switch (library.Type)
        {
            case "movie":
                movie = await context.Movies
                    .Where(m => m.Id == id)
                    .FirstOrDefaultAsync();
                type = "movie";
                break;
            case "tv":
                show = await context.Tvs
                    .Where(t => t.Id == id)
                    .FirstOrDefaultAsync();
                type = "tv";
                break;
            case "anime":
                show = await context.Tvs
                    .Where(t => t.Id == id)
                    .FirstOrDefaultAsync();
                type = "anime";
                break;
        }

        return (movie, show, type);
    }

    public async Task SetCreatedAt(VideoFile videoFile)
    {
        string path = videoFile.HostFolder.Substring(0, videoFile.HostFolder.LastIndexOf('/'));
        DateTime createdDateTime = Directory.GetCreationTime(path);

        if (videoFile.CreatedAt == createdDateTime) return;
        
        if (videoFile.EpisodeId is not null)
        {
            Tv? tv = await context.Tvs.FindAsync(videoFile.EpisodeId);
            if (tv is null) return;

            tv.CreatedAt = createdDateTime;
            await context.SaveChangesAsync();
        }
        else if (videoFile.MovieId is not null)
        {
            Movie? movie = await context.Movies.FindAsync(videoFile.MovieId);
            if (movie is null) return;

            movie.CreatedAt = createdDateTime;
            await context.SaveChangesAsync();
        }

    }
}