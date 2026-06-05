using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCommons.Services.JobQueue;

/// Worker process entity
[Table("job_workers")]
public class DbJobWorker
{
    /// Unique ID for each worker process
    [Key]
    public int Id { get; set; }

    /// Timestamp of last heartbeat
    [Required]
    public DateTime LastSeenZ { get; set; }

    /// IPv4/v6 address
    [Required, MaxLength(50)]
    public string Address { get; set; } = "";

    /// Unique name for this worker (machinename : process)
    [Required, MaxLength(50)]
    public string Name { get; set; } = "";

    /// Minimum worker heartbeat time before jobs are reclaimed
    public TimeSpan EvictAfter { get; set; }
}