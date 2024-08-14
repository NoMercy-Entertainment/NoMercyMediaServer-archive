using Microsoft.EntityFrameworkCore;
using NoMercy.Database;
using NoMercy.Server.system;
using NoMercy.Providers.CoverArt.Client;
using NoMercy.Server.Logic.ImageLogic;
using NoMercy.Database.Models;
using NoMercy.Helpers;
using NoMercy.NmSystem;
using NoMercy.Providers.MusicBrainz.Models;
using Serilog.Events;

namespace NoMercy.Server.app.Jobs;

[Serializable]
public class CoverArtImageJob : IShouldQueue
{
    public MusicBrainzReleaseAppends? MusicBrainzRelease { get; set; }

    public CoverArtImageJob()
    {
        //
    }
    
    public CoverArtImageJob(MusicBrainzReleaseAppends musicBrainzRelease)
    {
        MusicBrainzRelease = musicBrainzRelease;
    }

    public async Task Handle()
    {
        try
        {
            if (MusicBrainzRelease is null) return;
            
            var coverPalette = await FetchCover(MusicBrainzRelease);
            if (coverPalette is null) return;
            
            await using MediaContext mediaContext = new();
            var album = await mediaContext.Albums
                .Include(a => a.AlbumTrack)
                .ThenInclude(a => a.Track)
                .FirstOrDefaultAsync(a => a.Id == MusicBrainzRelease.Id);
            if (album is null) return;
            
            album._colorPalette = coverPalette.Palette ?? album._colorPalette;
            album.Cover = coverPalette.Url is not null 
                ? "/" + coverPalette.Url.FileName() 
                : album.Cover;
            album.UpdatedAt = DateTime.Now;
            
            await mediaContext.SaveChangesAsync();

            foreach (var albumTrack in album.AlbumTrack)
            {
                albumTrack.Track._colorPalette = coverPalette.Palette ?? albumTrack.Track._colorPalette;
                albumTrack.Track.Cover = coverPalette.Url is not null 
                    ? "/" + coverPalette.Url.FileName() 
                    : albumTrack.Track.Cover;
                albumTrack.Track.UpdatedAt = DateTime.Now;
                
                await mediaContext.SaveChangesAsync();
            }
            
        }
        catch (Exception e)
        {
            if(e.Message.Contains("404")) return;
            Logger.CoverArt(e.Message, LogEventLevel.Verbose);
        }
    }
    
    private class CoverPalette : Image
    {
        public string? Palette { get; set; }
        public Uri? Url { get; set; }
    }
    
    private static async Task<CoverPalette?> FetchCover(MusicBrainzReleaseAppends musicBrainzReleaseAppends)
    {
        var hasCover = musicBrainzReleaseAppends.CoverArtArchive.Front;
        if (!hasCover) return null;
        
        CoverArtCoverArtClient coverArtCoverArtClient = new(musicBrainzReleaseAppends.Id);
        var covers = await coverArtCoverArtClient.Cover();
        if (covers is null) return null;

        var coverList = covers.Images
            .Where(image => image.Types.Contains("Front"))
            .ToList();

        foreach (var coverItem in coverList)
        {
            if(!coverItem.CoverArtThumbnails.Large.HasSuccessStatus("image/*")) continue;
            
            return new CoverPalette
            {
                Palette = await CoverArtImage.ColorPalette("cover", coverItem.CoverArtThumbnails.Large),
                Url = coverItem.CoverArtThumbnails.Large
            };
        }
        
        return null;
    }
}