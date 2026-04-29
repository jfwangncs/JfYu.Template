using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
//#if (EnableRBAC)
using JfYu.WebApi.Template.Infrastructure;
//#endif

namespace JfYu.WebApi.Template.Entity
{
    public class AppDbContext(DbContextOptions<AppDbContext> options, IEnumerable<IInterceptor> interceptors) : DbContext(options)
    {
        private readonly IEnumerable<IInterceptor> _interceptors = interceptors;

        public DbSet<User> Users { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<Permission> Permissions { get; set; }

        //#if (EnableRBAC)
        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<LoginLog> LoginLogs { get; set; }

        public DbSet<DictType> DictTypes { get; set; }

        public DbSet<DictItem> DictItems { get; set; }
        //#endif

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            foreach (var interceptor in _interceptors)
                optionsBuilder.AddInterceptors(interceptor);
        }
    }
}
