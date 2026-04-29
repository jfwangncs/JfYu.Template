using JfYu.Data.Model;
using JfYu.Data.Service;
using JfYu.WebApi.Template.Entity;
using JfYu.WebApi.Template.Model;

namespace JfYu.WebApi.Template.Services.Interfaces
{
    public interface IRoleService : IService<Role, AppDbContext>
    {
        Task<PagedData<Role>> GetPagedAsync(QueryRequest query);

        Task AssignPermissionsAsync(int roleId, List<int> permissionIds);
    }
}
