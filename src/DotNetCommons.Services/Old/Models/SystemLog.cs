using System.ComponentModel.DataAnnotations;

namespace DotNetCommons.Services.Old.Models;

public class SystemLog
{
    public int Id { get; set; }
    public DateTime TimeZ { get; set; }
    public string? Ip { get; set; }
    [Required]
    public string? LogType { get; set; }
    [Required]
    public string? Level { get; set; }
    public int? UserId { get; set; }
    public string? Data { get; set; }
}