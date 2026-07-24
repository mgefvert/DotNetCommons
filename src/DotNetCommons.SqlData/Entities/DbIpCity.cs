using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCommons.SqlData.Entities;

[Table("ip_city")]
public class DbIpCity
{
    [Key, Required]
    public byte[]? Ip { get; set; }

    public int? Country { get; set; }
    public int? State { get; set; }
    public int? City { get; set; }
}
