using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace NoMercy.Api.Controllers.Socket;

public class WebRtcHub : Hub
{
    private readonly ILogger<WebRtcHub> _logger;

    public WebRtcHub(ILogger<WebRtcHub> logger)
    {
        _logger = logger;
    }

    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        _logger.LogInformation($"Client {Context.ConnectionId} joined room {roomId}");
    }

    public async Task SendSignal(string roomId, string type, string payload)
    {
        await Clients.OthersInGroup(roomId).SendAsync("ReceiveSignal", type, payload);
    }

    public async Task SendApiRequest(string roomId, string endpoint, string method, string data)
    {
        await Clients.OthersInGroup(roomId).SendAsync("ReceiveApiRequest", endpoint, method, data);
    }

    public async Task SendApiResponse(string roomId, string response)
    {
        await Clients.OthersInGroup(roomId).SendAsync("ReceiveApiResponse", response);
    }
}