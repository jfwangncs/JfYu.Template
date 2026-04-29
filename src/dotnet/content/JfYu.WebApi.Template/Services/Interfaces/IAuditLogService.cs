using JfYu.Data.Model;
using JfYu.Data.Service;
using JfYu.WebApi.Template.Entity;
using JfYu.WebApi.Template.Model.AuditLog;

namespace JfYu.WebApi.Template.Services.Interfaces
{
    public interface IAuditLogService : IService<AuditLog, AppDbContext>
    {
        Task<PagedData<AuditLog>> GetPagedAsync(QueryAuditLogRequest query);
    }
}
