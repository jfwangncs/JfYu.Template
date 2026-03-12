using Mapster;
using WebApi.Entity;
using WebApi.Model.Request;
using WebApi.Model.Response;

namespace WebApi.Extensions
{
    public static class MapsterExtension
    {
        public static IServiceCollection AddMapster(this IServiceCollection services)
        {
            TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
            //#if (EnableRBAC)
            TypeAdapterConfig<UpdateRoleRequest, Role>.NewConfig().IgnoreNullValues(true);
            TypeAdapterConfig<UpdatePermissionRequest, Permission>.NewConfig().IgnoreNullValues(true);
            TypeAdapterConfig<UpdateUserRequest, User>.NewConfig().IgnoreNullValues(true);
            TypeAdapterConfig<User, UserResponse>.ForType().Map(q => q.Roles, q => q.Roles.Select(q => q.Name).ToList());
            //#endif
            return services;
        }
    }
}
