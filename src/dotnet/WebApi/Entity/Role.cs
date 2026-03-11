using JfYu.Data.Model;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Entity
{
    public class Role : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Permission> Permissions { get; set; } = [];

        public ICollection<User> Users { get; set; } = [];
    }
}
