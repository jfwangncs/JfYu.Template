using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using WebApi.Constants;
using WebApi.Extensions;
using WebApi.Model;

namespace WebApi.Attributes
{
    public class PermissionAttribute(string code, PermissionType type = PermissionType.Button, string? parentCode = null) : Attribute
    {
        public string Code { get; } = code;
        public PermissionType Type { get; } = type;
        public string? ParentCode { get; } = parentCode;

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
