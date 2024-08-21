using NoMercy.MediaProcessing.Jobs.PaletteJobs;

namespace NoMercy.MediaProcessing.Jobs;

public class JobDispatcher
{
    internal void DispatchJob<TJob, TChild>(int id, IEnumerable<TChild> jobItems)
        where TJob : AbstractPaletteJob<TChild>, new() {
        var job = new TJob { Id = id, Storage = jobItems.ToArray() };
        Queue.JobDispatcher.Dispatch(job, job.QueueName, job.Priority);
    }
    
}