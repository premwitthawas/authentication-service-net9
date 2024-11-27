using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Models
{
    public class Session
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [StringLength(50)]
        public string TokenType { get; set; } = "Bearer";
        [Required]
        [StringLength(50)]
        public string Provider { get; set; } = "local";

        [Required]
        [StringLength(2000)]
        public string AccessToken { get; set; }
        [Required]
        [StringLength(2000)]
        public string RefreshToken { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public DateTime AccessExpiresAt { get; set; }
        [Required]
        public DateTime RefreshExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }
        [Required]
        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public User User { get; set; }
        public bool IsAccessTokenExpired()
        {
            return DateTime.UtcNow > AccessExpiresAt;
        }
        public bool IsRefreshTokenExpired()
        {
            return DateTime.UtcNow > RefreshExpiresAt;
        }
        public bool IsActive()
        {
            return !IsRevoked && !IsAccessTokenExpired() && !IsRefreshTokenExpired();
        }
    }
}
