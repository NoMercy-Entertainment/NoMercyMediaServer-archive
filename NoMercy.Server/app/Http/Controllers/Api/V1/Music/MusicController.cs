using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoMercy.Database;
using NoMercy.Helpers;
using NoMercy.Networking;
using NoMercy.Server.app.Helper;
using NoMercy.Server.app.Http.Controllers.Api.V1.Media.DTO;
using NoMercy.Server.app.Http.Middleware;

namespace NoMercy.Server.app.Http.Controllers.Api.V1.Music;

[ApiController]
[ApiVersion("1")]
[Tags("Music")]
[Authorize]
[Route("api/v{Version:apiVersion}/music")]
public class MusicController: BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = HttpContext.User.UserId();
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view music");

        List<GenreRowDto<dynamic>> list = [];

        MediaContext mediaContext = new();

        var favoriteArtist = CarouselResponseItemDto.GetFavoriteArtist(mediaContext, userId);
        var favoriteAlbum = CarouselResponseItemDto.GetFavoriteAlbum(mediaContext, userId);
        var favoritePlaylist = CarouselResponseItemDto.GetFavoritePlaylist(mediaContext, userId);
        
        var favoriteArtists = await CarouselResponseItemDto.GetFavoriteArtists(mediaContext, userId);
        var favoriteAlbums = await CarouselResponseItemDto.GetFavoriteAlbums(mediaContext, userId);
        
        var playlists = await CarouselResponseItemDto.GetPlaylists(mediaContext, userId);
        var latestArtists = await CarouselResponseItemDto.GetLatestArtists(mediaContext, userId);
        var latestAlbums = await CarouselResponseItemDto.GetLatestAlbums(mediaContext, userId);


        list.Add(new GenreRowDto<dynamic>
        {
            Title = null,
            MoreLink = "",
            Items =
            [
                favoriteArtist,
                favoriteAlbum,
                favoritePlaylist,
            ]
        });
        
        list.Add(new GenreRowDto<dynamic>
        {
            Title = "Favorite Artists",
            MoreLink = "",
            Items = favoriteArtists
        });
        
        list.Add(new GenreRowDto<dynamic>
        {
            Title = "Favorite Albums",
            MoreLink = "",
            Items = favoriteAlbums
        });
        
        list.Add(new GenreRowDto<dynamic>
        {
            Title = "Playlists",
            MoreLink = "app.music.playlists",
            Items = playlists,
        });

        list.Add(new GenreRowDto<dynamic>
        {
            Title = "Artists",
            MoreLink = "app.music.artists",
            Items = latestArtists
        });

        list.Add(new GenreRowDto<dynamic>
        {
            Title = "Albums",
            MoreLink = "app.music.albums",
            Items = latestAlbums!
        });

        return Ok(new HomeResponseDto<dynamic>
        {
            Data = list
        });
    }

    [HttpPost]
    [Route("search")]
    public IActionResult Search()
    {
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to search music");

        return Ok(new PlaceholderResponse
        {
            Data = []
        });
    }

    [HttpPost]
    [Route("search/{query}/{Type}")]
    public IActionResult TypeSearch(string query, string type)
    {
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to search music");

        return Ok(new PlaceholderResponse
        {
            Data = []
        });
    }

    [HttpPost]
    [Route("coverimage")]
    public IActionResult CoverImage()
    {
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view cover images");

        return Ok(new PlaceholderResponse
        {
            Data = []
        });
    }

    [HttpPost]
    [Route("images")]
    public IActionResult Images()
    {
        if (!HttpContext.User.IsAllowed())
            return UnauthorizedResponse("You do not have permission to view images");

        return Ok(new PlaceholderResponse
        {
            Data = []
        });
    }
}