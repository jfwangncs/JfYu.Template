using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using JfYu.WebApi.Template.Constants;
using JfYu.WebApi.Template.Extensions;
using JfYu.WebApi.Template.Model;
//#if (EnableRBAC)
using JfYu.WebApi.Template.Entity;
//#endif
//#if (EnableJWTRedis)
using JfYu.Redis.Interface;
//#endif

namespace JfYu.WebApi.Template.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class PermissionAttribute(string code, PermissionType type = PermissionType.Button, string? parentCode = null) : Attribute, IAsyncAuthorizationFilter
    {
        public string Code { get; } = code;
        public PermissionType Type { get; } = type;
        public string? ParentCode { get; } = parentCode;

        private static readonly TimeSpan PermissionCacheTtl = TimeSpan.FromMinutes(30);

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // skip [AllowAnonymous]
            if (context.ActionDescriptor.EndpointMetadata.Any(m => m is AllowAnonymousAttribute))
                return;

            var user = context.HttpContext.User;

            if (user?.Identity?.IsAuthenticated != true)
            {
                context.Result = new ObjectResult(new BaseResponse<string>
                {
                    Code = ResponseCode.Error,
                    Message = ErrorCode.UnauthorizedError.GetDescription(),
                    ErrorCode = ErrorCode.UnauthorizedError,
                })
                { StatusCode = 401 };
                return;
            }

            if (Type == PermissionType.Menu)
                return;

            var permissions = await GetPermissionsAsync(context.HttpContext, user);

            if (!permissions.Contains(Code))
            {
                context.Result = new ObjectResult(new BaseResponse<string>
                {
                    Code = ResponseCode.Error,
                    Message = ErrorCode.ForbiddenError.GetDescription(),
                    ErrorCode = ErrorCode.ForbiddenError,
                })
                { StatusCode = 403 };
            }
        }

        private static async Task<HashSet<string>> GetPermissionsAsync(HttpContext httpContext, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            //#if (EnableJWTRedis)
            var redis = httpContext.RequestServices.GetService<IRedisService>();
            if (redis != null && userId != null)
            {
                var cacheKey = string.Format(RedisKey.UserPermission, userId);
                var cached = await redis.GetAsync<List<string>>(cacheKey);
                if (cached != null)
                    return [.. cached];

                //#if (EnableRBAC)
                var db = httpContext.RequestServices.GetService<AppDbContext>();
                if (db != null && int.TryParse(userId, out var uid))
                {
                    var dbPerms = await db.Users
                        .Where(u => u.Id == uid)
                        .SelectMany(u => u.Roles)
                        .SelectMany(r => r.Permissions)
                        .Where(p => p.Status == 1 && p.Type == PermissionType.Button)
                        .Select(p => p.Code)
                        .Distinct()
                        .ToListAsync();

                    await redis.AddAsync(cacheKey, dbPerms, PermissionCacheTtl);
                    return [.. dbPerms];
                }
                //#endif
            }
            //#endif

            // fallback: read from JWT claims
            return user.Claims
                .Where(c => c.Type == "permission")
                .Select(c => c.Value)
                .ToHashSet();
        }
    }
}
