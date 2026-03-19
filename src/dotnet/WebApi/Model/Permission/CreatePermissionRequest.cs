using WebApi.Constants;

namespace WebApi.Model.Permission
{
    public class CreatePermissionRequest
    {
        public required string Name { get; set; }
        public required string Code { get; set; }
        public PermissionType Type { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int Sort { get; set; }
        public int? ParentId { get; set; }
    }
}
