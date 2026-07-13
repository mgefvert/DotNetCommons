using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCommons.Services.Rdap.Entities;

[Table("rdap_cache")]
public class RdapCache
{
    [Key]
    public int Id { get; set; }

    public ulong Start1 { get; set; }
    public ulong Start2 { get; set; }
    public ulong End1 { get; set; }
    public ulong End2 { get; set; }
    public double Size { get; set; }

    [Required, StringLength(50)]
    public string? StartAddress { get; set; }

    [Required, StringLength(50)]
    public string? EndAddress { get; set; }

    public DateTime UpdatedZ { get; set; }

    [StringLength(255)]
    public string? Organization { get; set; }

    [StringLength(255)]
    public string? Country { get; set; }
}