using JfYu.Data.Model;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Entity
{
    public class Permission : BaseEntity
    { 

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        [MaxLength(200)]
        public string? Path { get; set; }

        [MaxLength(200)]
        public string? Component { get; set; }

        [MaxLength(50)]
        public string? Icon { get; set; }

        public bool IsActive { get; set; } = true;
        public int Sort { get; set; }
        public int? ParentId { get; set; }

        public Permission? Parent { get; set; }

        public ICollection<Permission> Children { get; set; } = [];

        public ICollection<Role> Roles { get; set; } = [];
    }
}
