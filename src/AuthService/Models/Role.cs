using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace AuthService.Models;

public class Role
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string RoleName { get; set; } = string.Empty;
}
