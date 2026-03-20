using Mapster;
using JfYu.Data.Model;
using WebApi.Entity;
using WebApi.Model;
using WebApi.Model.Request;
using WebApi.Model.Response;
using WebApi.Model.Role;

namespace WebApi.Extensions
{
    public static class MapsterExtension
    {
        public static IServiceCollection AddMapster(this IServiceCollection services)
        {
            TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
            TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);
            TypeAdapterConfig.GlobalSettings.ForType(typeof(PagedData<>), typeof(PagedResult<>))
                .Map("Items", "Data")
                .Map("Total", "TotalCount");
            //#if (EnableRBAC) 
            TypeAdapterConfig<UpdateRoleRequest, Role>.NewConfig().IgnoreNullValues(true);
            TypeAdapterConfig<UpdateUserRequest, User>.NewConfig().IgnoreNullValues(true);
            TypeAdapterConfig<User, UserResponse>.ForType().Map(q => q.Roles, q => q.Roles.SelectMany(r => r.Permissions).Select(p => p.Code).ToList())
            .Map(q => q.RoleList, q => q.Roles.Select(r => r.Adapt<RoleResponse>()).ToList());
            //#endif
            return services;
        }
    }
}
