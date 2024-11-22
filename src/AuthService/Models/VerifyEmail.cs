using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Models;


public class VerifyEmail
{
    [Key]
    public Guid Id { get; set; }
    [ForeignKey("User")]
    public Guid UserId { get; set; }
    public User User { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
    public bool IsUsed { get; set; } = false;
    public bool IsExpired { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt  { get; set; }
}