using WebApi.Entity;
using WebApi.Model.Request;

namespace WebApi.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<List<Permission>> GetAllAsync();

        Task<Permission?> GetByIdAsync(long id);

        Task<Permission> CreateAsync(CreatePermissionRequest request);

        Task<Permission?> UpdateAsync(long id, UpdatePermissionRequest request);

        Task<bool> DeleteAsync(long id);
    }
}
