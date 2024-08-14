using System.Net;
using System.Security.Claims;
using Microsoft.Extensions.Primitives;
using NoMercy.Helpers;
using NoMercy.Networking;
using NoMercy.Server.app.Helper;

namespace NoMercy.Server.app.Http.Middleware;

public class TokenParamAuthMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.Headers.Authorization = context.Request.Headers.Authorization.ToString().Split(",")[0];
        
        string url = context.Request.Path;

        if (!ClaimsPrincipleExtensions.FolderIds.Any(x => url.StartsWith("/" + x)) ||
            context.Request.Headers.Authorization.ToString().Contains("Bearer"))
        {
            await next(context);
            return;
        }

        var claim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(claim))
        {
            var jwt = context.Request.Query
                .FirstOrDefault(q => q.Key is "token" or "access_token").Value.ToString();

            if (string.IsNullOrEmpty(jwt))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }

            context.Request.Headers.Authorization = new StringValues("Bearer " + jwt);
        }
        else
        {
            var userId = Guid.Parse(claim);

            if (userId == Guid.Empty)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            var user = ClaimsPrincipleExtensions.Users.FirstOrDefault(x => x.Id == userId);

            if (user is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }
        }

        await next(context);
    }
}