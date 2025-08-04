using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCommons.Services.Old.Models;

[Table("services_idp_users_rights")]
public class IdpUserRight
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Right { get; set; }
    public DateTime Created { get; set; }

    public IdpUser? User { get; set; }
}