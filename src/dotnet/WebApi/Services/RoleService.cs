using JfYu.Data.Context;
using JfYu.Data.Extension;
using JfYu.Data.Model;
using JfYu.Data.Service;
using Microsoft.EntityFrameworkCore;
using WebApi.Entity;
using WebApi.Model.Request;
using WebApi.Services.Interfaces;

namespace WebApi.Services
{
    public class RoleService(AppDbContext context, ReadonlyDBContext<AppDbContext> readonlyDBContext) : Service<Role, AppDbContext>(context, readonlyDBContext), IRoleService
    {
        public async Task<PagedData<Role>> GetPagedAsync(QueryRequest query)
        {
            var q = _readonlyContext.Roles.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.SearchKey))
            {
                q = q.Where(r => r.Name.Contains(query.SearchKey) ||
                    (r.Description != null && r.Description.Contains(query.SearchKey)));
            }

            return await q.ToPagedAsync(query.PageIndex, query.PageSize);
        }
    }
}
