using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCommons.SqlData.Entities;

[Table("ip_country")]
public class DbIpCountry
{
    [Key, Required]
    public byte[]? Ip { get; set; }

    public int? Country { get; set; }
}
