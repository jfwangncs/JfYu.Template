using JfYu.Data.Service;
using JfYu.WebApi.Template.Entity;

namespace JfYu.WebApi.Template.Services.Interfaces
{
    public interface IPermissionService : IService<Permission, AppDbContext>
    {
        void SyncAsync();
    }
}
