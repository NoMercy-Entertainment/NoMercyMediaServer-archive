using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using H.NotifyIcon.Core;

namespace NoMercy.Server;

public class TrayIcon
{
#pragma warning disable CA1416
    private static Icon LoadIcon()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string resourceName = "NoMercy.Server.Assets.icon.ico";

        using Stream stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException("Icon resource not found.");
        }
        return new Icon(stream);
    }
    
    private static readonly Icon Icon = LoadIcon();

    private readonly TrayIconWithContextMenu _trayIcon = new()
    {
        Icon = Icon.Handle,
        ToolTip = "NoMercy MediaServer C#"
    };

    [SupportedOSPlatform("windows10.0.18362")]
    private TrayIcon()
    {
        _trayIcon.ContextMenu = new PopupMenu
        {
            Items =
            {
                new PopupMenuItem("Show App", (_, _) => Show()),
                new PopupMenuSeparator(),
                new PopupMenuItem("Pause Server", (_, _) => Pause()),
                new PopupMenuItem("Restart Server", (_, _) => Restart()),
                new PopupMenuItem("Shutdown", (_, _) => Shutdown())
            }
        };

        _trayIcon.Create();
    }

    private static void Pause()
    {
    }

    private static void Show()
    {
    }

    private static void Restart()
    {
    }

    private void Shutdown()
    {
        _trayIcon.Dispose();
        Environment.Exit(0);
    }

    public static Task Make()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
            OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362))
        {
            TrayIcon _ = new();
        }

        return Task.CompletedTask;
    }
    
#pragma warning disable CA1416
}