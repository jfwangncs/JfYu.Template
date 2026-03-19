using JfYu.Data.Context;
using JfYu.Data.Extension;
using JfYu.Data.Service;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using WebApi.Attributes;
using WebApi.Constants;
using WebApi.Entity;
using WebApi.Model;
using WebApi.Model.Permission;
using WebApi.Services.Interfaces;

namespace WebApi.Services
{
    public class PermissionService(AppDbContext context, ReadonlyDBContext<AppDbContext> readonlyDBContext, ILogger<Permission> _logger) : Service<Permission, AppDbContext>(context, readonlyDBContext), IPermissionService
    {
        public void SyncAsync()
        {
            var scanned = ScanAttributes();
            var existingCodes = (_context.Permissions.Select(p => p.Code).ToList()).ToHashSet();
            _logger.LogInformation("Permission sync started, scanned {Count} attributes, {Existing} existing in db", scanned.Count, existingCodes.Count);

            var allParentCodes = scanned.Where(s => s.ParentCode != null).Select(s => s.ParentCode).OfType<string>().Distinct();

            foreach (var parentCode in allParentCodes)
            {
                if (existingCodes.Contains(parentCode))
                    continue;
                if (scanned.Any(s => s.Code == parentCode))
                    continue;
                try
                {
                    _context.Permissions.Add(new Permission
                    {
                        Code = parentCode,
                        Name = parentCode,
                        Type = PermissionType.Menu,
                    });
                    _context.SaveChanges();
                    existingCodes.Add(parentCode);
                }
                catch (DbUpdateException)
                {
                    _context.ChangeTracker.Clear();
                    existingCodes.Add(parentCode);
                }
            }

            foreach (var item in scanned)
            {
                if (existingCodes.Contains(item.Code))
                    continue;
                int? parentId = null;
                if (item.ParentCode != null)
                {
                    var parent = _context.Permissions.FirstOrDefault(p => p.Code == item.ParentCode);
                    parentId = parent?.Id;
                }

                try
                {
                    _context.Permissions.Add(new Permission
                    {
                        Code = item.Code,
                        Name = item.Code,
                        Type = item.Type,
                        ParentId = parentId,
                    });
                    _context.SaveChanges();
                    existingCodes.Add(item.Code);
                }
                catch (DbUpdateException)
                {
                    _context.ChangeTracker.Clear();
                    existingCodes.Add(item.Code);
                }
            }
            _logger.LogInformation("Permission sync completed");
        }
        private List<ScannedPermission> ScanAttributes()
        {
            var result = new List<ScannedPermission>();
            var controllers = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract);

            foreach (var ctrl in controllers)
            {
                // Menu
                var menuAttr = ctrl.GetCustomAttribute<PermissionAttribute>();
                if (menuAttr != null)
                {
                    result.Add(new ScannedPermission
                    {
                        Code = menuAttr.Code,
                        Type = menuAttr.Type,
                        ParentCode = menuAttr.ParentCode,
                    });
                }

                // Button 
                foreach (var method in ctrl.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    var btnAttr = method.GetCustomAttribute<PermissionAttribute>();
                    if (btnAttr != null)
                    {
                        result.Add(new ScannedPermission
                        {
                            Code = btnAttr.Code,
                            Type = btnAttr.Type,
                            ParentCode = btnAttr.ParentCode ?? menuAttr?.Code,
                        });
                    }
                }
            }

            // 去重
            return result
                .GroupBy(x => x.Code)
                .Select(g => g.First())
                .ToList();
        }

        public async Task<PagedResult<PermissionResponse>> GetPagedAsync(QueryRequest query)
        {
            var q = _readonlyContext.Permissions.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.SearchKey))
                q = q.Where(p => p.Name.Contains(query.SearchKey) || p.Code.Contains(query.SearchKey));

            if (query.Status.HasValue)
                q = q.Where(p => p.Status == query.Status.Value);

            if (query.StartTime.HasValue)
                q = q.Where(p => p.CreatedTime >= query.StartTime.Value);

            if (query.EndTime.HasValue)
                q = q.Where(p => p.CreatedTime <= query.EndTime.Value);

            q = q.OrderBy(p => p.Sort).ThenBy(p => p.Id);

            var paged = await q.ToPagedAsync(q => q.Adapt<IEnumerable<PermissionResponse>>(), query.PageIndex, query.PageSize);
            return new PagedResult<PermissionResponse>
            {
                Items = paged.Data?.ToList() ?? [],
                Total = paged.TotalCount
            };
        }
    }
}
