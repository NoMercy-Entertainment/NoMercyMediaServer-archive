#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

using System.Collections.Concurrent;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.Helpers;
using NoMercy.Networking;
using NoMercy.Server.app.Helper;
using NoMercy.Server.app.Http.Middleware;

namespace NoMercy.Server.app.Http.Controllers.Socket;

public class Song
{
    [JsonProperty("color_palette")] public ColorPalette ColorPalette { get; set; }
    [JsonProperty("cover")] public string Cover { get; set; }
    [JsonProperty("date")] public DateTime Date { get; set; }
    [JsonProperty("disc")] public int Disc { get; set; }
    [JsonProperty("duration")] public string Duration { get; set; }
    [JsonProperty("favorite")] public bool Favorite { get; set; }
    [JsonProperty("filename")] public string Filename { get; set; }
    [JsonProperty("folder")] public string Folder { get; set; }
    [JsonProperty("id")] public Guid Id { get; set; }
    [JsonProperty("library_id")] public Ulid LibraryId { get; set; }
    [JsonProperty("folder_id")] public Ulid FolderId { get; set; }
    [JsonProperty("name")] public string Name { get; set; }
    [JsonProperty("origin")] public string Origin { get; set; }
    [JsonProperty("path")] public string Path { get; set; }
    [JsonProperty("quality")] public int Quality { get; set; }
    [JsonProperty("track")] public int Track { get; set; }

    [JsonProperty("type")] public string Type { get; set; }

    [JsonProperty("lyrics")] public Lyric[]? Lyrics { get; set; }
    [JsonProperty("artist_track")] public Track[] ArtistTrack { get; set; }
    [JsonProperty("album_track")] public Track[] AlbumTrack { get; set; }
    [JsonProperty("full_cover")] public string FullCover { get; set; }
}

public class Lyric
{
    [JsonProperty("text")] public string Text;
    [JsonProperty("time")] public LineTime Time;

    public class LineTime
    {
        [JsonProperty("total")] public double Total;
        [JsonProperty("minutes")] public int Minutes;
        [JsonProperty("seconds")] public int Seconds;
        [JsonProperty("hundredths")] public int Hundredths;
    }
}

public class Track
{
    [JsonProperty("color_palette")] public ColorPalette ColorPalette { get; set; }
    [JsonProperty("cover")] public string Cover { get; set; }
    [JsonProperty("description")] public string Description { get; set; }
    [JsonProperty("folder")] public string Folder { get; set; }
    [JsonProperty("id")] public Guid Id { get; set; }
    [JsonProperty("library_id")] public Ulid LibraryId { get; set; }
    [JsonProperty("name")] public string Name { get; set; }
    [JsonProperty("origin")] public string Origin { get; set; }
}

public class ColorPalette
{
    [JsonProperty("cover")] public PaletteColors Cover { get; set; }
}

public class PlayerState
{
    [JsonProperty("play_State")] public string PlayState { get; set; } = "idle";
    [JsonProperty("time")] public double Time { get; set; } = 0;
    [JsonProperty("volume")] public double Volume { get; set; } = 20;
    [JsonProperty("muted")] public bool Muted { get; set; }
    [JsonProperty("shuffle")] public bool Shuffle { get; set; }
    [JsonProperty("repeat")] public string Repeat { get; set; }
    [JsonProperty("percentage")] public int Percentage { get; set; } = 0;
    [JsonProperty("current_device")] public string? CurrentDevice { get; set; }
    [JsonProperty("current_playlist")] public string? CurrentPlaylist { get; set; }
    [JsonProperty("currentItem")] public Song? CurrentItem { get; set; }
    [JsonProperty("queue")] public IEnumerable<Song> Queue { get; set; } = [];
    [JsonProperty("backlog")] public IEnumerable<Song> Backlog { get; set; } = [];
}

public class VideoHub : ConnectionHub
{
    private static readonly ConcurrentDictionary<Guid, string> CurrentDevices = new();
    private static readonly ConcurrentDictionary<Guid, PlayerState> PlayerState = new();

    public VideoHub(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
    }

    public async Task SetTime(VideoProgressRequest request)
    {
        var userId = Guid.Parse(Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);

        var user = ClaimsPrincipleExtensions.Users.FirstOrDefault(x => x.Id == userId);

        if (user is null) return;

        var userdata = new UserData
        {
            UserId = user.Id,
            Type = request.PlaylistType,
            Time = request.Time,
            Audio = request.Audio,
            Subtitle = request.Subtitle,
            SubtitleType = request.SubtitleType,
            VideoFileId = Ulid.Parse(request.VideoId),
            MovieId = request.PlaylistType == "movie" ? request.TmdbId : null,
            TvId = request.PlaylistType == "tv" ? request.TmdbId : null,
            CollectionId = request.PlaylistType == "collection" ? request.TmdbId : null,
            SpecialId = request.SpecialId
        };

        await using MediaContext mediaContext = new();
        await mediaContext.UserData.Upsert(userdata)
            .On(x => new { x.UserId, x.VideoFileId })
            .WhenMatched((uds, udi) => new UserData
            {
                Id = uds.Id,
                Time = udi.Time,
                Audio = udi.Audio,
                Subtitle = udi.Subtitle,
                SubtitleType = udi.SubtitleType,
                LastPlayedDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ")
            })
            .RunAsync();
    }

