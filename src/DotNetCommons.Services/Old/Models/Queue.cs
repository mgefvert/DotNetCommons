using System.ComponentModel.DataAnnotations;

namespace DotNetCommons.Services.Old.Models;

public class Queue
{
    public int Id { get; set; }
    public DateTime CreatedZ { get; set; }
    public DateTime ExpiresZ { get; set; }
    public DateTime RetryAtZ { get; set; }
    public int RetryAttempts { get; set; }
    [Required]
    public string? QueueName { get; set; }
    public string? Data { get; set; }
}