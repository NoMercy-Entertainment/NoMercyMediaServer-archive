using NoMercy.NmSystem.Capabilities;
using NoMercy.EncoderV2.Hardware;

// Initialize the detector
var service = new HardwareAccelerationService();
var accelerators = service.GetAvailableAccelerators();

Console.WriteLine($"GPU Detection Results:");
Console.WriteLine($"  Total accelerators found: {accelerators.Count}");
foreach (var acc in accelerators)
{
    Console.WriteLine($"  - Vendor: {acc.Vendor}");
    Console.WriteLine($"    Accelerator: {acc.Accelerator}");
    Console.WriteLine($"    FFmpeg Args: {acc.FfmpegArgs}");
}

if (accelerators.Count == 0)
{
    Console.WriteLine("\nNo GPU acceleration detected!");
    Console.WriteLine("This could mean:");
    Console.WriteLine("  1. No dedicated GPU is installed");
    Console.WriteLine("  2. FFmpeg is not in PATH");
    Console.WriteLine("  3. FFmpeg doesn't support your GPU");
}
