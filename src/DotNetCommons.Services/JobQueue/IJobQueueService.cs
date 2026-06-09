using System.Net;

namespace DotNetCommons.Services.JobQueue;

public record JobStats(int? Id, int? Queued, int? Running, int? Completed, int? Failed);

public interface IJobQueueService
{
    // Job types

    Task<DbJobType[]> ListJobTypes();

    // Workers

    Task<Result<int>> RegisterWorker(string name, IPAddress address, TimeSpan evictAfter);
    Task<Result> PingWorker(int workerId);
    Task UnregisterWorker(int workerId);
    Task<DbJobWorker[]> ListWorkers();

    // Jobs

    Task<Result> QueueJob(string jobId, string jobType, string payload,
        int? priority = null, DateTimeOffset? available = null, DateTimeOffset? expires = null);
    Task<Result> CancelJob(string jobId);

    // Claim jobs

    Task<Result<DbJobQueue[]>> ClaimJob(int workerId, string jobType, TimeSpan workerExpires);
    Task<Result> CompleteJob(int workerId, string jobId, string? resultJson);
    Task<Result> RescheduleJob(int workerId, string jobId, string error);
    Task<Result> FailJob(int workerId, string jobId, string error);

    // Job statistics

    Task<JobStats[]> GetJobStatsAll(DateTimeOffset? since = null);
    Task<JobStats> GetJobStatsForJob(string jobName, DateTimeOffset? since = null);
    Task<JobStats> GetJobStatsForWorkerId(int workerId, DateTimeOffset? since = null);

    // Maintenance

    Task ArchiveOldJobs(TimeSpan olderThan);
    Task RunMaintenance();
}