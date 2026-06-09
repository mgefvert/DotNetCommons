using Microsoft.EntityFrameworkCore;

namespace DotNetCommons.Services.JobQueue;

public class JobDbContext : DbContext
{
    public DbSet<DbJobArchive> JobArchive { get; set; }
    public DbSet<DbJobQueue> JobQueue { get; set; }
    public DbSet<DbJobType> JobTypes { get; set; }
    public DbSet<DbJobWorker> JobWorkers { get; set; }
}