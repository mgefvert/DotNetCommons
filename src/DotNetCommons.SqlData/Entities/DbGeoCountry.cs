using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotNetCommons.EF.ObjectManagement;

namespace DotNetCommons.SqlData.Entities;

[Table("geo_countries")]
public class DbGeoCountry
{
    [Key] public int Id { get; set; }

    [Patch] public string? Iso2 { get; set; }
    [Patch] public string? Iso3 { get; set; }
    [Patch] public string? Name { get; set; }
    [Patch] public string? Capital { get; set; }
    [Patch] public string? TelCode { get; set; }
    [Patch] public string? Currency { get; set; }
    [Patch] public string? Continent { get; set; }
    [Patch] public string? Region { get; set; }
    [Patch] public string? Subregion { get; set; }
    
    public bool IsValid => Iso2.IsSet() && Iso3.IsSet() && Name.IsSet();
}
