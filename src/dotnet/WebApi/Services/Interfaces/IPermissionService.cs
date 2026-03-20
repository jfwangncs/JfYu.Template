using JfYu.Data.Service;
using WebApi.Entity;
using WebApi.Model;
using WebApi.Model.Permission;

namespace WebApi.Services.Interfaces
{
    public interface IPermissionService : IService<Permission, AppDbContext>
    {
        void SyncAsync();

        Task<List<PermissionResponse>> GetListAsync();

        Task<List<string>> GetCurrentUserPermissionCodesAsync(int? userId = null);
    }
}