    public async Task RemoveWatched(VideoProgressRequest request)
    {
        var userId = Guid.Parse(Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);

        var user = ClaimsPrincipleExtensions.Users.FirstOrDefault(x => x.Id == userId);

        if (user is null) return;

        await using MediaContext mediaContext = new();
        var userdata = await mediaContext.UserData
            .Where(x => x.UserId == user.Id)
            .Where(x => x.Type == request.PlaylistType)
            .Where(x => x.MovieId == request.TmdbId
                        || x.TvId == request.TmdbId
                        || x.SpecialId == request.SpecialId
                        || x.CollectionId == request.TmdbId)
            .ToArrayAsync();

        mediaContext.UserData.RemoveRange(userdata);

        await mediaContext.SaveChangesAsync();
    }

    public Task CreateSession(VideoProgressRequest request)
    {
        return Task.CompletedTask;
    }

    public Task Disconnect(VideoProgressRequest request)
    {
        return Task.CompletedTask;
    }

    public Task Connect(VideoProgressRequest request)
    {
        return Task.CompletedTask;
    }

    public Task Party(VideoProgressRequest request)
    {
        return Task.CompletedTask;
    }

    public Task JoinSession(VideoProgressRequest request)
    {
        return Task.CompletedTask;
    }

    public Task LeaveSession(VideoProgressRequest request)
    {
        return Task.CompletedTask;
    }

    public Task PartyTime(VideoProgressRequest request)
    {
        return Task.CompletedTask;
    }

    public Task PartyPlay(VideoProgressRequest request)
    {
        return Task.CompletedTask;
    }

    public Task PartyPause(VideoProgressRequest request)
    {
        return Task.CompletedTask;
    }

    public Task PartyItem(VideoProgressRequest request)
    {
        return Task.CompletedTask;
    }
    
    public void Log(dynamic? request)
    {
        Logger.Socket(request);
    }

    public class VideoProgressRequest
    {
        [JsonProperty("app")] public int AppId { get; set; }
        [JsonProperty("video_id")] public string VideoId { get; set; }
        [JsonProperty("tmdb_id")] public int TmdbId { get; set; }
        [JsonProperty("playlist_type")] public string PlaylistType { get; set; }
        [JsonProperty("video_type")] public string VideoType { get; set; }
        [JsonProperty("time")] public int Time { get; set; }
        [JsonProperty("audio")] public string Audio { get; set; }
        [JsonProperty("subtitle")] public string Subtitle { get; set; }
        [JsonProperty("subtitle_type")] public string SubtitleType { get; set; }
        [JsonProperty("special_id")] public Ulid? SpecialId { get; set; }
    }

    private PlayerState MusicPlayerState()
    {
        var user = Context.User.User();

        if (user is null)
        {        
            Logger.Socket("Creating new player state");
            return new PlayerState();
        }
        
        var playerState = PlayerState.FirstOrDefault(p => p.Key == user.Id).Value;
        
        if (playerState != null)
        {
            if (playerState.CurrentItem is not null)
            {
                playerState.CurrentItem.Lyrics = null;
            }
            return playerState;
        }

        Logger.Socket("Creating new player state");
        playerState = new PlayerState();

        PlayerState.TryAdd(user.Id, playerState);

        return playerState;
    }

    public List<Device> ConnectedDevices()
    {
        var user = Context.User.User();

        return Networking.Networking.SocketClients.Values
            .Where(x => x.Sub.Equals(user?.Id))
            .Select(c => new Device
            {
                Name = c.Name,
                Ip = c.Ip,
                DeviceId = c.DeviceId,
                Browser = c.Browser,
                Os = c.Os,
                Model = c.Model,
                Type = c.Type,
                Version = c.Version,
                Id = c.Id,
                CustomName = c.CustomName
            })
            .ToList();
    }

    public class CurrentDeviceRequest
    {
        [JsonProperty("deviceId")] public string DeviceId { get; set; }
    }

    public void SetCurrentDevice(CurrentDeviceRequest request)
    {
        var user = Context.User.User();

        var state = MusicPlayerState();
        state.CurrentDevice = request.DeviceId;

        CurrentDevices.TryAdd(user!.Id, request.DeviceId);

        Clients.All.SendAsync("CurrentDevice", request.DeviceId);

        // Logger.Socket($"Set current device to {request.DeviceId} for {user.Id.ToString()}");

        if (state.Muted) Clients.All.SendAsync("Mute", state.Muted);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);

