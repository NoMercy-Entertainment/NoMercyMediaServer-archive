using Microsoft.EntityFrameworkCore;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.Helpers;
using NoMercy.NmSystem;
using NoMercy.Providers.FanArt.Client;
using NoMercy.Providers.MusicBrainz.Models;
using NoMercy.Server.Logic.ImageLogic;
using NoMercy.Server.system;
using Serilog.Events;

namespace NoMercy.Server.app.Jobs;

[Serializable]
public class FanArtImagesJob : IShouldQueue
{
    public MusicBrainzArtist? MusicBrainzArtist { get; set; }
    public MusicBrainzReleaseAppends? MusicBrainzRelease { get; set; }

    public FanArtImagesJob()
    {
        //
    }

    public FanArtImagesJob(MusicBrainzArtist musicBrainzArtist)
    {
        MusicBrainzArtist = musicBrainzArtist;
    }

    public FanArtImagesJob(MusicBrainzReleaseAppends musicBrainzRelease)
    {
        MusicBrainzRelease = musicBrainzRelease;
    }

    public async Task Handle()
    {
        
        if (MusicBrainzArtist is not null) 
            await StoreArtist(MusicBrainzArtist);

        if (MusicBrainzRelease is not null) 
            await StoreRelease(MusicBrainzRelease);
    }

    public async Task StoreArtist(MusicBrainzArtist musicBrainzArtist)
    {
        try
        {
            using FanArtMusicClient fanArtMusicClient = new();
            var fanArt = await fanArtMusicClient.Artist(musicBrainzArtist.Id);
            if (fanArt is null) return;

            var thumbs = fanArt.Thumbs.ToList()
                .ConvertAll<Image>(image => new Image{
                        AspectRatio = 1,
                        Type = "thumb",
                        VoteCount = image.Likes,
                        FilePath = "/" + image.Url.FileName(),
                        ArtistId =  musicBrainzArtist.Id,
                        Site = image.Url.BasePath(),
                         _colorPalette = FanArtImage.ColorPalette("image", image.Url).Result
                });

            var logos = fanArt.Logos.ToList()
                .ConvertAll<Image>(image => new Image{
                    AspectRatio = 1,
                    Type = "logo",
                    VoteCount = image.Likes,
                    FilePath = "/" + image.Url.FileName(),
                    ArtistId =  musicBrainzArtist.Id,
                    Site = image.Url.BasePath(),
                    _colorPalette = FanArtImage.ColorPalette("image", image.Url).Result
                });
            var banners = fanArt.Banners.ToList()
                .ConvertAll<Image>(image => new Image{
                    AspectRatio = 1,
                    Type = "banner",
                    VoteCount = image.Likes,
                    FilePath = "/" + image.Url.FileName(),
                    ArtistId =  musicBrainzArtist.Id,
                    Site = image.Url.BasePath(),
                    _colorPalette = FanArtImage.ColorPalette("image", image.Url).Result
                });
            var hdLogos = fanArt.HdLogos.ToList()
                .ConvertAll<Image>(image => new Image{
                    AspectRatio = 1,
                    Type = "hdLogo",
                    VoteCount = image.Likes,
                    FilePath = "/" + image.Url.FileName(),
                    ArtistId =  musicBrainzArtist.Id,
                    Site = image.Url.BasePath(),
                    _colorPalette = FanArtImage.ColorPalette("image", image.Url).Result
                });
            var artistBackgrounds = fanArt.Backgrounds.ToList()
                .ConvertAll<Image>(image => new Image{
                    AspectRatio = 1,
                    Type = "background",
                    VoteCount = image.Likes,
                    FilePath = "/" + image.Url.FileName(),
                    ArtistId =  musicBrainzArtist.Id,
                    Site = image.Url.BasePath(),
                    _colorPalette = FanArtImage.ColorPalette("image", image.Url).Result
                });

            var images = thumbs
                .Concat(logos)
                .Concat(banners)
                .Concat(hdLogos)
                .Concat(artistBackgrounds)
                .ToList();

            await using MediaContext mediaContext = new();
            var dbArtist = await mediaContext.Artists
                .FirstAsync(a => a.Id == musicBrainzArtist.Id);

            var artistCover = thumbs.FirstOrDefault();
            dbArtist.Cover = artistCover?.FilePath ?? dbArtist.Cover;
            
            dbArtist._colorPalette = artistCover?._colorPalette.Replace("\"image\"", "\"cover\"") 
                                     ?? dbArtist._colorPalette;
            
            await mediaContext.SaveChangesAsync();
            
            await mediaContext.Images.UpsertRange(images)
                .On(v => new { v.FilePath, v.ArtistId })
                .WhenMatched((s, i) => new Image
                {
                    UpdatedAt = DateTime.UtcNow,
                    Id = i.Id,
                    AspectRatio = i.AspectRatio,
                    Height = i.Height,
                    FilePath = i.FilePath,
                    Width = i.Width,
                    VoteCount = i.VoteCount,
                    ArtistId = i.ArtistId,
                    Type = i.Type,
                    Site = i.Site,
                    _colorPalette = i._colorPalette,
                })
                .RunAsync();
        }
        catch (Exception e)
        {
            if(e.Message.Contains("404")) return;
            Logger.FanArt(e.Message, LogEventLevel.Verbose);
        }
    }

