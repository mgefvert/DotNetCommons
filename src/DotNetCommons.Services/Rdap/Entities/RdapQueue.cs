using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCommons.Services.Rdap.Entities;

[Table("rdap_queue")]
public class RdapQueue
{
    [Key, Required, StringLength(50)]
    public string? Address { get; set; }
}