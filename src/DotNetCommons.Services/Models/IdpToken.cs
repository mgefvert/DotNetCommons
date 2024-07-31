using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCommons.Services.Models;

[Table("services_idp_tokens")]
public class IdpToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    [Required]
    public string? TokenType { get; set; }
    [Required]
    public string? Token { get; set; }
    public DateTime CreatedZ { get; set; }
    public DateTime ExpiresZ { get; set; }
}