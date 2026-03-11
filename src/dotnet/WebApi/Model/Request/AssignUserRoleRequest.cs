namespace WebApi.Model.Request
{
    public class AssignUserRoleRequest
    {
        public long UserId { get; set; }

        public List<long> RoleIds { get; set; } = [];
    }
}
