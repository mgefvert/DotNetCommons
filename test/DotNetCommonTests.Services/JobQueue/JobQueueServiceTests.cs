using System.Net;
using DotNetCommons.Services;
using DotNetCommons.Services.JobQueue;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotNetCommonTests.Services.JobQueue;

[TestClass]
public class JobQueueServiceTests
{
    private static readonly DateTimeOffset TestNow = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);

    private SqliteConnection _connection = null!;
    private TestJobDbContext _context = null!;
    private TestTimeProvider _clock = null!;
    private JobQueueService _service = null!;
    private DbJobType _emailJobType = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();

        _context = new TestJobDbContext(_connection);
        await _context.Database.EnsureCreatedAsync();

        _clock = new TestTimeProvider(TestNow);
        _service = new JobQueueService(_context, NullLogger<JobQueueService>.Instance, _clock);

        _emailJobType = new DbJobType
        {
            Name              = "email",
            DefaultPriority   = 50,
            DefaultExpiration = TimeSpan.FromHours(2),
            RescheduleDelay   = TimeSpan.FromMinutes(5)
        };

        _context.JobTypes.Add(_emailJobType);
        await _context.SaveChangesAsync();
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [TestMethod]
    public async Task RegisterWorker_WithNewWorker_PersistsWorker()
    {
        var result = await _service.RegisterWorker("worker-1", IPAddress.Loopback, TimeSpan.FromMinutes(10));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeGreaterThan(0);

        var worker = await _context.JobWorkers.SingleAsync();
        worker.Id.Should().Be(result.Value);
        worker.Name.Should().Be("worker-1");
        worker.Address.Should().Be(IPAddress.Loopback.ToString());
        worker.EvictAfter.Should().Be(TimeSpan.FromMinutes(10));
        worker.LastSeenZ.Should().Be(TestNow.UtcDateTime);
    }

    [TestMethod]
    public async Task RegisterWorker_WithDuplicateName_Fails()
    {
        var first = await _service.RegisterWorker("worker-1", IPAddress.Loopback, TimeSpan.FromMinutes(10));
        first.IsSuccess.Should().BeTrue();

        var second = await _service.RegisterWorker("worker-1", IPAddress.IPv6Loopback, TimeSpan.FromMinutes(10));

        second.IsFailure.Should().BeTrue();
        second.Error.Should().BeSameAs(Errors.WorkerAlreadyRegistered);
        (await _context.JobWorkers.CountAsync()).Should().Be(1);
    }

    [TestMethod]
    public async Task QueueJob_WithKnownType_PersistsJobUsingTypeDefaults()
    {
        var result = await _service.QueueJob("job-1", "email", """{"to":"test@example.com"}""");

        result.IsSuccess.Should().BeTrue();

        var job = await _context.JobQueue.SingleAsync();
        job.JobTypeId.Should().Be(_emailJobType.Id);
        job.JobId.Should().Be("job-1");
        job.Payload.Should().Be("""{"to":"test@example.com"}""");
        job.Priority.Should().Be(_emailJobType.DefaultPriority);
        job.Attempts.Should().Be(0);
        job.CreatedZ.Should().Be(TestNow.UtcDateTime);
        job.AvailableZ.Should().Be(TestNow.UtcDateTime);
        job.JobExpiresZ.Should().Be(TestNow.UtcDateTime.Add(_emailJobType.DefaultExpiration!.Value));
    }

    [TestMethod]
    public async Task QueueJob_WithUnknownType_ReturnsJobTypeNotFound()
    {
        var result = await _service.QueueJob("job-1", "missing", "{}");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeSameAs(Errors.JobTypeNotFound);
        (await _context.JobQueue.CountAsync()).Should().Be(0);
    }

    [TestMethod]
    public async Task QueueJob_WithDuplicateJobId_ReturnsJobAlreadyExists()
    {
        var first = await _service.QueueJob("job-1", "email", "{}");
        first.IsSuccess.Should().BeTrue();

        var second = await _service.QueueJob("job-1", "email", "{}");

        second.IsFailure.Should().BeTrue();
        second.Error.Should().BeSameAs(Errors.JobAlreadyExists);
        (await _context.JobQueue.CountAsync()).Should().Be(1);
    }

    [TestMethod]
    public async Task ClaimJob_WithAvailableJob_AssignsWorkerAndReturnsClaimedJob()
    {
        var worker = await _service.RegisterWorker("worker-1", IPAddress.Loopback, TimeSpan.FromMinutes(10));
        await _service.QueueJob("job-1", "email", "{}");

        _context.ChangeTracker.Clear();

        var result = await _service.ClaimJob(worker.Value, "email", TimeSpan.FromMinutes(15));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(j => j.JobId == "job-1");

        _context.ChangeTracker.Clear();
        var job = await _context.JobQueue.SingleAsync();
        job.WorkerId.Should().Be(worker.Value);
        job.StartedZ.Should().Be(TestNow.UtcDateTime);
        job.WorkerExpiresZ.Should().Be(TestNow.UtcDateTime.AddMinutes(15));
    }

    [TestMethod]
    public async Task CompleteJob_WithOwnedJob_ClosesJobSuccessfully()
    {
        var worker = await _service.RegisterWorker("worker-1", IPAddress.Loopback, TimeSpan.FromMinutes(10));
        await _service.QueueJob("job-1", "email", "{}");

        _context.ChangeTracker.Clear();
        var claim = await _service.ClaimJob(worker.Value, "email", TimeSpan.FromMinutes(15));
        claim.IsSuccess.Should().BeTrue();

        var result = await _service.CompleteJob(worker.Value, "job-1", """{"ok":true}""");

        result.IsSuccess.Should().BeTrue();

        _context.ChangeTracker.Clear();
        var job = await _context.JobQueue.SingleAsync();
        job.ClosedZ.Should().Be(TestNow.UtcDateTime);
        job.Success.Should().BeTrue();
        job.Result.Should().Be("""{"ok":true}""");
    }

    private sealed class TestJobDbContext(SqliteConnection connection) : JobDbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(connection);
        }
    }

    private sealed class TestTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Advance(TimeSpan value)
        {
            _utcNow = _utcNow.Add(value);
        }
    }
}
