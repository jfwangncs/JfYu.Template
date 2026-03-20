using JfYu.Data.Context;
using JfYu.Data.Extension;
using JfYu.Data.Model;
using JfYu.Data.Service;
using Microsoft.EntityFrameworkCore;
using WebApi.Entity;
using WebApi.Model;
using WebApi.Services.Interfaces;

namespace WebApi.Services
{
    public class RoleService(AppDbContext context, ReadonlyDBContext<AppDbContext> readonlyDBContext) : Service<Role, AppDbContext>(context, readonlyDBContext), IRoleService
    {
        public async Task<PagedData<Role>> GetPagedAsync(QueryRequest query)
        {
            var q = _readonlyContext.Roles.Include(q => q.Permissions).AsQueryable();

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

            return await q.ToPagedAsync(query.PageIndex, query.PageSize);
        }

        public async Task AssignPermissionsAsync(int roleId, List<int> permissionIds)
        {
            var role = await _context.Roles
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == roleId);
            if (role == null) return;

            role.Permissions.Clear();
            if (permissionIds.Count > 0)
            {
                var permissions = await _context.Permissions
                    .Where(p => permissionIds.Contains(p.Id))
                    .ToListAsync();
                foreach (var p in permissions)
                    role.Permissions.Add(p);
            }
            await _context.SaveChangesAsync();
        }
    }
}
