namespace WebApi.Model.Role
{
    public class CreateRoleRequest
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
