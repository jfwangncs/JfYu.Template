using JfYu.Data.Service;
using WebApi.Entity;

namespace WebApi.Services.Interfaces
{
    public interface IPermissionService : IService<Permission, AppDbContext>
    {
        void SyncAsync();
    }
}
