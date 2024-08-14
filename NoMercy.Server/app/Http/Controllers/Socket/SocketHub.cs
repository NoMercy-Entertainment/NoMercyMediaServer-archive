using NoMercy.Helpers;
using NoMercy.Networking;
using NoMercy.Server.app.Helper;

namespace NoMercy.Server.app.Http.Controllers.Socket;

public class SocketHub : ConnectionHub
{
    public SocketHub(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
    }
}