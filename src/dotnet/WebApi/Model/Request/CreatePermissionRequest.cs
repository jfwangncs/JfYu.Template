namespace WebApi.Model.Request
{
    public class CreatePermissionRequest
    {
        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
