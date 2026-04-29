using Mapster;
//#if (EnableRBAC)
using JfYu.Data.Model;
using JfYu.WebApi.Template.Entity;
using JfYu.WebApi.Template.Model;
using JfYu.WebApi.Template.Model.Request;
using JfYu.WebApi.Template.Model.Response;
using JfYu.WebApi.Template.Model.Role;
//#endif

namespace JfYu.WebApi.Template.Extensions
{
    public static class MapsterExtension
    {
        public static IServiceCollection AddMapster(this IServiceCollection services)
        {
            TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
            TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);
            //#if (EnableRBAC)
            TypeAdapterConfig.GlobalSettings.ForType(typeof(PagedData<>), typeof(PagedResult<>))
                .Map("Items", "Data")
                .Map("Total", "TotalCount");
            TypeAdapterConfig<UpdateRoleRequest, Role>.NewConfig().IgnoreNullValues(true);
            TypeAdapterConfig<UpdateUserRequest, User>.NewConfig().IgnoreNullValues(true);
            TypeAdapterConfig<User, UserResponse>.ForType().Map(q => q.Roles, q => q.Roles.Where(r => r.Status == (int)DataStatus.Active).SelectMany(r => r.Permissions).Where(p => p.Status == (int)DataStatus.Active).Select(p => p.Code).Distinct().ToList())
            .Map(q => q.RoleList, q => q.Roles.Select(r => r.Adapt<RoleResponse>()).ToList());
            //#endif
            return services;
        }
    }
}
