using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;
using NoMercy.Api.Controllers.Socket;

namespace NoMercy.Api.Middleware;

public class WebRtcFallbackMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHubContext<WebRtcHub> _hubContext;

    public WebRtcFallbackMiddleware(RequestDelegate next, IHubContext<WebRtcHub> hubContext)
    {
        _next = next;
        _hubContext = hubContext;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("X-WebRTC-Fallback", out StringValues roomId))
        {
            await _next(context);
            return;
        }

        var request = new
        {
            Endpoint = context.Request.Path.Value,
            Method = context.Request.Method,
            Data = await new StreamReader(context.Request.Body).ReadToEndAsync()
        };

        await _hubContext.Clients.Group(roomId.ToString())
            .SendAsync("ReceiveApiRequest", request.Endpoint, request.Method, request.Data);

        // Wait for response through WebRTC
        // Implementation depends on your specific needs
    }
}