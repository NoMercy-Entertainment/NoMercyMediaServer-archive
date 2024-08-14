#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace NoMercy.Helpers.Monitoring
{
    public static class StorageMonitor
    {
        public static List<ResourceMonitorDto> Main()
        {
            var allDrives = DriveInfo.GetDrives();
            List<ResourceMonitorDto> resourceMonitorDtos = [];

            foreach (var d in allDrives)
            {
                var resourceMonitorDto = new ResourceMonitorDto
                {
                    Name = d.Name,
                    Type = d.DriveType.ToString()
                };
                if (d.IsReady)
                {
                    resourceMonitorDto.Total = (float)d.TotalSize / 1024 / 1024 / 1024;
                    resourceMonitorDto.Available = (float)d.AvailableFreeSpace / 1024 / 1024 / 1024;
                }

                resourceMonitorDtos.Add(resourceMonitorDto);
            }

            return resourceMonitorDtos;
        }
    }
}

public class ResourceMonitorDto
{
    public string Name { get; set; }
    public string Type { get; set; }
    public float Total { get; set; }
    public float Available { get; set; }
    public float Percentage => (Available / Total) * 100;
}