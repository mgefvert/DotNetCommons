using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotNetCommons.EF;

namespace DotNetCommons.SqlData.Entities;

[Table("geo_area_codes")]
public class DbGeoAreaCode
{
    [Key] public int Id { get; set; }

    [Patch] public string? Code { get; set; }
    [Patch] public string? Country { get; set; }
    [Patch] public string? State { get; set; }

    public bool IsValid => Code.IsSet() && Country.IsSet();
}