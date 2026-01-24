using System.ComponentModel.DataAnnotations;

namespace DatabaseSeeder.Entities;

public class Airport
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(20)]
    public string Ident { get; set; } = null!;

    public int Type { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = null!;

    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Elevation { get; set; }
}