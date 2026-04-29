using JfYu.Data.Model;
using JfYu.Data.Service;
using JfYu.WebApi.Template.Entity;
using JfYu.WebApi.Template.Model.LoginLog;

namespace JfYu.WebApi.Template.Services.Interfaces
{
    public interface ILoginLogService : IService<LoginLog, AppDbContext>
    {
        Task<PagedData<LoginLog>> GetPagedAsync(QueryLoginLogRequest query);
    }
}