    public async Task StoreRelease(MusicBrainzReleaseAppends musicBrainzRelease)
    {
        try
        {
            using FanArtMusicClient fanArtMusicClient = new();
            var fanArt = await fanArtMusicClient.Album(musicBrainzRelease.MusicBrainzReleaseGroup.Id);
            if (fanArt is null) return;

            List<Image> covers = [];
            List<Image> cdArts = [];
            foreach (var (_, albums) in fanArt.Albums)
            {
                covers.AddRange(albums.Cover
                    .Select(image => new Image{
                        AspectRatio = 1,
                        Type = "cover",
                        VoteCount = image.Likes,
                        FilePath = "/" + image.Url.FileName(),
                        ArtistId =  musicBrainzRelease.Id,
                        Site = image.Url.BasePath(),
                        Name = fanArt.Name,
                        _colorPalette = FanArtImage.ColorPalette("image", image.Url).Result
                    }));

                cdArts.AddRange(albums.CdArt
                    .Select(image => new Image{
                        AspectRatio = 1,
                        Type = "cdArt",
                        VoteCount = image.Likes,
                        FilePath = "/" + image.Url.FileName(),
                        ArtistId =  musicBrainzRelease.Id,
                        Site = image.Url.BasePath(),
                        Name = fanArt.Name,
                        _colorPalette = FanArtImage.ColorPalette("image", image.Url).Result
                    }));
            }
            
            await using MediaContext mediaContext = new();
            var dbRelease = await mediaContext.ReleaseGroups
                .Include(a => a.AlbumReleaseGroup)
                    .ThenInclude(a => a.Album)
                .FirstAsync(a => a.Id == musicBrainzRelease.MusicBrainzReleaseGroup.Id);
            
            var images = covers
                .Concat(cdArts)
                .Where(image => dbRelease.AlbumReleaseGroup
                    .Any(ar => ar.AlbumId == image.AlbumId));
            
            var albumCover = covers.FirstOrDefault();
            
            dbRelease.Cover = albumCover?.FilePath ?? dbRelease.Cover;
            dbRelease._colorPalette = albumCover?._colorPalette.Replace("\"image\"", "\"cover\"") 
                                      ?? dbRelease._colorPalette;
            
            foreach (var albumRelease in dbRelease.AlbumReleaseGroup)
            {
                albumRelease.Album.Cover = albumCover?.FilePath ?? albumRelease.Album.Cover;
                albumRelease.Album._colorPalette = albumCover?._colorPalette.Replace("\"image\"", "\"cover\"")
                                                   ?? albumRelease.Album._colorPalette;
            }
            
            await mediaContext.SaveChangesAsync();
            
            await mediaContext.Images.UpsertRange(images)
                .On(v => new { v.FilePath, v.AlbumId })
                .WhenMatched((s, i) => new Image
                {
                    UpdatedAt = DateTime.UtcNow,
                    Id = i.Id,
                    AspectRatio = i.AspectRatio,
                    Name = i.Name,
                    Height = i.Height,
                    FilePath = i.FilePath,
                    Width = i.Width,
                    VoteCount = i.VoteCount,
                    AlbumId = i.AlbumId,
                    Type = i.Type,
                    Site = i.Site,
                    _colorPalette = i._colorPalette,
                })
                .RunAsync();
        }
        catch (Exception e)
        {
            if(e.Message.Contains("404")) return;
            Logger.FanArt(e.Message, LogEventLevel.Verbose);
        }
    }

}
