using NoMercy.Helpers;
using NoMercy.Helpers.Monitoring;
using NoMercy.Networking;
using NoMercy.Server.app.Helper;
using Serilog.Events;

namespace NoMercy.Server.app.Http.Controllers.Socket;

public class DashboardHub : ConnectionHub
{
    public DashboardHub(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
    }
    
    public void StartResources()
    {
        ResourceMonitor.StartBroadcasting();
    }

    public void StopResources()
    {
        ResourceMonitor.StopBroadcasting();
    }
}

public static class ResourceMonitor
{
    private static bool _broadcasting = false;
    private static readonly CancellationTokenSource CancellationTokenSource = new();

    public static void StartBroadcasting()
    {
        Logger.Socket("Starting resource monitoring broadcast");
        _broadcasting = true;
        Task.Run(() => BroadcastLoop(CancellationTokenSource.Token));
    }

    public static void StopBroadcasting()
    {
        Logger.Socket("Stopping resource monitoring broadcast");
        _broadcasting = false;
        CancellationTokenSource.Cancel();
    }

    private static void BroadcastLoop(CancellationToken cancellationToken)
    {
        while (true)
        {
            if (_broadcasting && !cancellationToken.IsCancellationRequested) break;
            try
            {
                Resource? resourceData = Helpers.Monitoring.ResourceMonitor.Monitor();
                Networking.Networking.SendToAll("ResourceUpdate", "dashboardHub", resourceData);
            }
            catch (Exception e)
            {
                Logger.Socket($"Error broadcasting resource data: {e.Message}", LogEventLevel.Error);
            }
        }
    }

}