using System.Security.Claims;
using WebApi.Services.Interfaces;

namespace WebApi.Services
{
    public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        private ClaimsPrincipal? User
            => _httpContextAccessor.HttpContext?.User;
         
        private bool IsAuth
            => User?.Identity?.IsAuthenticated ?? false;

        public bool IsAuthenticated => IsAuth;         

        public int? Id
            => IsAuth && int.TryParse(
                   User!.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
               ? id : null;

        public string? Username
            => IsAuth ? User!.FindFirstValue(ClaimTypes.Name) : null;
 

        public IReadOnlyList<string> Roles
            => IsAuth
               ? [.. User!.FindAll(ClaimTypes.Role).Select(c => c.Value)]
               : new List<string>();

        public IReadOnlyList<string> Permissions
            => IsAuth
               ? [.. User!.FindAll("permission").Select(c => c.Value)]
               : new List<string>();
         
    }
}
