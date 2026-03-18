using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using WebApi.Constants;
using WebApi.Extensions;
using WebApi.Model;

namespace WebApi.Attributes
{
    public class PermissionAttribute : Attribute
    {
        public string Code { get; }
        public string Name { get; }
        public PermissionType Type { get; }
        public string? ParentCode { get; }
        public string? ParentName { get; }
        public string? Icon { get; }
        public int Sort { get; }

        public PermissionAttribute(
            string code,
            string name,
            PermissionType type = PermissionType.Button,
            string? parentCode = null,
            string? parentName = null,
            string? icon = null,
            int sort = 0)
        {
            Code = code;
            Name = name;
            Type = type;
            ParentCode = parentCode;
            ParentName = parentName;
            Icon = icon;
            Sort = sort;
        }
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // skip [AllowAnonymous]
            if (context.ActionDescriptor.EndpointMetadata.Any(m => m is AllowAnonymousAttribute))
                return;            

            var user = context.HttpContext.User;
            var options = context.HttpContext.RequestServices
                .GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;

            if (user?.Identity?.IsAuthenticated != true)
            {
                context.HttpContext.Response.StatusCode = 401;
                await context.HttpContext.Response.WriteAsJsonAsync(new BaseResponse<string>
                {
                    Code = ResponseCode.Error,
                    Message = ErrorCode.UnauthorizedError.GetDescription(),
                    ErrorCode = ErrorCode.UnauthorizedError,
                }, options);
                return;
            }

            if (Type == PermissionType.Menu)
                return;

            var permissions = user.Claims
                .Where(c => c.Type == "permission")
                .Select(c => c.Value)
                .ToHashSet();

            if (!permissions.Contains(Code))
            {
                context.HttpContext.Response.StatusCode = 403;
                await context.HttpContext.Response.WriteAsJsonAsync(new BaseResponse<string>
                {
                    Code = ResponseCode.Error,
                    Message = ErrorCode.NoPermission.GetDescription(),
                    ErrorCode = ErrorCode.NoPermission,
                }, options);
            }
        }

    }
}
