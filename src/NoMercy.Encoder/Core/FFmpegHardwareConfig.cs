using System.Runtime.InteropServices;
using NoMercy.NmSystem.Information;
using NoMercy.NmSystem.SystemCalls;
using Serilog.Events;

namespace NoMercy.Encoder.Core;

public enum GpuVendor
{
    Nvidia,
    Amd,
    Intel,
    Qualcomm,
    Apple,
    Unknown
}

public class FFmpegHardwareConfig
{
    public List<GpuAccelerator> Accelerators { get; private set; } = new();

    public FFmpegHardwareConfig()
    {
        SetHardwareAccelerationFlags(Info.GpuVendors);
    }

    public bool HasAccelerator(string accelerator)
    {
        return Accelerators.Any(a => a.Accelerator == accelerator);
    }

    private static bool CheckAccel(string arg)
    {
        try
        {
            Logger.Encoder($"Checking Acceleration: -hide_banner {arg} -hwaccels 2>&1", LogEventLevel.Debug);
            // var awaiter = FfMpeg.ExecStdOut($"-hide_banner {arg} -hwaccels 2>&1").GetAwaiter();
            // string result = awaiter.GetResult();
            string result = Shell.ExecStdOutSync(AppFiles.FfmpegPath, $"-hide_banner {arg} -hwaccels 2>&1");

            return !result.Contains("Failed", StringComparison.InvariantCultureIgnoreCase) &&
                   !result.Contains("exception", StringComparison.InvariantCultureIgnoreCase);
        }
        catch (Exception e)
        {
            Logger.Encoder(e, LogEventLevel.Debug);
            return false;
        }
    }

    private void OpenClCheck()
    {
        string arg = "-extra_hw_frames 3 -init_hw_device opencl=ocl";
        bool supported = CheckAccel(arg);
        if (supported)
        {
            Accelerators.Add(new(
                vendor: GpuVendor.Unknown,
                ffmpegArgs: arg,
                accelerator: "none"
            ));
        }
    }

    private void SetHardwareAccelerationFlags(List<string> gpuVendors)
    {
        Dictionary<GpuVendor, int> gpuCounts = new()
        {
            { GpuVendor.Nvidia, 0 },
            { GpuVendor.Amd, 0 },
            { GpuVendor.Intel, 0 },
            { GpuVendor.Qualcomm, 0 },
            { GpuVendor.Apple, 0 }
        };

        foreach (string vendor in gpuVendors.Select(v => v.ToLower()))
        {
            if (vendor.Contains("nvidia"))
            {
                int index = gpuCounts[GpuVendor.Nvidia];
                string arg =
                    $"-init_hw_device cuda=cu:{index} -filter_hw_device cu";
                bool supported = CheckAccel(arg);
                if (supported)
                {
                    Accelerators.Add(new(
                        vendor: GpuVendor.Nvidia,
                        ffmpegArgs: arg,
                        accelerator: "cuda"
                    ));
                }
                else
                {
                    OpenClCheck();
                }

                gpuCounts[GpuVendor.Nvidia]++;
            }
            else if (vendor.Contains("amd") || vendor.Contains("advanced micro devices"))
            {
                int index = gpuCounts[GpuVendor.Amd];
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    string arg =
                        $"-init_hw_device dxva2=hw{index} -filter_hw_device hw";
                    bool supported = CheckAccel(arg);
                    if (supported)
                    {
                        Accelerators.Add(new(
                            vendor: GpuVendor.Amd,
                            ffmpegArgs: arg,
                            accelerator: "dxva2"
                        ));
                    }
                    else
                    {
                        OpenClCheck();
                    }
                }
                else
                {
                    string arg =
                        $"-init_hw_device vaapi=hw{index}:/dev/dri/renderD128 -filter_hw_device hw";
                    bool supported = CheckAccel(arg);
                    if (supported)
                    {
                        Accelerators.Add(new(
                            vendor: GpuVendor.Amd,
                            ffmpegArgs: arg,
                            filter: "hwupload",
                            accelerator: "vaapi"
                        ));
                    }
                    else
                    {
                        OpenClCheck();
                    }
                }

                gpuCounts[GpuVendor.Amd]++;
            }
            else if (vendor.Contains("intel"))
            {
                int index = gpuCounts[GpuVendor.Intel];
                string arg =
                    $"-init_hw_device qsv=hw{index} -filter_hw_device hw";
                bool supported = CheckAccel(arg);
                if (supported)
                {
                    Accelerators.Add(new(
                        vendor: GpuVendor.Intel,
                        ffmpegArgs: arg,
                        accelerator: "qsv"
                    ));
                }
                else
                {
                    OpenClCheck();
                }

                gpuCounts[GpuVendor.Intel]++;
            }
            else if (vendor.Contains("qualcomm"))
            {
                int index = gpuCounts[GpuVendor.Qualcomm];
                string arg = $"-init_hw_device opencl=hw{index} -filter_hw_device hw";
                bool supported = CheckAccel(arg);
                if (supported)
                {
                    Accelerators.Add(new(
                        vendor: GpuVendor.Qualcomm,
                        ffmpegArgs: arg,
                        accelerator: "opencl"
                    ));
                }
                else
                {
                    OpenClCheck();
                }

                gpuCounts[GpuVendor.Qualcomm]++;
            }
            else if (vendor.Contains("apple"))
            {
                int index = gpuCounts[GpuVendor.Apple];
                string arg = $"-init_hw_device videotoolbox:hw{index} -filter_hw_device hw";
                bool supported = CheckAccel(arg);
                if (supported)
                {
                    Accelerators.Add(new(
                        vendor: GpuVendor.Apple,
                        ffmpegArgs: arg,
                        accelerator: "videotoolbox"
                    ));
                }
                else
                {
                    OpenClCheck();
                }

                gpuCounts[GpuVendor.Apple]++;
            }
            else
            {
                OpenClCheck();
            }
        }
    }
}

public class GpuAccelerator
{
    public GpuVendor Vendor { get; }
    public string FfmpegArgs { get; }
    public string Filter { get; }
    public string Accelerator { get; }

    public GpuAccelerator(GpuVendor vendor, string ffmpegArgs, string accelerator, string filter = "")
    {
        Vendor = vendor;
        FfmpegArgs = ffmpegArgs;
        Filter = filter;
        Accelerator = accelerator;
    }
}