namespace WebApi.Model.Role
{
    public class AssignUserRoleRequest
    {
        public long UserId { get; set; }

        public List<long> RoleIds { get; set; } = [];
    }
}
