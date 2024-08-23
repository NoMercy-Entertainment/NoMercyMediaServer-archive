using NoMercy.Database.Models;
using NoMercy.MediaProcessing.Jobs.MediaJobs;
using NoMercy.MediaProcessing.Jobs.PaletteJobs;

namespace NoMercy.MediaProcessing.Jobs;

public class JobDispatcher
{
    public void DispatchJob<TJob>(int id, Ulid libraryId)
        where TJob : AbstractMediaJob, new()
    {
        TJob? job = new TJob { Id = id, LibraryId = libraryId };
        Queue.JobDispatcher.Dispatch(job, job.QueueName, job.Priority);
    }

    public void DispatchJob<TJob>(int id, Library library)
        where TJob : AbstractMediaJob, new()
    {
        TJob? job = new TJob { Id = id, LibraryId = library.Id };
        Queue.JobDispatcher.Dispatch(job, job.QueueName, job.Priority);
    }

    internal void DispatchJob<TJob, TChild>(int id, IEnumerable<TChild> jobItems)
        where TJob : AbstractPaletteJob<TChild>, new()
    {
        TJob? job = new TJob { Id = id, Storage = jobItems.ToArray() };
        Queue.JobDispatcher.Dispatch(job, job.QueueName, job.Priority);
    }

    internal void DispatchJob<TJob, TChild>(TChild data)
        where TJob : AbstractMediaExraDataJob<TChild>, new()
    {
        TJob? job = new TJob { Storage = data };
        Queue.JobDispatcher.Dispatch(job, job.QueueName, job.Priority);
    }

    internal void DispatchJob<TJob, TChild>(IEnumerable<TChild> data, string name)
        where TJob : AbstractShowExtraDataJob<TChild, string>, new()
    {
        TJob? job = new TJob { Storage = data, Name = name };
        Queue.JobDispatcher.Dispatch(job, job.QueueName, job.Priority);
    }
}