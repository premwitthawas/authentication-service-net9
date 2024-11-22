using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Models;

public class User
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string UserName { get; set; }
    [Required]
    public string Password { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [ForeignKey("Role")]
    public int RoleId { get; set; }
    public Role Role { get; set; }
    public bool IsVerified { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}