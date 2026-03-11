namespace WebApi.Model.Request
{
    public class AssignRolePermissionRequest
    {
        public long RoleId { get; set; }

        public List<long> PermissionIds { get; set; } = [];
    }
}
