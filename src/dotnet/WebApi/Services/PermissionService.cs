using Microsoft.EntityFrameworkCore;
using WebApi.Entity;
using WebApi.Model.Request;
using WebApi.Services.Interfaces;

namespace WebApi.Services
{
    public class PermissionService(AppDbContext dbContext) : IPermissionService
    {
        private readonly AppDbContext _dbContext = dbContext;

        public async Task<List<Permission>> GetAllAsync()
        {
            return await _dbContext.Permissions.ToListAsync();
        }

        public async Task<Permission?> GetByIdAsync(long id)
        {
            return await _dbContext.Permissions.FindAsync(id);
        }

        public async Task<Permission> CreateAsync(CreatePermissionRequest request)
        {
            var permission = new Permission
            {
                Name = request.Name,
                Code = request.Code,
                Description = request.Description
            };

            _dbContext.Permissions.Add(permission);
            await _dbContext.SaveChangesAsync();
            return permission;
        }

        public async Task<Permission?> UpdateAsync(long id, UpdatePermissionRequest request)
        {
            var permission = await _dbContext.Permissions.FindAsync(id);
            if (permission == null)
                return null;

            permission.Name = request.Name;
            permission.Code = request.Code;
            permission.Description = request.Description;
            permission.IsActive = request.IsActive;

            await _dbContext.SaveChangesAsync();
            return permission;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var permission = await _dbContext.Permissions.FindAsync(id);
            if (permission == null)
                return false;

            _dbContext.Permissions.Remove(permission);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
