using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.Helpers;
using Serilog.Events;
using Serilog.Events;

namespace NoMercy.Server.system;

public interface IShouldQueue
{
    Task Handle();
}

public static class JobDispatcher
{
    private static readonly JobQueue Queue = new(new QueueContext());

    public static void Dispatch(IShouldQueue job, string onQueue = "default", int priority = 0)
    {
        var jobData = new QueueJob()
        {
            Queue = onQueue,
            Payload = SerializationHelper.Serialize(job),
            AvailableAt = DateTime.UtcNow,
            Priority = priority
        };

        try
        {
            Queue.Enqueue(jobData);
        }
        catch (Exception e)
        {
            Logger.Queue(e, LogEventLevel.Error);
        }
    }
}