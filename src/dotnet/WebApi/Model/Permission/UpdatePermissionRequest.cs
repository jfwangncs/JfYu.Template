namespace WebApi.Model.Permission
{
    public class UpdatePermissionRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int? Sort { get; set; }
    }
}
