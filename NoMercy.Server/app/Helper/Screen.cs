using System.Runtime.InteropServices;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace NoMercy.Server.app.Helper;

public static class Screen
{
    public static int ScreenWidth()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ScreenWidthWindows() : 1666;
    }

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    private static int ScreenWidthWindows(int screenIndex = 0)
    {
        return GetSystemMetrics(screenIndex);
    }
    
    public static bool IsDesktopEnvironment()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return true;
        }

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return false;
        
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WSL_DISTRO_NAME")))
        {
            return false;
        }
        
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY"));

    }
}