        var connectedDevices = ConnectedDevices();
        var state = MusicPlayerState();

        // if (state.CurrentItem is null)
        // {
        //     state.Time = 0;
        //     state.Volume = 20;
        // }

        if (connectedDevices.Any(c => c.DeviceId == state.CurrentDevice) == false) await Clients.All.SendAsync("Pause");
    }

    public void CurrentDevice()
    {
        var state = MusicPlayerState();

        Clients.Caller.SendAsync("CurrentDevice", state.CurrentDevice);

        if (state.Muted) Clients.All.SendAsync("Mute", state.Muted);

        switch (state.PlayState)
        {
            case "play":
                Clients.Others.SendAsync("Play");
                break;
            case "pause":
                Clients.Others.SendAsync("Pause");
                break;
            case "stop":
                Clients.Others.SendAsync("Stop");
                break;
        }

        Clients.All.SendAsync("Volume", state.Volume);
    }

    public async Task Play()
    {
        var state = MusicPlayerState();
        state.PlayState = "play";

        // Logger.Socket(state.State);

        await Clients.Others.SendAsync("Play");
    }

    public async Task Stop()
    {
        var state = MusicPlayerState();
        state.PlayState = "stop";
        state.CurrentItem = null;
        state.Queue = [];
        state.Backlog = [];
        
        Logger.Socket(state);

        await Clients.All.SendAsync("Stop");
        await Clients.All.SendAsync("State", state);
        await Clients.All.SendAsync("Queue", state.Queue);
        await Clients.All.SendAsync("Backlog", state.Backlog);
        
    }

    public async Task Pause()
    {
        var state = MusicPlayerState();
        state.PlayState = "pause";

        // Logger.Socket(state.State);

        await Clients.Others.SendAsync("Pause");
    }

    public async Task Next()
    {
        var state = MusicPlayerState();
        // state.CurrentItem = state.Queue.FirstOrDefault(x => x.Id == state.CurrentItem?.Id + 1);

        // Logger.Socket(state.State);

        await Clients.Others.SendAsync("Next");
    }

    public async Task Previous()
    {
        var state = MusicPlayerState();
        // state.CurrentItem = state.Queue.FirstOrDefault(x => x.Id == state.CurrentItem?.Id - 1);

        // Logger.Socket(state.State);

        await Clients.Others.SendAsync("Previous");
    }

    public async Task Shuffle(bool shuffle)
    {
        var state = MusicPlayerState();
        state.Shuffle = shuffle;

        // Logger.Socket(state.Shuffle.ToString());

        await Clients.Others.SendAsync("Shuffle");
    }

    public async Task Repeat(string repeat)
    {
        var state = MusicPlayerState();
        state.Repeat = repeat;

        // Logger.Socket(state.Repeat.ToString());

        await Clients.Others.SendAsync("Repeat");
    }

    public async Task Mute(bool mute)
    {
        var state = MusicPlayerState();
        state.Muted = mute;

        // Logger.Socket(state.Muted.ToString());

        await Clients.Others.SendAsync("Mute", mute);
    }

    public async Task Volume(double volume)
    {
        var state = MusicPlayerState();
        state.Volume = volume;

        await Clients.Others.SendAsync("Volume", volume);
    }

    public async Task SeekTo(double value)
    {
        var state = MusicPlayerState();
        state.Time = value;

        // Logger.Socket(state.Percentage.ToString());

        await Clients.Others.SendAsync("SeekTo", value);
    }

    public async Task CurrentTime(double time)
    {
        var state = MusicPlayerState();
        state.Time = time;

        // Logger.Socket(state.Time.ToString(CultureInfo.InvariantCulture));

        await Clients.Others.SendAsync("CurrentTime", time);
    }

    public async Task CurrentItem(Song? currentItem)
    {
        var state = MusicPlayerState();
        state.CurrentItem = currentItem;

        await Clients.Others.SendAsync("CurrentItem", currentItem);
        await Clients.All.SendAsync("CurrentDevice", state.CurrentDevice);
    }

    public async Task Queue(Dictionary<string, Song> queue)
    {
        var state = MusicPlayerState();
        state.Queue = queue.Values;

        await Clients.Others.SendAsync("Queue", queue.Values);
    }

    public async Task Backlog(Dictionary<string, Song> backlog)
    {
        var state = MusicPlayerState();
        state.Backlog = backlog.Values;

        await Clients.Others.SendAsync("Backlog", backlog.Values);
    }

    public PlayerState State()
    {
        var state = MusicPlayerState();
        return state;
    }
}