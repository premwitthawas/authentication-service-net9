using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace AuthService.Models
{
    public class Permission
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public bool Read { get; set; }
        [Required]
        public bool Write { get; set; }
        [Required]
        public bool Update { get; set; }
        [Required]
        public bool Delete { get; set; }
        public ICollection<User> Users { get; set; }
    }
}