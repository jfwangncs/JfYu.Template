//#if (EnableRBAC)
using JfYu.Data.Extension;
using JfYu.WebApi.Template.Infrastructure;
//#endif
//#if (EnableJWTRedis)
using JfYu.Redis.Extensions;
//#endif
//#if (EnableWeChat)
using JfYu.WeChat;
//#endif
//#if (EnableJWT || EnableRBAC)
using JfYu.WebApi.Template.Services;
using JfYu.WebApi.Template.Services.Interfaces;
//#endif
//#if (EnableRBAC)
using JfYu.WebApi.Template.Entity;
using Microsoft.EntityFrameworkCore.Diagnostics;
//#endif

namespace JfYu.WebApi.Template.Extensions
{
    public static class InjectionExtension
    {
        public static IServiceCollection AddCustomInjection(this IServiceCollection services, IConfiguration configuration)
        {
            //#if (EnableJWT)
            services.AddScoped<IJwtService, JwtService>();
            //#endif

            //#if (EnableJWTRedis)
            services.AddRedisService(options => { configuration.GetSection("Redis").Bind(options); });
            services.AddScoped<IRedisCacheService, RedisCacheService>();
            //#endif

            //#if (EnableWeChat)
            services.AddMiniProgram(options => { configuration.GetSection("MiniProgram").Bind(options); });
            //#endif

            //#if (EnableRBAC)
            services.AddScoped<IInterceptor, AuditInterceptor>();
            services.AddJfYuDbContext<AppDbContext>(options =>
            {
                configuration.GetSection("ConnectionStrings").Bind(options);
            });
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<ILoginLogService, LoginLogService>();
            services.AddScoped<IDictTypeService, DictTypeService>();
            services.AddScoped<IDictItemService, DictItemService>();
            //#endif
            return services;
        }
    }
}
