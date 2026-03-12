using JfYu.Data.Extension;
using JfYu.Redis.Extensions;
using JfYu.WeChat;
using WebApi.Entity;
using WebApi.Services;
using WebApi.Services.Interfaces;

namespace WebApi.Extensions
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
            //#endif

            //#if (EnableWeChat)
            services.AddMiniProgram(options => { configuration.GetSection("MiniProgram").Bind(options); });
            //#endif

            //#if (EnableRBAC)
            services.AddJfYuDbContext<AppDbContext>(options =>
            {
                configuration.GetSection("ConnectionStrings").Bind(options);
            });
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IUserService, UserService>();
            //#endif
            return services;
        }
    }
}
