using System.ComponentModel.DataAnnotations;
using DotNetCommons.EF;

namespace DatabaseSeeder.Entities;

public class AirportType
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(20)]
    public string Name { get; set; } = null!;

    [StringLength(255)]
    public string? Description { get; set; }
}