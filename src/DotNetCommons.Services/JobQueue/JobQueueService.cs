using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotNetCommons.Services.JobQueue;

public class JobQueueService : IJobQueueService
{
    private readonly JobDbContext _context;
    private readonly ILogger<JobQueueService> _logger;
    private readonly TimeProvider _clock;

    private DbJobType[]? _jobTypes;

    public JobQueueService(JobDbContext context, ILogger<JobQueueService> logger, TimeProvider clock)
    {
        _context = context;
        _logger  = logger;
        _clock   = clock;
    }

    public async Task<DbJobType[]> ListJobTypes()
    {
        if (_jobTypes != null)
            return _jobTypes;

        return _jobTypes = await _context.JobTypes.ToArrayAsync();
    }

    public async Task<Result<int>> RegisterWorker(string name, IPAddress address, TimeSpan evictAfter)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(evictAfter.TotalSeconds, nameof(evictAfter));

        if (_context.JobWorkers.Any(w => w.Name == name))
            return Errors.WorkerAlreadyRegistered;

        var worker = new DbJobWorker
        {
            LastSeenZ  = _clock.UtcNow(),
            Address    = address.ToString(),
            Name       = name,
            EvictAfter = evictAfter
        };

        _context.JobWorkers.Add(worker);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Registered worker {Worker} ({Address}) with ID {ID}", worker.Name, worker.Address, worker.Id);
        return worker.Id;
    }

    public async Task<Result> PingWorker(int workerId)
    {
        var worker = await _context.JobWorkers.FindAsync(workerId);
        if (worker == null)
            return Errors.WorkerNotFound;

        worker.LastSeenZ = _clock.UtcNow();
        await _context.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task UnregisterWorker(int workerId)
    {
        var worker = await _context.JobWorkers.FindAsync(workerId);
        if (worker == null)
            return;

        _context.JobWorkers.Remove(worker);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Unregistered worker {Worker} ({Address}) with ID {ID}", worker.Name, worker.Address, worker.Id);
    }

    public Task<DbJobWorker[]> ListWorkers()
    {
        return _context.JobWorkers.ToArrayAsync();
    }

    public async Task<Result> QueueJob(string jobId, string jobType, string payload, int? priority = null, DateTimeOffset? available = null,
        DateTimeOffset? expires = null)
    {
        var types = await ListJobTypes();
        var type  = types.SingleOrDefault(t => t.Name == jobType);
        if (type == null)
            return Errors.JobTypeNotFound;

        var exists = await _context.JobQueue.AnyAsync(j => j.JobId == jobId);
        if (exists)
            return Errors.JobAlreadyExists;

        var job = new DbJobQueue
        {
            JobTypeId = type.Id,
            JobId = jobId,
            Priority = priority ?? type.DefaultPriority,
            Attempts = 0,
            CreatedZ = _clock.UtcNow(),
            AvailableZ = available?.UtcDateTime ?? _clock.UtcNow(),
            JobExpiresZ = expires?.UtcDateTime ?? (type.DefaultExpiration != null ? _clock.UtcNow().Add(type.DefaultExpiration.Value) : null),
            Payload = payload
        };

        _context.JobQueue.Add(job);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Queued job {JobId} ({JobType}) with ID {ID}", job.JobId, job.JobTypeId, job.Id);
        return Result.Ok();
    }

    public async Task<Result> CancelJob(string jobId)
    {
        var exists = await _context.JobQueue.AnyAsync(j => j.JobId == jobId);
        if (!exists)
            return Errors.JobNotFound;

        var n = await _context.JobQueue.Where(x => x.JobId == jobId && x.StartedZ == null && x.ClosedZ == null)
            .ExecuteUpdateAsync(setter => setter
                .SetProperty(x => x.ClosedZ, _clock.UtcNow())
                .SetProperty(x => x.Success, false)
                .SetProperty(x => x.Error, "Job canceled")
            );

        return n > 0 ? Result.Ok() : Errors.JobIsProcessing;
    }

    public async Task<Result<DbJobQueue[]>> ClaimJob(int workerId, string jobType, TimeSpan workerExpires)
    {
        // TODO: Make a claim ID and stamp the job row with that, then query for it - so we only return one new row.

        var worker = await _context.JobWorkers.FindAsync(workerId);
        if (worker == null)
            return Errors.WorkerNotFound;

        var types = await ListJobTypes();
        var type  = types.SingleOrDefault(t => t.Name == jobType);
        if (type == null)
            return Errors.JobTypeNotFound;

        var result = await PingWorker(workerId);
        if (result.IsFailure)
            return result.Error!;

        var now = _clock.UtcNow();
        var n = await _context.JobQueue
            .Where(j => j.JobTypeId == type.Id && j.WorkerId == null && j.AvailableZ <= now && j.ClosedZ == null)
            .Where(j => j.JobExpiresZ == null || j.JobExpiresZ > now)
            .OrderBy(j => j.Priority)
            .ThenBy(j => j.AvailableZ)
            .Take(1)
            .ExecuteUpdateAsync(setter => setter
                .SetProperty(x => x.WorkerId, workerId)
                .SetProperty(x => x.StartedZ, now)
                .SetProperty(x => x.WorkerExpiresZ, now.Add(workerExpires))
                .SetProperty(x => x.Error, (string?)null)
            );

        if (n == 0)
            return Result<DbJobQueue[]>.Ok([]);

        var jobs = await _context.JobQueue.Where(j => j.JobTypeId == type.Id && j.WorkerId == workerId).ToArrayAsync();
        _logger.LogInformation("Worker {Worker} ({JobType}) started working on job {JobId}", workerId, type.Name, string.Join(",", jobs.Select(j => j.JobId)));
        return jobs;
    }

    public async Task<Result> CompleteJob(int workerId, string jobId, string? resultJson)
    {
        var job = await _context.JobQueue.SingleOrDefaultAsync(j => j.JobId == jobId);
        if (job == null)
            return Errors.JobNotFound;

        if (job.ClosedZ != null)
            return Errors.JobAlreadyClosed;

        if (job.WorkerId != workerId)
            return Errors.JobIsNotOwnedByWorker;

        job.ClosedZ = _clock.UtcNow();
        job.Success = true;
        job.Result = resultJson;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Completed job {JobId} ({JobType}) with ID {ID}; elapsed={Elapsed}",
            job.JobId, job.JobTypeId, job.Id, job.ClosedZ - job.StartedZ);
        return Result.Ok();
    }

    public async Task<Result> RescheduleJob(int workerId, string jobId, string error)
    {
        var job = await _context.JobQueue.SingleOrDefaultAsync(j => j.JobId == jobId);
        if (job == null)
            return Errors.JobNotFound;

        if (job.ClosedZ != null)
            return Errors.JobAlreadyClosed;

        if (job.WorkerId != workerId)
            return Errors.JobIsNotOwnedByWorker;

        var types = await ListJobTypes();
        var jobType = types.SingleOrDefault(t => t.Id == job.JobTypeId);
        if (jobType == null)
            return Errors.JobTypeNotFound;

        var now = _clock.UtcNow();

        // Check if job has expired
        if (job.JobExpiresZ != null && now >= job.JobExpiresZ.Value)
            return await FailJob(workerId, jobId, error);

        // Reschedule with back-off strategy
        job.Attempts++;
        var backoffMultiplier = Math.Pow(2, job.Attempts - 1); // Exponential back-off: 1x, 2x, 4x, 8x, etc.
        var delay = TimeSpan.FromTicks((long)(jobType.RescheduleDelay.Ticks * backoffMultiplier));
        job.AvailableZ = now.Add(delay);
        job.StartedZ = null;
        job.WorkerId = null;
        job.Error = error;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Rescheduled job {JobId} ({JobType}) with ID {ID}; attempt={Attempt}, availableAt={AvailableAt}, error={Error}",
            job.JobId, job.JobTypeId, job.Id, job.Attempts, job.AvailableZ, error);

        return Result.Ok();
    }

    public async Task<Result> FailJob(int workerId, string jobId, string error)
    {
        var job = await _context.JobQueue.SingleOrDefaultAsync(j => j.JobId == jobId);
        if (job == null)
            return Errors.JobNotFound;

        if (job.ClosedZ != null)
            return Errors.JobAlreadyClosed;

        if (job.WorkerId != workerId)
            return Errors.JobIsNotOwnedByWorker;

        job.ClosedZ = _clock.UtcNow();
        job.Success = false;
        job.Error   = error;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Job failed: {JobId} ({JobType}) with ID {ID}; error={Error}", job.JobId, job.JobTypeId, job.Id, job.Error);
        return Result.Ok();
    }

    public async Task<JobStats[]> GetJobStatsAll(DateTimeOffset? since = null)
    {
        if (since == null)
        {
            return await _context.JobQueue
                .Where(x => x.ClosedZ == null)
                .Select(x => new { x.JobTypeId, x.StartedZ, x.ClosedZ })
                .GroupBy(x => x.JobTypeId)
                .Select(g => new JobStats(
                    g.Key,
                    g.Count(j => j.StartedZ == null && j.ClosedZ == null),
                    g.Count(j => j.StartedZ != null && j.ClosedZ == null),
                    null,
                    null))
                .ToArrayAsync();
        }

        var cutOff = since.Value.UtcDateTime;
        return await _context.JobQueue
            .Where(j => j.ClosedZ == null || j.ClosedZ >= cutOff)
            .Select(x => new { x.JobTypeId, x.StartedZ, x.ClosedZ, x.Success })
            .GroupBy(x => x.JobTypeId)
            .Select(g => new JobStats(
                g.Key,
                g.Count(j => j.StartedZ == null && j.ClosedZ == null),
                g.Count(j => j.StartedZ != null && j.ClosedZ == null),
                g.Count(j => j.ClosedZ != null && j.Success == true),
                g.Count(j => j.ClosedZ != null && j.Success == false)))
            .ToArrayAsync();
    }

    public async Task<JobStats> GetJobStatsForJob(string jobName, DateTimeOffset? since = null)
    {
        var types = await ListJobTypes();
        var type = types.SingleOrDefault(t => t.Name == jobName);
        if (type == null)
            return new JobStats(null, null, null, null, null);

        if (since == null)
        {
            var query = _context.JobQueue.Where(j => j.JobTypeId == type.Id);
            return new JobStats(
                type.Id,
                await query.CountAsync(j => j.StartedZ == null && j.ClosedZ == null),
                await query.CountAsync(j => j.StartedZ != null && j.ClosedZ == null),
                null,
                null
            );
        }
        else
        {
            var cutOff = since.Value.UtcDateTime;
            var query  = _context.JobQueue.Where(j => j.JobTypeId == type.Id && (j.ClosedZ == null || j.ClosedZ >= cutOff));
            return new JobStats(
                type.Id,
                await query.CountAsync(j => j.ClosedZ == null && j.StartedZ == null),
                await query.CountAsync(j => j.ClosedZ == null && j.StartedZ != null),
                await query.CountAsync(j => j.ClosedZ != null && j.Success == true),
                await query.CountAsync(j => j.ClosedZ != null && j.Success == false)
            );
        }
    }

    public async Task<JobStats> GetJobStatsForWorkerId(int workerId, DateTimeOffset? since = null)
    {
        if (since == null)
            return new JobStats(
                workerId,
                null,
                await _context.JobQueue.CountAsync(j => j.StartedZ != null && j.ClosedZ == null && j.WorkerId == workerId),
                null,
                null
            );

        var cutOff = since.Value.UtcDateTime;
        var query  = _context.JobQueue.Where(j => j.WorkerId == workerId && (j.ClosedZ == null || j.ClosedZ >= cutOff));
        return new JobStats(
            workerId,
            null,
            await query.CountAsync(j => j.ClosedZ == null && j.StartedZ != null),
            await query.CountAsync(j => j.ClosedZ != null && j.Success == true),
            await query.CountAsync(j => j.ClosedZ != null && j.Success == false)
        );
    }

    public async Task ArchiveOldJobs(TimeSpan olderThan)
    {
        var cutoff = _clock.UtcNow().Subtract(olderThan);

        var jobs = await _context.JobQueue.Where(j => j.ClosedZ < cutoff).ToArrayAsync();
        _context.JobQueue.RemoveRange(jobs);
        _context.JobArchive.AddRange(jobs.Select(j => j.ToArchive()));
        await _context.SaveChangesAsync();
    }

    public async Task RunMaintenance()
    {
        var now = _clock.UtcNow();

        // Evict stale workers (where lastSeenZ + evictAfter < now)

        var staleWorkers = (await _context.JobWorkers.ToArrayAsync())
            .Where(w => w.LastSeenZ.Add(w.EvictAfter) < now)
            .ToArray();

        if (staleWorkers.Length > 0)
        {
            _context.JobWorkers.RemoveRange(staleWorkers);
            _logger.LogInformation("Evicted {Count} stale workers: {Workers}",
                staleWorkers.Length,
                string.Join(", ", staleWorkers.Select(w => $"{w.Name} (ID: {w.Id})")));
        }

        // Permanently fail expired jobs (jobExpiresZ < now)

        var expiredJobs = await _context.JobQueue
            .Where(j => j.JobExpiresZ != null && j.JobExpiresZ < now && j.ClosedZ == null)
            .ExecuteUpdateAsync(setter => setter
                .SetProperty(x => x.ClosedZ, now)
                .SetProperty(x => x.Success, false)
                .SetProperty(x => x.Error, "Job expired"));

        if (expiredJobs > 0)
            _logger.LogInformation("Permanently failed {Count} expired jobs", expiredJobs);

        // Reclaim jobs that started but weren't closed (workerExpiresZ < now)
        // TODO: Look at the heartbeat thingy. Maybe we should include that in here?

        var reclaimedJobs = await _context.JobQueue
            .Where(j => j.WorkerExpiresZ != null && j.WorkerExpiresZ < now && j.ClosedZ == null)
            .ExecuteUpdateAsync(setter => setter
                .SetProperty(x => x.WorkerId, (int?)null)
                .SetProperty(x => x.StartedZ, (DateTime?)null)
                .SetProperty(x => x.WorkerExpiresZ, (DateTime?)null)
                .SetProperty(x => x.Error, "Worker expired - job reclaimed")
                .SetProperty(x => x.Attempts, x => x.Attempts + 1));

        if (reclaimedJobs > 0)
            _logger.LogInformation("Reclaimed {Count} jobs from expired workers", reclaimedJobs);

        await _context.SaveChangesAsync();
    }
}