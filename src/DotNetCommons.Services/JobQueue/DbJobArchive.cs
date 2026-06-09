using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCommons.Services.JobQueue;

/// Job archive entity
[Table("job_archive")]
public class DbJobArchive
{
    /// Job serial ID
    [Key]
    public long Id { get; set; }

    /// Job ID, links to job_types
    [Required]
    public int JobTypeId { get; set; }

    /// UUID
    [Required, MaxLength(36)]
    public string JobId { get; set; } = "";

    /// Job priority
    [Required]
    public int Priority { get; set; } = 0;

    /// Attempts
    [Required]
    public int Attempts { get; set; } = 0;

    /// Job creation time
    [Required]
    public DateTime CreatedZ { get; set; }

    /// When job was picked up by a worker, NULL if no worker processing
    public DateTime? StartedZ { get; set; }

    /// When this job was completed
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
}