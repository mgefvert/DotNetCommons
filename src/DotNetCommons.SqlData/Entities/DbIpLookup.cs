using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCommons.SqlData.Entities;

[Table("ip_lookup")]
public class DbIpLookup
{
    [Key]
    public int Id { get; set; }

    [StringLength(255), Required]
    public string? Name { get; set; }
}
