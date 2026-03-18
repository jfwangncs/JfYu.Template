using JfYu.Data.Context;
using JfYu.Data.Extension;
using JfYu.Data.Service;
using WebApi.Entity;
using WebApi.Model;
using WebApi.Services.Interfaces;

namespace WebApi.Services
{
  public class RoleService(AppDbContext context, ReadonlyDBContext<AppDbContext> readonlyDBContext) : Service<Role, AppDbContext>(context, readonlyDBContext), IRoleService
  {
    public async Task<PagedResult<Role>> GetPagedAsync(QueryRequest query)
    {
      var q = _readonlyContext.Roles.AsQueryable();

      if (!string.IsNullOrWhiteSpace(query.SearchKey))
      {
        q = q.Where(r => r.Name.Contains(query.SearchKey) ||
            (r.Description != null && r.Description.Contains(query.SearchKey)));
      }

      if (query.Status.HasValue)
        q = q.Where(r => r.Status == query.Status.Value);

      if (query.StartTime.HasValue)
        q = q.Where(r => r.CreatedTime >= query.StartTime.Value);

      if (query.EndTime.HasValue)
        q = q.Where(r => r.CreatedTime <= query.EndTime.Value);

      var paged = await q.ToPagedAsync(query.PageIndex, query.PageSize);
      return new PagedResult<Role>
      {
        Items = paged.Data?.ToList() ?? [],
        Total = paged.TotalCount
      };
    }
  }
}
