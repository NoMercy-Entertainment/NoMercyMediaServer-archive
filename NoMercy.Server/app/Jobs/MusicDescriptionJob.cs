using Microsoft.EntityFrameworkCore;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.Helpers;
using NoMercy.NmSystem;
using NoMercy.Providers.MusicBrainz.Models;
using NoMercy.Providers.TADB.Client;
using NoMercy.Server.system;
using Serilog.Events;

namespace NoMercy.Server.app.Jobs;

[Serializable]
public class MusicDescriptionJob : IShouldQueue
{
    public MusicBrainzArtist? MusicBrainzArtist { get; set; }
    public MusicBrainzReleaseGroup? MusicBrainzReleaseGroup { get; set; }

    public MusicDescriptionJob()
    {
        //
    }

    public MusicDescriptionJob(MusicBrainzArtist musicBrainzArtist)
    {
        MusicBrainzArtist = musicBrainzArtist;
    }
    
    public MusicDescriptionJob(MusicBrainzReleaseGroup? musicBrainzReleaseGroup)
    {
        MusicBrainzReleaseGroup = musicBrainzReleaseGroup;
    }

    public async Task Handle()
    {
        if (MusicBrainzArtist != null)
        {
            await HandleArtist();
        }
        else if (MusicBrainzReleaseGroup != null)
        {
            await HandleReleaseGroup();
        }

    }
    
    private async Task HandleArtist()
    {
        if (MusicBrainzArtist == null) return;
        
        try
        {
            var artistClient = new TadbArtistClient();
            var result = artistClient.ByMusicBrainzId(MusicBrainzArtist.Id);
            if (result?.Descriptions is null) return;
            
            await using MediaContext context = new();
            var artist = await context.Artists.FindAsync(MusicBrainzArtist.Id);
            if (artist == null) return;
            
            artist.Description = result.Descriptions
                .Where(x => x.Iso31661 == "EN")
                .Select(x => x.Description)
                .FirstOrDefault();
            artist.Year = result.IntFormedYear?.ToInt();
            await context.SaveChangesAsync();
            
            var translations = result.Descriptions.
                ConvertAll(x => new Translation
                {
                    ArtistId = MusicBrainzArtist.Id,
                    Iso31661 = x.Iso31661,
                    Description = x.Description
                });
            
            await context.Translations.UpsertRange(translations)
                .On(x => new {x.ArtistId, x.Iso31661})
                .WhenMatched((s, i) => new Translation
                {
                    ArtistId = s.ArtistId,
                    Iso31661 = s.Iso31661,
                    Description = s.Description
                })
                .RunAsync();
        
        }
        catch (Exception e)
        {
            if(e.Message.Contains("404")) return;
            Logger.AudioDb(e.Message, LogEventLevel.Verbose);
        }
    }
    
    private async Task HandleReleaseGroup()
    {
        if (MusicBrainzReleaseGroup == null) return;
        
        try
        {
            var releaseClient = new TadbReleaseGroupClient();
            var result = releaseClient.ByMusicBrainzId(MusicBrainzReleaseGroup.Id);
            if (result?.Descriptions is null) return;
            
            await using MediaContext context = new();
            var releaseGroup = await context.ReleaseGroups.FindAsync(MusicBrainzReleaseGroup.Id);
            if (releaseGroup == null) return;
            
            var description = result.Descriptions
                .Where(x => x.Iso31661 == "EN")
                .Select(x => x.Description)
                .FirstOrDefault();
            
            var hasUpdatedDescription = !string.IsNullOrEmpty(description) && releaseGroup.Description != description;

            releaseGroup.Description = description;
            if(hasUpdatedDescription)
                await context.SaveChangesAsync();
            
            var translations = result.Descriptions.
                ConvertAll(x => new Translation
                {
                    ReleaseGroupId = MusicBrainzReleaseGroup.Id,
                    Iso31661 = x.Iso31661,
                    Description = x.Description
                });
            
            await context.Translations.UpsertRange(translations)
                .On(x => new {x.ReleaseGroupId, x.Iso31661})
                .WhenMatched((s, i) => new Translation
                {
                    ReleaseGroupId = s.ReleaseGroupId,
                    Iso31661 = s.Iso31661,
                    Description = s.Description
                })
                .RunAsync();
        }
        catch (Exception e)
        {
            if(e.Message.Contains("404")) return;
            Logger.AudioDb(e.Message, LogEventLevel.Verbose);
        }
    }
}