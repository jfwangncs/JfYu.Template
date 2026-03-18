using JfYu.Data.Service;
using WebApi.Entity;
using WebApi.Model;
using WebApi.Model.Permission;

namespace WebApi.Services.Interfaces
{
    public interface IPermissionService : IService<Permission, AppDbContext>
    {
        void SyncAsync();
        Task<PagedResult<PermissionResponse>> GetPagedAsync(QueryRequest query);
    }
}
