using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotNetCommons.EF;

namespace DotNetCommons.SqlData.Entities;

[Table("geo_zip_codes")]
public class DbGeoZipCode
{
    [Key] public int Id { get; set; }

    [Patch] public string? Code { get; set; }
    [Patch] public string? City { get; set; }
    [Patch] public string? State { get; set; }
    [Patch] public string? County { get; set; }
    [Patch] public double? Latitude { get; set; }
    [Patch] public double? Longitude { get; set; }

    public bool IsValid => Code.IsSet() && State.IsSet();
}
