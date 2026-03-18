namespace WebApi.Model.Role
{
  public class UpdateRoleRequest
  {
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? Status { get; set; }
  }
}
