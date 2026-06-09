using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCommons.Services.JobQueue;

/// Job queue entity
[Table("job_queue")]
public class DbJobQueue
{
    /// Job serial ID
    [Key]
    public long Id { get; set; }

    /// Job ID, links to job_types
    [Required]
    public int JobTypeId { get; set; }

    /// UUID, must be unique
    [Required, MaxLength(36)]
    public string JobId { get; set; } = "";

    /// Lower priority goes first
    [Required]
    public int Priority { get; set; } = 0;

    /// Number of attempts
    [Required]
    public int Attempts { get; set; } = 0;

    /// Job creation time
    [Required]
    public DateTime CreatedZ { get; set; }

    /// Job expiration date/time
    public DateTime? AvailableZ { get; set; }

    /// When job was picked up by a worker, NULL if no worker processing
    public DateTime? StartedZ { get; set; }

    /// Worker ID that picked it up
    public int? WorkerId { get; set; }

    /// Job expiration date/time
    public DateTime? WorkerExpiresZ { get; set; }

    /// Job expiration date/time
    public DateTime? JobExpiresZ { get; set; }

    /// When this job was completed; signals transfer to job_archive
    public DateTime? ClosedZ { get; set; }

    /// Success or fail?
    public bool? Success { get; set; }

    /// Payload
    [Column("payload", TypeName = "json")]
    public string? Payload { get; set; }

    /// Result
    [Column("result", TypeName = "json")]
    public string? Result { get; set; }

    /// Textual error on failure
    [MaxLength(255)]
    public string? Error { get; set; }

    public DbJobType? JobType { get; set; }

    public DbJobArchive ToArchive() => new()
    {
        JobTypeId = JobTypeId,
        JobId     = JobId,
        Priority  = Priority,
        Attempts  = Attempts,
        CreatedZ  = CreatedZ,
        StartedZ  = StartedZ,
        ClosedZ   = ClosedZ,
        Success   = Success,
        Payload   = Payload,
        Result    = Result,
        Error     = Error,
    };
}