using JfYu.Data.Context;
using JfYu.Data.Extension;
using JfYu.Data.Model;
using JfYu.Data.Service;
using JfYu.WebApi.Template.Entity;
using JfYu.WebApi.Template.Model.AuditLog;
using JfYu.WebApi.Template.Services.Interfaces;

namespace JfYu.WebApi.Template.Services
{
    public class AuditLogService(AppDbContext context, ReadonlyDBContext<AppDbContext> readonlyDBContext)
        : Service<AuditLog, AppDbContext>(context, readonlyDBContext), IAuditLogService
    {
        public async Task<PagedData<AuditLog>> GetPagedAsync(QueryAuditLogRequest query)
        {
            var q = _readonlyContext.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.SearchKey))
                q = q.Where(a => (a.UserName != null && a.UserName.Contains(query.SearchKey)) ||
                                  a.Resource.Contains(query.SearchKey));

            if (!string.IsNullOrWhiteSpace(query.Resource))
                q = q.Where(a => a.Resource == query.Resource);

            if (!string.IsNullOrWhiteSpace(query.Action))
                q = q.Where(a => a.Action == query.Action);

            if (query.UserId.HasValue)
                q = q.Where(a => a.UserId == query.UserId.Value);

            if (query.StartTime.HasValue)
                q = q.Where(a => a.CreatedTime >= query.StartTime.Value);

            if (query.EndTime.HasValue)
                q = q.Where(a => a.CreatedTime <= query.EndTime.Value);

            return await q.OrderByDescending(a => a.CreatedTime).ToPagedAsync(query.PageIndex, query.PageSize);
        }
    }
}
