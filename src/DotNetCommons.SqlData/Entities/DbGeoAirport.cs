using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotNetCommons.EF;

namespace DotNetCommons.SqlData.Entities;

[Table("geo_airports")]
public class DbGeoAirport
{
    [Key] public int Id { get; set; }

    [Patch] public string? Ident { get; set; }
    [Patch] public string? Type { get; set; }
    [Patch] public string? IcaoCode { get; set; }
    [Patch] public string? IataCode { get; set; }
    [Patch] public string? Name { get; set; }
    [Patch] public double Latitude { get; set; }
    [Patch] public double Longitude { get; set; }
    [Patch] public int? Elevation { get; set; }
    [Patch] public string? Continent { get; set; }
    [Patch] public string? Country { get; set; }
    [Patch] public string? Region { get; set; }
    [Patch] public string? Municipality { get; set; }

    public bool IsValid => Ident.IsSet() && Type.IsSet() && Name.IsSet();
}