using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCommons.Services.JobQueue;

/// Job type entity
[Table("job_types")]
public class DbJobType
{
    /// Unique ID
    [Key]
    public int Id { get; set; }

    /// Job name
    [Required, MaxLength(50)]
    public string Name { get; set; } = "";

    /// Default expiration of new jobs
    public TimeSpan? DefaultExpiration { get; set; }

    /// Default priority for new jobs
    public int DefaultPriority { get; set; }

    /// Default wait time before rescheduling a job
    public TimeSpan RescheduleDelay { get; set; }
}