//using System.Security.Claims;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.EntityFrameworkCore;
//using WebApi.Entity;

//namespace WebApi.Authorization
//{
//    public class PermissionHandler(AppDbContext dbContext) : AuthorizationHandler<PermissionRequirement>
//    {
//        private readonly AppDbContext _dbContext = dbContext;

//        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
//        {
//            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
//                return;

//            var hasPermission = await _dbContext.UserRoles
//                .Where(ur => ur.UserId == userId)
//                .SelectMany(ur => ur.Role.RolePermissions)
//                .AnyAsync(rp => rp.Permission.Code == requirement.PermissionCode);

//            if (hasPermission)
//                context.Succeed(requirement);
//        }
//    }
//}
