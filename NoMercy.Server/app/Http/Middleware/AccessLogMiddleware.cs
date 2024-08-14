using System.Security.Claims;
using System.Web;
using NoMercy.Helpers;
using NoMercy.Networking;
using NoMercy.Server.app.Helper;

namespace NoMercy.Server.app.Http.Middleware;

public class AccessLogMiddleware
{
    private readonly RequestDelegate _next;

    private readonly string[] _ignoredStartsWithRoutes =
    [
        "/images"
    ];

    private readonly string[] _ignoreExact =
    [
        "/",
        "/swagger",
        "/swagger-ui",
        "/swagger-ui/favicon-16x16.png",
        "/swagger-ui/favicon-32x32.png",
        "/swagger-ui/favicon.ico",
        "/swagger-ui/index.html",
        "/swagger-ui/swagger-ui-bundle.js",
        "/swagger-ui/swagger-ui-standalone-preset.js",
        "/swagger-ui/swagger-ui.css",
        "/swagger/v1/swagger.json",
        "/api/v1/dashboard/logs"
    ];

    private readonly string[] _ignoreIfAuthenticated =
    [
        "/socket"
    ];

    private readonly string[] _ignoreIfGuest =
    [
        "/status"
    ];

    public AccessLogMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = HttpUtility.UrlDecode(context.Request.Path);

        var ignoreStart = _ignoredStartsWithRoutes
            .Any(route => context.Request.Path.ToString().StartsWith(route));

        var ignoreExactRoute = _ignoreExact
            .Any(route => context.Request.Path.ToString().Equals(route));

        if (ignoreStart || ignoreExactRoute)
        {
            await _next(context);
            return;
        }

        var ignoreIfGuest = _ignoreIfGuest
            .Any(route => context.Request.Path.ToString().Equals(route));

        var guid = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (guid is null)
        {
            if (ignoreIfGuest)
            {
                await _next(context);
                return;
            }

            Logger.Http($"Unknown: {context.Connection.RemoteIpAddress}: {path}");
            await _next(context);
            return;
        }


        var userId = Guid.Parse(guid);
        if (userId == Guid.Empty)
        {
            if (ignoreIfGuest)
            {
                await _next(context);
                return;
            }

            Logger.Http($"Unknown: {context.Connection.RemoteIpAddress}: {path}");
            await _next(context);
            return;
        }

        var ignoreIfAuthenticated = _ignoreIfAuthenticated
            .Any(route => context.Request.Path.ToString().Equals(route));

        if (ignoreIfAuthenticated)
        {
            await _next(context);
            return;
        }

        if (ClaimsPrincipleExtensions.FolderIds.Any(x => path.StartsWith("/" + x)))
        {
            await _next(context);
            return;
        }

        var user = ClaimsPrincipleExtensions.Users.FirstOrDefault(x => x.Id == userId);

        Logger.Http($"{user?.Name ?? $"Unknown: {context.Connection.RemoteIpAddress}:"}: {path}");

        await _next(context);
    }
}