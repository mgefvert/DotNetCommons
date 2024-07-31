using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCommons.Services.Models;

[Table("services_idp_users")]
public class IdpUser
{
    public int Id { get; set; }
    public bool Active { get; set; }
    public string? Email { get; set; }
    public string? Hash { get; set; }
    public int Magic { get; set; }
    public int LoginFails { get; set; }
    public bool LoginLocked { get; set; }
    public string? Gender { get; set; }
    public DateTime CreatedZ { get; set; }
    public DateTime UpdatedZ { get; set; }

    public List<IdpUserRight>? Rights { get; set; }
}